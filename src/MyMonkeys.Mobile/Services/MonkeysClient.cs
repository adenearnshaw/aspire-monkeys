using System.Net.Http.Json;
using MyMonkeys.Mobile.Models;

namespace MyMonkeys.Mobile.Services;

public sealed class MonkeysClient
{
    private readonly HttpClient _http;

    public MonkeysClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<IReadOnlyList<Monkey>> GetMonkeysAsync(CancellationToken cancellationToken = default)
    {
        return await _http.GetFromJsonAsync<IReadOnlyList<Monkey>>("/api/monkeys", cancellationToken)
            ?? Array.Empty<Monkey>();
    }

    public async Task<Monkey?> GetMonkeyAsync(string id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return null;
        }

        return await _http.GetFromJsonAsync<Monkey>($"/api/monkeys/{Uri.EscapeDataString(id)}", cancellationToken);
    }
}
