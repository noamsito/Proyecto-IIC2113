using Shin_Megami_Tensei_View;
using Shin_Megami_Tensei.Combat;
using Shin_Megami_Tensei.Gadgets;

namespace Shin_Megami_Tensei.Managers;

public static class SpecialSkillManager
{
    public static bool UseSpecialSkill(SkillUseContext skillCtx)
    {
        bool usedSkill = true;
        switch (skillCtx.Skill.Name)
        {
            case "Sabbatma":
                usedSkill = UseSabbatma(skillCtx);
                break;
        }
        
        return usedSkill;
    }

    private static bool UseSabbatma(SkillUseContext skillCtx)
    {
        return SummonManager.SummonFromReserveBySamurai(skillCtx.Attacker);
    }
}
