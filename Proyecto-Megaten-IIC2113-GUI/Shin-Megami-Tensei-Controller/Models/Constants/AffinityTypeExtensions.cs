using Shin_Megami_Tensei.Enums;

public static class AffinityTypeExtensions
{
    private static readonly Dictionary<AffinityType, string> _affinityStrings = new()
    {
        { AffinityType.Weak, "Wk" },
        { AffinityType.Resistant, "Rs" },
        { AffinityType.Null, "Nu" },
        { AffinityType.Repel, "Rp" },
        { AffinityType.Drain, "Dr" },
        { AffinityType.Normal, "-" }
    };

    public static string ToGameString(this AffinityType affinity)
    {
        return _affinityStrings[affinity];
    }

    public static AffinityType FromGameString(string gameString)
    {
        return _affinityStrings.FirstOrDefault(kvp => kvp.Value == gameString).Key;
    }
}
