using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

[Serializable]
public class RiotValorantContent
{
    [JsonIgnore]
    public static RiotValorantContent Instance = null!;

    public string? Version { get; set; }

    public ValorantEntity[]? Characters { get; set; }

    public ValorantEntity[]? Maps { get; set; }

    public List<ValorantEntity> GetAgents()
    {
        return (Instance.Characters ?? new ValorantEntity[] { })
            .Where(c => c.Name != "Null UI Data!")
            .GroupBy(a => a.Name)
            .Select(a => a.First())
            .ToList();
    }


    public List<ValorantEntity> GetMaps()
    {
        return (Instance.Maps ?? new ValorantEntity[] { })
            .Where(c => c.Name != "Null UI Data!")
            .Where(c => c.Name != "The Range")
            .GroupBy(a => a.Name)
            .Select(a => a.First())
            .ToList();
    }

    public static ValorantEntity? GetAgentByName(string name)
    {
        return Instance.Characters?.Where(c => c.Name == name).FirstOrDefault();
    }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }

    public async Task UpdateAgentData()
    {
        for (int i = 0; i < RiotValorantContent.Instance?.Characters?.Length; ++i)
        {
            await RiotValorantContent.Instance!.Characters![i].UpdateDataFromAgentJSON();
        }
    }
}