using System.Net;
using System.Text.Json;

public static class RiotApiManager
{
    private static readonly HttpClient _webClient = new HttpClient();
    private static string ApiKey;

    private static Dictionary<string, string> Paths = new Dictionary<string, string>{
      {"contents", "/content/v1/contents"}
    };

    static RiotApiManager()
    {
        ApiKey = File.ReadAllText("./tokens/riot-api-key.txt");
    }

    public async static Task UpdateContent()
    {
        RiotApiSettings jsonSettings = JsonSerializer.Deserialize<RiotApiSettings>(File.ReadAllText("./data/RiotApi.json"))!;

        if (jsonSettings.LastUpdate is DateTime && jsonSettings.LastUpdate > DateTime.Now.AddDays(-2))
        {
            RiotValorantContent.Instance = JsonSerializer.Deserialize<RiotValorantContent>(
                File.ReadAllText("./data/ValorantContents.json"),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

            await RiotValorantContent.Instance!.UpdateAgentData();
            return;
        }

        var BR_Content = await RiotApiManager.Get("contents", "locale=pt-BR");

        if (BR_Content is string && !string.IsNullOrWhiteSpace(BR_Content))
        {
            File.WriteAllText("./data/ValorantContents.json", BR_Content);

            File.WriteAllText("./data/RiotApi.json", JsonSerializer.Serialize<RiotApiSettings>(new RiotApiSettings
            {
                LastUpdate = DateTime.Now
            }));

            RiotValorantContent.Instance = JsonSerializer.Deserialize<RiotValorantContent>(
                BR_Content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        }

        await RiotValorantContent.Instance!.UpdateAgentData();
        await UpdateAgentDisplayIcons();
    }



    private static async Task UpdateAgentDisplayIcons()
    {
        var agents = RiotValorantContent.Instance?.GetAgents();
        if (agents == null || agents.Count < 1)
        {
            return;
        }

        foreach (var agent in agents)
        {
            if (!File.Exists(agent.IconPath))
            {
                var agentData = await GetAgentIconFromValorantAPI(agent.Id);
                if (agentData != null)
                {
                    using (var fileStream = new FileStream(agent.IconPath, FileMode.Create, FileAccess.Write))
                    {
                        agentData.CopyTo(fileStream);
                        fileStream.Dispose();
                    }
                }
            }
        }
    }


    private async static Task<Stream?> GetAgentIconFromValorantAPI(string agentId)
    {
        var builder = new UriBuilder($@"https://media.valorant-api.com/agents/{agentId.ToLower()}/displayicon.png");

        HttpResponseMessage response = await _webClient.GetAsync(builder.Uri);
        if (response.IsSuccessStatusCode)
        {
            return response.Content.ReadAsStream();
        }
        else
        {
            return null;
        }
    }

    private async static Task<string> Get(string path, string parameters)
    {
        if (Paths.TryGetValue(path, out var clientPath))
        {
            var builder = new UriBuilder($@"https://br.api.riotgames.com/val{clientPath}");

            parameters = !string.IsNullOrWhiteSpace(parameters) && !string.IsNullOrEmpty(parameters) ? $"&{parameters}" : "";
            builder.Query = $"api_key={ApiKey}{parameters}";

            var httpRequestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = builder.Uri,
                Headers = {
                    {"User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/103.0.0.0 Safari/537.36"},
                    { "Accept-Language", "pt-BR,pt;q=0.9,en-US;q=0.8,en;q=0.7"},
                    { "Origin", "https://developer.riotgames.com"}
                }
            };

            HttpResponseMessage response = await _webClient.SendAsync(httpRequestMessage);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
        }

        return "";
    }
}