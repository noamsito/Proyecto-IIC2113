public static class GameConstants
{
    public const string JSON_FILE_SAMURAI = "data/samurai.json";
    public const string JSON_FILE_MONSTERS = "data/monsters.json";
    public const string JSON_FILE_SKILLS = "data/skills.json";

    public const string CANCEL_OPTION = "Cancelar";
    public const int SEPARATOR_LINES_COUNT = 40;
    public static readonly string Separator = new string('-', SEPARATOR_LINES_COUNT);
        
    public const double CONSTANT_OF_DAMAGE = 0.0114f;
    public const double MODIFIER_PHYS_DAMAGE = 54;
    public const double MODIFIER_GUN_DAMAGE = 80;

    public const int MAX_DEMONS = 7;
    public const int MAX_SKILLS_SAMURAI = 8;
    public const double MULTIPLIER_WEAK_AFFINITY = 1.5;
    public const double MULTIPLIER_RESISTANT_AFFINITY = 2.0;
}