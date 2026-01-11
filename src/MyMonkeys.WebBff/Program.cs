using System.Net;
using MyMonkeys.WebBff;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddOpenApi();

builder.Services.AddHttpClient("monkeys", client =>
{
    client.BaseAddress = new Uri("https://monkeys-service");
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

app.Run();