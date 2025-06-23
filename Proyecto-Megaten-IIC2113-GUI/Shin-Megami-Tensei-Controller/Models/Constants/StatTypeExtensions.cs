namespace Shin_Megami_Tensei.Enums;

public static class StatTypeExtensions
{
    private static readonly Dictionary<StatType, string> _statStrings = new()
    {
        { StatType.Hp, "HP" },
        { StatType.Mp, "MP" },
        { StatType.Strength, "Str" },
        { StatType.Skill, "Skl" },
        { StatType.Magic, "Mag" },
        { StatType.Speed, "Spd" },
        { StatType.Luck, "Lck" }
    };

    public static string ToGameString(this StatType stat)
    {
        return _statStrings[stat];
    }

    public static StatType FromGameString(string gameString)
    {
        return _statStrings.FirstOrDefault(kvp => kvp.Value == gameString).Key;
    }
}