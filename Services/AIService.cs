using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace download_redirector.Services;

public interface IAIProvider
{
    Task<string> GenerateFileNameAsync(string originalName, string prompt);
}

public static class AIServiceFactory
{
    public static IAIProvider? GetProvider(string providerName)
    {
        string apiKey = CryptoHelper.GetDecryptedKey(providerName);
        if (string.IsNullOrWhiteSpace(apiKey)) return null;

        if (providerName == "Gemini") return new GeminiProvider(apiKey);
        if (providerName == "ChatGPT") return new OpenAIProvider(apiKey);
        
        return null;
    }
}

public class GeminiProvider : IAIProvider
{
    private readonly string _apiKey;
    private static readonly HttpClient _httpClient = new();

    public GeminiProvider(string apiKey)
    {
        _apiKey = apiKey;
    }

    public async Task<string> GenerateFileNameAsync(string originalName, string prompt)
    {
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={_apiKey}";
        
        var systemInstruction = "Você é um assistente de sistema operacional. Responda APENAS com o nome final do arquivo sugerido, sem extensões, sem aspas, sem quebras de linha e sem explicações.";
        var userMessage = $"Arquivo Original: {originalName}\nRegras: {prompt}";

        var requestBody = new
        {
            contents = new[] { new { parts = new[] { new { text = userMessage } } } },
            systemInstruction = new { parts = new[] { new { text = systemInstruction } } },
            generationConfig = new { temperature = 0.2 }
        };

        var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(url, content);
        
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Gemini API Error: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
        }

        var jsonResponse = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(jsonResponse);
        var text = doc.RootElement.GetProperty("candidates")[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString();
        
        return SanitizeFileName(text ?? originalName);
    }

    private string SanitizeFileName(string name)
    {
        name = name.Trim().Trim('"', '\'', '`', '\n', '\r');
        foreach (var c in System.IO.Path.GetInvalidFileNameChars())
        {
            name = name.Replace(c.ToString(), "");
        }
        return name;
    }
}

public class OpenAIProvider : IAIProvider
{
    private readonly string _apiKey;
    private static readonly HttpClient _httpClient = new();

    public OpenAIProvider(string apiKey)
    {
        _apiKey = apiKey;
    }

    public async Task<string> GenerateFileNameAsync(string originalName, string prompt)
    {
        var url = "https://api.openai.com/v1/chat/completions";
        
        var requestBody = new
        {
            model = "gpt-4o-mini",
            messages = new[]
            {
                new { role = "system", content = "Você é um assistente de sistema operacional. Responda APENAS com o nome final do arquivo sugerido, sem extensões, sem aspas e sem explicações." },
                new { role = "user", content = $"Arquivo Original: {originalName}\nRegras: {prompt}" }
            },
            temperature = 0.2
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request);
        
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"OpenAI API Error: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
        }

        var jsonResponse = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(jsonResponse);
        var text = doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
        
        return SanitizeFileName(text ?? originalName);
    }

    private string SanitizeFileName(string name)
    {
        name = name.Trim().Trim('"', '\'', '`', '\n', '\r');
        foreach (var c in System.IO.Path.GetInvalidFileNameChars())
        {
            name = name.Replace(c.ToString(), "");
        }
        return name;
    }
}
