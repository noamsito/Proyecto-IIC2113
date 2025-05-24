using System.Text.Json;
using Shin_Megami_Tensei.Data;
using Shin_Megami_Tensei.Gadgets;
using Shin_Megami_Tensei.Units;

namespace Shin_Megami_Tensei.Files_Handlers;

public static class SamuraiDataFactory
{

    public static SamuraiData? LoadFromJson(string name, List<string> skills)
    {
        var unitJson = UnitJsonReader.FindUnitByName(name, GameConstants.JSON_FILE_SAMURAI);
        if (!unitJson.HasValue) return null;

        var baseStats = UnitJsonReader.ReadStats(unitJson.Value);
        var affinityDict = UnitJsonReader.ReadAffinity(unitJson.Value);
        var affinity = new Affinity(affinityDict);

        var skillList = LoadSkillsFromJson(skills);

        return new SamuraiData(name, baseStats, affinity, skillList);
    }

    private static List<Skill> LoadSkillsFromJson(List<string> skillNames)
    {
        return skillNames
            .Select(SkillJsonReader.FindSkillByName)
            .Where(skill => skill != null)
            .ToList()!;
    }
}