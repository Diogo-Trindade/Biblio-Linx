using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;

namespace BiblioLinx.Services;

public class GroqApiService : IGroqApiService
{
    private readonly string _url = "https://api.groq.com/openai/v1/chat/completions";
    
    // A sua chave de backup original (caso queira usá-la)
   private readonly string _chaveUniversal = "";

    private string ApiKey 
    {
        get
        {
            var chaveDoCliente = Preferences.Get("GroqApiKey", string.Empty);
            return string.IsNullOrWhiteSpace(chaveDoCliente) ? _chaveUniversal : chaveDoCliente;
        }
    }

    public async Task<string> ResumirCasoAsync(string chatCliente, string baseDeConhecimento)
    {
        try
        {
            string systemInstruction = $@"Você é um assistente de suporte técnico. 
Sua tarefa é ler o chat do cliente e resumi-lo de forma clara, extraindo os dados do cliente, o problema e a possível solução com base nesta base de conhecimento:
{baseDeConhecimento}";

            var payload = new { model = "llama-3.3-70b-versatile", messages = new[] { new { role = "system", content = systemInstruction }, new { role = "user", content = chatCliente } } };
            var options = new JsonSerializerOptions { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
            
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {ApiKey}");

            var jsonContent = new StringContent(JsonSerializer.Serialize(payload, options), Encoding.UTF8, "application/json");
            var response = await client.PostAsync(_url, jsonContent);
            var responseJson = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode) return $"Erro na API: {response.StatusCode}";

            using var doc = JsonDocument.Parse(responseJson);
            return doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? "Sem resposta gerada.";
        }
        catch (Exception ex) { return $"Erro ao processar a IA: {ex.Message}"; }
    }

    public async Task<string> ResponderChatAsync(string mensagemUsuario, string baseDeConhecimento)
    {
        try
        {
            string systemInstruction = $@"Você é o assistente virtual do sistema BiblioLinx.
Forneça a solução baseada EXCLUSIVAMENTE na base de conhecimento abaixo:
{baseDeConhecimento}
Seja direto, objetivo e claro. Responda em português."; 
            
            var payload = new { model = "llama-3.3-70b-versatile", messages = new[] { new { role = "system", content = systemInstruction }, new { role = "user", content = mensagemUsuario } } };
            var options = new JsonSerializerOptions { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
            
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {ApiKey}");

            var jsonContent = new StringContent(JsonSerializer.Serialize(payload, options), Encoding.UTF8, "application/json");
            var response = await client.PostAsync(_url, jsonContent);
            var responseJson = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode) return $"Erro na API: {response.StatusCode}";

            using var doc = JsonDocument.Parse(responseJson);
            return doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? "Sem resposta gerada.";
        }
        catch (Exception ex) { return $"Erro ao processar a IA: {ex.Message}"; }
    }
}