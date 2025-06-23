namespace Shin_Megami_Tensei.Enums;

public static class AttackTypeExtensions
{
    private static readonly Dictionary<AttackType, string> _attackTypeStrings = new()
    {
        { AttackType.Physical, "Phys" },
        { AttackType.Gun, "Gun" },
        { AttackType.Fire, "Fire" },
        { AttackType.Ice, "Ice" },
        { AttackType.Electric, "Elec" },
        { AttackType.Force, "Force" },
        { AttackType.Light, "Light" },
        { AttackType.Dark, "Dark" },
        { AttackType.Almighty, "Almighty" },
        { AttackType.Heal, "Heal" }
    };

    public static string ToGameString(this AttackType attackType)
    {
        return _attackTypeStrings[attackType];
    }

    public static AttackType FromGameString(string gameString)
    {
        return _attackTypeStrings.FirstOrDefault(kvp => kvp.Value == gameString).Key;
    }
}
