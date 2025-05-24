namespace Shin_Megami_Tensei;

public static class GameConstants
{
    public const string JSON_FILE_SAMURAI = "data/samurai.json";
    public const string JSON_FILE_MONSTERS = "data/monsters.json";
    public const string JSON_FILE_SKILLS = "data/skills.json";

    public const string ConstCancel = "Cancelar";
    public const int SeparatorLinesCount = 40;
    public static readonly string Separator = new string('-', SeparatorLinesCount);    public const double ConstantOfDamage = 0.0114f;
    public const double ModifierPhysDamage = 54;
    public const double ModifierGunDamage = 80;

    public const int MAX_DEMONS = 7;
    public const int MAX_SKILLS_SAMURAI = 8;
    public const double MULTIPLIER_WEAK_AFFINITY = 1.5;
    public const double MULTIPLIER_RESISTANT_AFFINITY = 2.0;
}