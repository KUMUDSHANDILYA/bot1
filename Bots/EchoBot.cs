using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace NL2SQLBotFrameworkBot.Bots
{
    public class EchoBot : ActivityHandler
    {
        private readonly HttpClient _httpClient;

        public EchoBot(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            string userInput = turnContext.Activity.Text;
            var requestBody = new { question = userInput };
            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            string apiUrl = "https://nltosqlkumud-etb3a7dpc5gdcwhj.canadacentral-01.azurewebsites.net/api/nl2sql";

            try
            {
                HttpResponseMessage response = await _httpClient.PostAsync(apiUrl, content, cancellationToken);
                response.EnsureSuccessStatusCode();
                string responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                await turnContext.SendActivityAsync($"API Response: {responseContent}", cancellationToken: cancellationToken);
            }
            catch (HttpRequestException ex)
            {
                await turnContext.SendActivityAsync($"Error calling API: {ex.Message}", cancellationToken: cancellationToken);
            }
        }
    }
}