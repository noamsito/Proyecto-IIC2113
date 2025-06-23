namespace Shin_Megami_Tensei.Enums;

public static class SkillTargetExtensions
{
    private static readonly Dictionary<SkillTarget, string> _targetStrings = new()
    {
        { SkillTarget.Single, "Single" },
        { SkillTarget.Ally, "Ally" },
        { SkillTarget.All, "All" },
        { SkillTarget.Party, "Party" },
        { SkillTarget.Multi, "Multi" }
    };

    public static string ToGameString(this SkillTarget target)
    {
        return _targetStrings[target];
    }

    public static SkillTarget FromGameString(string gameString)
    {
        return _targetStrings.FirstOrDefault(kvp => kvp.Value == gameString).Key;
    }
}
