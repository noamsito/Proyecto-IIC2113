using System.Text.Json;
using Shin_Megami_Tensei.Gadgets;

namespace Shin_Megami_Tensei.Units;

public static class SkillJsonReader
{
    private const string JSON_FILE_SKILLS = "data/skills.json";
    
    public static Skill? FindSkillByName(string name)
    {
        string jsonString = File.ReadAllText(JSON_FILE_SKILLS);
        JsonDocument document = JsonDocument.Parse(jsonString);

        foreach (JsonElement skillJson in document.RootElement.EnumerateArray())
        {
            if (skillJson.GetProperty("name").GetString() == name)
            {
                return new Skill(
                    name,
                    skillJson.GetProperty("type").GetString()!,
                    skillJson.GetProperty("cost").GetInt32(),
                    skillJson.GetProperty("power").GetInt32(),
                    skillJson.GetProperty("target").GetString()!,
                    skillJson.GetProperty("hits").GetString()!,
                    skillJson.GetProperty("effect").GetString()!
                );
            }
        }

        return null;
    }
}