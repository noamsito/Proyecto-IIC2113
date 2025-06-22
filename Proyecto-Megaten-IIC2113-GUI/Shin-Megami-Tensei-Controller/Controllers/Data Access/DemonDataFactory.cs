using System.Text.Json;
using Shin_Megami_Tensei.Data;
using Shin_Megami_Tensei.Gadgets;
using Shin_Megami_Tensei.Units;

namespace Shin_Megami_Tensei.Files_Handlers;

public static class DemonDataFactory
{
    public static DemonData? LoadFromJson(string name)
    {
        var unitJson = UnitJsonReader.FindUnitByName(name, GameConstants.JSON_FILE_MONSTERS);
        if (!unitJson.HasValue) return null;

        var baseStats = UnitJsonReader.ReadStats(unitJson.Value);
        var affinity = new Affinity(UnitJsonReader.ReadAffinity(unitJson.Value));
        var skills = LoadSkillsFromJson(unitJson.Value);

        return new DemonData(name, baseStats, affinity, skills);
    }

    private static List<Skill> LoadSkillsFromJson(JsonElement demonJson)
    {
        return demonJson.TryGetProperty("skills", out var skillsJson) && skillsJson.ValueKind == JsonValueKind.Array
            ? skillsJson.EnumerateArray()
                .Select(skillJson => SkillJsonReader.FindSkillByName(skillJson.GetString() ?? string.Empty))
                .Where(skill => skill != null)
                .ToList()!
            : new List<Skill>();
    }
}