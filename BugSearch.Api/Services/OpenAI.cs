using System.Text;
using System.Text.Json;
using BugSearch.Api.Models;
using BugSearch.Shared.Services;

namespace BugSearch.Api.Services;

public class OpenAI
{
    private static readonly HttpClient client = new HttpClient();

    public static async Task<string> PromptSearch(string query)
    {
        try
        {
            string url = "https://api.openai.com/v1/chat/completions";

            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {EnvironmentService.GetValue("OPENAI_KEY")}");

            var requestBody = new OpenAIPromptRequest
            {
                model = "gpt-3.5-turbo",
                temperature = 0.9,
                messages = new List<Message>
            {
                new Message
                {
                    role = "assistant",
                    content = "Olá, eu sou o BugSearchBot, como posso te ajudar?"
                },
                new Message
                {
                    role = "user",
                    content = query
                }
            }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(url, content);
            var responseContent = await response.Content.ReadFromJsonAsync<OpenAIPromptResponse>();

            if (responseContent == null || responseContent.choices is null)
            {
                return "Não entendi, pode repetir?";
            }

            return responseContent.choices[0].message?.content ?? "Não entendi, pode repetir?";
        }
        catch (Exception ex)
        {
            return ex.ToString();
        }
    }
}