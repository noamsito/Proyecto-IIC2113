using Shin_Megami_Tensei_View;
using Shin_Megami_Tensei.Combat;
using Shin_Megami_Tensei.Gadgets;

namespace Shin_Megami_Tensei.Managers;

public static class AffinityEffectManager
{
    public static double ApplyEffectForSkill(SkillUseContext skillCtx, TurnContext turnCtx)
    {
        int stat = skillCtx.Skill.Type switch
        {
            "Phys" => skillCtx.Caster.GetCurrentStats().GetStatByName("Str"),
            "Gun" => skillCtx.Caster.GetCurrentStats().GetStatByName("Skl"),
            "Fire" or "Ice" or "Elec" or "Force" or "Almighty" => skillCtx.Caster.GetCurrentStats().GetStatByName("Mag"),
            _ => 0
        };

        double baseDamage = Math.Sqrt(stat * skillCtx.Skill.Power);

        var affinityCtx = new AffinityContext(skillCtx.Caster, skillCtx.Target, skillCtx.Skill.Type, baseDamage);
        string affinityType = AffinityResolver.GetAffinity(affinityCtx.Target, affinityCtx.AttackType);
        double finalDamage = GetDamageBasedOnAffinity(affinityCtx);

        if (finalDamage > 0)
        {
            UnitActionManager.ApplyDamageTaken(skillCtx.Target, finalDamage);
        }
        else
        {
            UnitActionManager.ApplyDamageTaken(skillCtx.Caster, finalDamage);
        }
        
        SkillManager.ConsumeMP(skillCtx.Caster, skillCtx.Skill.Cost);

        Unit casterSkillUnit = skillCtx.Caster;
        Skill skill = skillCtx.Skill;
        Unit targetUnit = skillCtx.Target;
        
        CombatUI.DisplaySkillUsage(casterSkillUnit, skill, targetUnit); 
        CombatUI.DisplayAffinityMessage(affinityCtx);
        CombatUI.ManageDisplayAffinity(affinityType, affinityCtx, finalDamage);
        
        TurnManager.ConsumeTurnsBasedOnAffinity(affinityCtx, turnCtx);
        return finalDamage;
    }
    
    public static double ApplyEffectForBasicAttack(AffinityContext affinityCtx)
    {
        string affinityType = AffinityResolver.GetAffinity(affinityCtx.Target, affinityCtx.AttackType);
        double finalDamage = GetDamageBasedOnAffinity(affinityCtx);
        
        if (finalDamage > 0)
        {
            UnitActionManager.ApplyDamageTaken(affinityCtx.Target, finalDamage);
        }
        else if (finalDamage == 0)
        {
            UnitActionManager.ApplyDamageTaken(affinityCtx.Caster, finalDamage);
        }

        CombatUI.DisplayAffinityMessage(affinityCtx);
        CombatUI.ManageDisplayAffinity(affinityType, affinityCtx, finalDamage);
        
        return finalDamage;
    }
    
    private static double GetDamageBasedOnAffinity(AffinityContext affinityCtx)
    {
        string affinity = AffinityResolver.GetAffinity(affinityCtx.Target, affinityCtx.AttackType);

        double multiplierWk = GameConstants.MULTIPLIER_WEAK_AFFINITY;
        double multiplierRs = GameConstants.MULTIPLIER_RESISTANT_AFFINITY;
        
        switch (affinity)
        {
            case "Wk":
                return affinityCtx.BaseDamage * multiplierWk;

            case "Rs":
                return affinityCtx.BaseDamage / multiplierRs;

            case "Nu":
                return 0;

            case "Dr":
                UnitActionManager.Heal(affinityCtx.Target, affinityCtx.BaseDamage);
                return 0;

            case "Rp":
                UnitActionManager.ApplyDamageTaken(affinityCtx.Caster, affinityCtx.BaseDamage);
                return 0;

            default:
                return affinityCtx.BaseDamage;
        }
    }
}
