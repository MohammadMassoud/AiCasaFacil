using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace AiCasaFacil.Infrastructure.AI;

public class OllamaService
{
    private readonly HttpClient _httpClient;
    private readonly OllamaSettings _settings;

    public OllamaService(HttpClient httpClient, IOptions<OllamaSettings> settings)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
    }

    public async Task<string> GenerateAsync(string pergunta)
    {
        var requestBody = new
        {
            model = _settings.Model,
            prompt = pergunta,
            stream = false,
            Options = new
            {
                temperature = _settings.Temperature
            }
            
        };

        var json = JsonSerializer.Serialize(requestBody);

        var response = await _httpClient.PostAsync(
            $"{_settings.BaseUrl}/api/generate",
            new StringContent(json, Encoding.UTF8, "application/json")
        );

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(content);
        return doc.RootElement.GetProperty("response").GetString() ?? "";
    }
}
