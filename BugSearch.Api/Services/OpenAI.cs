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
        IConfiguration configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();
        try
        {
            string url = "https://api.openai.com/v1/chat/completions";

            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {Environment.GetEnvironmentVariable("OPENAI_KEY") ?? System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(configuration.GetSection("OpenAi:Key").Value ?? string.Empty))}");

            var requestBody = new OpenAIPromptRequest
            {
                model = "gpt-3.5-turbo",
                temperature = 0,
                messages = new List<Message>
            {
                new Message
                {
                    role = "user",
                    content = "Sempre que eu for pesquisar algo, quero que você me ajude respondendo as perguntas que eu te fizer iniciando as frase educadamente dando saudações ao usuário sendo que seu nome é 'BugSearch' e responda com o que você acha que eu devo saber sobre oque eu escrevi ou perguntei."
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