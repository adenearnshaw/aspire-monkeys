using System.Net;
using Microsoft.Extensions.Caching.Memory;
using MyMonkeys.WebBff;
using SkiaSharp;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddOpenApi();

builder.Services.AddHttpClient("monkeys", client =>
{
    client.BaseAddress = new Uri("https://monkeys-service");
});

builder.Services.AddMemoryCache();

builder.Services.AddHttpClient("osm-tiles", client =>
{
    client.BaseAddress = new Uri("https://tile.openstreetmap.org/");
    client.DefaultRequestHeaders.UserAgent.ParseAdd("MyMonkeys.WebBff/1.0 (+https://localhost)");
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("dev", policy =>
        policy.WithOrigins("https://localhost:6022")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

if (app.Environment.IsDevelopment())
{
    app.UseCors("dev");
}

app.MapGet("/api/monkeys", async (IHttpClientFactory httpClientFactory, CancellationToken cancellationToken) =>
{
    var client = httpClientFactory.CreateClient("monkeys");
    var monkeys = await client.GetFromJsonAsync<IReadOnlyList<MonkeyDto>>("/monkeys", cancellationToken);
    return monkeys is null ? Results.Problem("Upstream returned no data") : Results.Ok(monkeys);
})
.WithName("GetMonkeys");

app.MapGet("/api/monkeys/{id}", async (string id, IHttpClientFactory httpClientFactory, CancellationToken cancellationToken) =>
{
    var client = httpClientFactory.CreateClient("monkeys");
    using var upstream = await client.GetAsync($"/monkeys/{Uri.EscapeDataString(id)}", cancellationToken);

    if (upstream.StatusCode == HttpStatusCode.NotFound)
    {
        return Results.NotFound();
    }

    upstream.EnsureSuccessStatusCode();
    var monkey = await upstream.Content.ReadFromJsonAsync<MonkeyDto>(cancellationToken: cancellationToken);
    return monkey is null ? Results.Problem("Upstream returned no data") : Results.Ok(monkey);
})
.WithName("GetMonkeyById");

app.MapGet("/api/map", async (
    HttpContext http,
    double lat,
    double lon,
    int zoom,
    int width,
    int height,
    IHttpClientFactory httpClientFactory,
    IMemoryCache cache,
    CancellationToken cancellationToken) =>
{
    if (!double.IsFinite(lat) || !double.IsFinite(lon))
    {
        return Results.BadRequest("Invalid lat/lon");
    }

    zoom = Math.Clamp(zoom, 0, 19);
    width = Math.Clamp(width, 64, 1024);
    height = Math.Clamp(height, 64, 1024);

    // Web Mercator tile math
    static (double x, double y) LatLonToTileXY(double latitude, double longitude, int z)
    {
        var latRad = latitude * Math.PI / 180.0;
        var n = Math.Pow(2.0, z);
        var x = (longitude + 180.0) / 360.0 * n;
        var y = (1.0 - Math.Log(Math.Tan(latRad) + 1.0 / Math.Cos(latRad)) / Math.PI) / 2.0 * n;
        return (x, y);
    }

    static int FloorDiv(double value, int divisor)
        => (int)Math.Floor(value / divisor);

    var (tileX, tileY) = LatLonToTileXY(lat, lon, zoom);
    var centerPxX = tileX * 256.0;
    var centerPxY = tileY * 256.0;
    var topLeftPxX = centerPxX - width / 2.0;
    var topLeftPxY = centerPxY - height / 2.0;

    var minTileX = FloorDiv(topLeftPxX, 256);
    var minTileY = FloorDiv(topLeftPxY, 256);
    var maxTileX = FloorDiv(topLeftPxX + width - 1, 256);
    var maxTileY = FloorDiv(topLeftPxY + height - 1, 256);

    var nTiles = 1 << zoom;

    int WrapX(int x)
    {
        var wrapped = x % nTiles;
        return wrapped < 0 ? wrapped + nTiles : wrapped;
    }

    int ClampY(int y)
        => Math.Clamp(y, 0, nTiles - 1);

    var tilesClient = httpClientFactory.CreateClient("osm-tiles");

    async Task<byte[]?> GetTileAsync(int x, int y)
    {
        x = WrapX(x);
        y = ClampY(y);

        var cacheKey = $"osm:{zoom}:{x}:{y}";
        if (cache.TryGetValue(cacheKey, out byte[]? cachedBytes) && cachedBytes is not null)
        {
            return cachedBytes;
        }

        using var response = await tilesClient.GetAsync($"{zoom}/{x}/{y}.png", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        cache.Set(cacheKey, bytes, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(6)
        });
        return bytes;
    }

    http.Response.Headers.CacheControl = "public, max-age=3600";

    using var surface = SKSurface.Create(new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul));
    var canvas = surface.Canvas;
    canvas.Clear(SKColors.Transparent);

    for (var y = minTileY; y <= maxTileY; y++)
    {
        for (var x = minTileX; x <= maxTileX; x++)
        {
            var bytes = await GetTileAsync(x, y);
            if (bytes is null)
            {
                continue;
            }

            using var bitmap = SKBitmap.Decode(bytes);
            if (bitmap is null)
            {
                continue;
            }

            var destX = (float)(x * 256.0 - topLeftPxX);
            var destY = (float)(y * 256.0 - topLeftPxY);
            canvas.DrawBitmap(bitmap, destX, destY);
        }
    }

    // Simple marker (red dot with white ring)
    var cx = width / 2f;
    var cy = height / 2f;
    using var fill = new SKPaint { Color = new SKColor(220, 38, 38, 230), IsAntialias = true, Style = SKPaintStyle.Fill };
    using var stroke = new SKPaint { Color = new SKColor(255, 255, 255, 235), IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 3 };
    canvas.DrawCircle(cx, cy, 9, stroke);
    canvas.DrawCircle(cx, cy, 7, fill);

    using var image = surface.Snapshot();
    using var encoded = image.Encode(SKEncodedImageFormat.Png, 90);
    return Results.File(encoded.ToArray(), "image/png");
})
.WithName("GetStaticMap");

app.Run();