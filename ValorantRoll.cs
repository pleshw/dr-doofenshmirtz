using System.Security.Cryptography;

public struct ValorantMatch
{
    public List<ValorantEntity> Team1;
    public List<ValorantEntity> Team2;

    public List<ValorantEntity> Maps;
}

public static class ValorantRoll
{
    public static ValorantEntity RollAgent()
    {
        var agents = RiotValorantContent.Instance.GetAgents();

        var randomIndex = RandomNumberGenerator.GetInt32(0, agents.Count);

        return agents[randomIndex];
    }

    public static ValorantEntity RollMap()
    {
        var maps = RiotValorantContent.Instance.GetMaps();

        var randomIndex = RandomNumberGenerator.GetInt32(0, maps.Count);

        return maps[randomIndex];
    }

    public static List<ValorantEntity> RollMaps(int numMaps)
    {
        var maps = RiotValorantContent.Instance.GetMaps();

        if (numMaps > maps.Count - 1)
        {
            numMaps = maps.Count - 1;
        }

        List<ValorantEntity> listMaps = new List<ValorantEntity>();
        GetRangeOfRandomUniqueNumbers(numMaps, maps.Count).ToList().ForEach(b => listMaps.Add(maps[b]));

        return listMaps;
    }

    public static List<ValorantEntity> RollAgents(int numAgents)
    {
        var agents = RiotValorantContent.Instance.GetAgents();

        if (numAgents > agents.Count - 1)
        {
            numAgents = agents.Count - 1;
        }

        List<ValorantEntity> listAgents = new List<ValorantEntity>();
        GetRangeOfRandomUniqueNumbers(numAgents, agents.Count).ToList().ForEach(b => listAgents.Add(agents[b]));

        return listAgents;
    }

    public static List<ValorantEntity> RollTeam()
    {
        return RollAgents(5);
    }

    public static List<ValorantEntity> RollMapPicks()
    {
        return RollMaps(3);
    }

    public static ValorantMatch RollMatch()
    {
        var agents = RollAgents(10);

        return new ValorantMatch
        {
            Team1 = RollTeam(),
            Team2 = RollTeam(),
            Maps = RollMapPicks()
        };
    }

    private static IEnumerable<int> GetRangeOfRandomUniqueNumbers(int numResults, int maxValueExcluded)
    {
        var listRange = new List<int> { };
        listRange.AddRange(Enumerable.Range(0, maxValueExcluded));

        for (int i = 0; i < numResults; ++i)
        {
            byte[] randomIndex = RandomNumberGenerator.GetBytes(1);

            int resultIndex = randomIndex[0] % listRange.Count;
            int result = listRange[resultIndex];
            listRange.RemoveAt(resultIndex);

            yield return result;
        }
    }
}