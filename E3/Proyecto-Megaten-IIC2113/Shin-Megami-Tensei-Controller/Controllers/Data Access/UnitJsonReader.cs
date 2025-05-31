namespace Shin_Megami_Tensei.Files_Handlers;
using Shin_Megami_Tensei.Gadgets;
using System.Text.Json;

public static class UnitJsonReader
{
    public static JsonElement? FindUnitByName(string name, string JSON_FILE)
    {
        string jsonString = File.ReadAllText(JSON_FILE);
        JsonDocument document = JsonDocument.Parse(jsonString);

        foreach (JsonElement unit in document.RootElement.EnumerateArray())
        {
            if (unit.GetProperty("name").GetString() == name)
            {
                return unit;
            }
        }

        return null;
    }

    public static Stat ReadStats(JsonElement samuraiData)
    {
        JsonElement stats = samuraiData.GetProperty("stats");
        return new Stat(
            stats.GetProperty("HP").GetInt32(),
            stats.GetProperty("MP").GetInt32(),
            stats.GetProperty("Str").GetInt32(),
            stats.GetProperty("Skl").GetInt32(),
            stats.GetProperty("Mag").GetInt32(),
            stats.GetProperty("Spd").GetInt32(),
            stats.GetProperty("Lck").GetInt32()
        );
    }

    public static Dictionary<string, string> ReadAffinity(JsonElement samuraiData)
    {
        var affinity = new Dictionary<string, string>();
        foreach (JsonProperty entry in samuraiData.GetProperty("affinity").EnumerateObject())
        {
            affinity[entry.Name] = entry.Value.GetString() ?? "Unknown";
        }
        return affinity;
    }
}
