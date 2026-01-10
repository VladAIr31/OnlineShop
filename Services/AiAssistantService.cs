using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using OnlineShop.Models;

namespace OnlineShop.Services
{
    public interface IAiAssistantService
    {
        Task<string> GetAnswerAsync(string question, Product product);
    }

    public class GeminiAiService : IAiAssistantService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private const string ModelId = "gemini-2.5-flash-lite";

        public GeminiAiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<string> GetAnswerAsync(string question, Product product)
        {
            var apiKey = _configuration["Gemini:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                return "AI Service is not configured (API Key missing). Please check settings.";
            }

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{ModelId}:generateContent?key={apiKey}";

            // Construct prompt
            var systemInstruction = "Esti un asistent virtual util pentru un magazin online. " +
                                  "Raspunde la intrebarile utilizatorului despre produs bazandu-te DOAR pe detaliile furnizate. " +
                                  "Daca raspunsul nu se afla in detalii, spune 'Nu am aceasta informatie'. " +
                                  "Raspunde intotdeauna in limba Romana. Pastreaza raspunsurile concise si politicoase (max 2-3 fraze).";

            var productContext = $"Product: {product.Title}\n" +
                                 $"Price: {product.Price} RON\n" +
                                 $"Rating: {product.Rating}/5\n" +
                                 $"Stock: {product.Stock}\n" +
                                 $"Description: {product.Description}";

            var userPrompt = $"Context:\n{productContext}\n\nUser Question: {question}";

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = systemInstruction + "\n\n" + userPrompt }
                        }
                    }
                }
            };

            var jsonContent = new StringContent( JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync(url, jsonContent);
                var responseString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                   // Fallback or error handling
                return "Sunt momentan suprasolicitat. Te rog incearca mai tarziu.";
                }

                var jsonNode = JsonNode.Parse(responseString);
                var aiText = jsonNode?["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString();

                return aiText ?? "Nu am putut genera un raspuns.";
            }
            catch (Exception)
            {
                return "Eroare de conexiune. Te rog incearca din nou.";
            }
        }
    }
}
