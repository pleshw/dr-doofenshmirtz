using System.Text.Json;
using System.Text.Json.Serialization;

[Serializable]
public class ValorantEntity
{

    public string Name { get; }
    public string Id { get; }
    public string AssetName { get; }

    [JsonIgnore]
    public string JSONPath
    {
        get
        {
            return $"./assets/agents/{Name.Replace(@"/", "")}.json";
        }
    }

    [JsonIgnore]
    public string? Role;

    [JsonIgnore]
    public List<string> Tags = new List<string> { };

    [JsonIgnore]
    public List<string> ColorScheme = new List<string> { };

    [JsonIgnore]
    public string IconPath
    {
        get
        {
            return $"./assets/agents/{Name.Replace(@"/", "")}_icon.png";
        }
    }

    [JsonIgnore]
    public string WebIcon
    {
        get
        {
            return $@"https://media.valorant-api.com/agents/{Id.ToLower()}/displayicon.png";
        }
    }


    [JsonConstructor]
    public ValorantEntity(string name, string id, string assetName)
    {
        (Name, Id, AssetName) = (name, id, assetName);
    }

    public async Task UpdateDataFromAgentJSON()
    {
        try
        {
            var jsonAgentSettings = JsonSerializer.Deserialize<Dictionary<string, object>>(File.ReadAllText(JSONPath))!;

            string[] agentColorScheme = JsonSerializer.Deserialize<string[]>(jsonAgentSettings["backgroundGradientColors"].ToString()!)!;
            ColorScheme = agentColorScheme.ToList();

            if (jsonAgentSettings.TryGetValue("characterTags", out var tags) && tags != null)
            {
                string[] agentTags = JsonSerializer.Deserialize<string[]>(tags.ToString()!)!;
                Tags = agentTags.ToList();
            }

            var agentRole = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonAgentSettings["role"].ToString()!)!;
            Role = agentRole["displayName"].ToString();
        }
        catch
        {
            if (!File.Exists(JSONPath))
            {
                var jsonAgent = await GetAgentJSONFromValorantAPI(Id);
                if (jsonAgent != null)
                {
                    await File.WriteAllTextAsync(JSONPath, jsonAgent);
                    await UpdateDataFromAgentJSON();
                }
            }
        }
    }

    private static async Task<string?> GetAgentJSONFromValorantAPI(string agentId)
    {
        var builder = new UriBuilder($@"https://valorant-api.com/v1/agents/{agentId.ToLower()}");

        using (HttpClient webClient = new HttpClient())
        {
            HttpResponseMessage response = await webClient.GetAsync(builder.Uri);
            if (response.IsSuccessStatusCode)
            {
                var strResponse = await response.Content.ReadAsStringAsync();
                var jsonResponse = JsonSerializer.Deserialize<Dictionary<string, object>>(strResponse)!;
                return jsonResponse["data"]!.ToString();
            }
            else
            {
                return null;
            }
        }
    }
}