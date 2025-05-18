using Shin_Megami_Tensei_View;
using Shin_Megami_Tensei.Combat;
using Shin_Megami_Tensei.Gadgets;

namespace Shin_Megami_Tensei.Managers;

public static class SpecialSkillManager
{
    public static void UseSpecialSkill(SkillUseContext skillCtx)
    {
        switch (skillCtx.Skill.Name)
        {
            case "Sabbatma":
                UseSabbatma(skillCtx);
                break;
        }
    }

    private static void UseSabbatma(SkillUseContext skillCtx)
    {
        if (skillCtx.Caster is Samurai)
        {
            SummonManager.SummonFromReserveBySamurai(skillCtx.Attacker);
            foreach (var unit in skillCtx.Attacker.GetSortedActiveUnitsByOrderOfAttack())
            {
                Console.WriteLine(unit.GetName());
            }
        }
        else
        {
            SummonManager.SummonFromReserveBySamurai(skillCtx.Attacker);
        }
    }
}
