namespace Shin_Megami_Tensei;
using Shin_Megami_Tensei.Enums;

public static class SkillConstants
{
    public static readonly List<string> SKILLS_WITHOUT_TARGET_SELECTION = new List<string>
    {
        "Invitation", "Media", "Mediarama", "Mediarahan", "Recarmdra", "Judgement Light", "Mahama"
    };

    public static readonly List<SkillTarget> MULTI_TARGET_INDICATORS = new List<SkillTarget>
    {
        SkillTarget.All, 
        SkillTarget.Party, 
        SkillTarget.Multi
    };

    public static readonly List<string> REVIVE_ONLY_SKILLS = new List<string>
    {
    };
        
    public static readonly List<string> REVIVE_AND_HEAL_SKILLS = new List<string>
    {
        "Recarmdra"
    };

    public static readonly List<string> HEALS_EXCLUDING_CASTER = new List<string>
    {
        "Recarmdra"
    };
    
    public static readonly List<string> DRAIN_SKILLS = new List<string>
    {
        "Life Drain",
        "Spirit Drain",
        "Energy Drain",
        "Serpent of Sheol"
    };
}
