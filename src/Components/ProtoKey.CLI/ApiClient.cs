using System.Net.Http.Json;

namespace ProtoKey.CLI;

public class ApiClient(HttpClient http)
{
    public async Task Set(string key, int value)
    {
        HttpResponseMessage response = await http.PutAsJsonAsync($"/api/keys/{key}", value);
        response.EnsureSuccessStatusCode();
    }

    public async Task<int> Get(string key)
    {
        HttpResponseMessage response = await http.GetAsync($"/api/keys/{key}");
        response.EnsureSuccessStatusCode();

        GetResponse? result = await response.Content.ReadFromJsonAsync<GetResponse>();
        return result!.Value;
    }

    public async Task<string[]> Keys(string prefix)
    {
        HttpResponseMessage response = await http.GetAsync($"/api/keys?prefix={Uri.EscapeDataString(prefix)}");
        response.EnsureSuccessStatusCode();

        string[]? result = await response.Content.ReadFromJsonAsync<string[]>();
        return result!;
    }

    private record GetResponse(int Value);
}