using Shin_Megami_Tensei_View;
using Shin_Megami_Tensei.Combat;
using Shin_Megami_Tensei.Gadgets;

namespace Shin_Megami_Tensei.Managers;

public static class AffinityEffectManager
{
    public static void ApplyEffectForSkill(SkillUseContext skillCtx, TurnContext turnCtx, int numHits)
    {
        Skill currentSkill = skillCtx.Skill;
        
        int stat = GetStatForSkill(skillCtx);
        double baseDamage = CalculateBaseDamage(stat, currentSkill.Power);
    
        var affinityCtx = CreateAffinityContext(skillCtx, baseDamage);
        
        for (int i = 0; i < numHits; i++)
        {
            ApplyDamage(skillCtx, affinityCtx);
        }
        
        SkillManager.ConsumeMP(skillCtx.Caster, currentSkill.Cost);
        TurnManager.ConsumeTurnsBasedOnAffinity(affinityCtx, turnCtx);
        CombatUI.DisplayCombatUi(skillCtx, affinityCtx, numHits);
    }
    
    public static int GetStatForSkill(SkillUseContext skillCtx)
    {
        return skillCtx.Skill.Type switch
        {
            "Phys" => skillCtx.Caster.GetCurrentStats().GetStatByName("Str"),
            "Gun" => skillCtx.Caster.GetCurrentStats().GetStatByName("Skl"),
            "Fire" or "Ice" or "Elec" or "Force" or "Almighty" => skillCtx.Caster.GetCurrentStats().GetStatByName("Mag"),
            _ => 0
        };
    }

    public static double CalculateBaseDamage(int stat, double skillPower)
    {
        return Math.Sqrt(stat * skillPower);
    }
    
    public static AffinityContext CreateAffinityContext(SkillUseContext skillCtx, double baseDamage)
    {
        return new AffinityContext(skillCtx.Caster, skillCtx.Target, skillCtx.Skill.Type, baseDamage);
    }

    public static void ApplyHeal(SkillUseContext skillCtx, AffinityContext affinityCtx)
    {
        double finalHeal = SkillManager.CalculateHeal(skillCtx.Target, skillCtx);
        UnitActionManager.Heal(affinityCtx.Target, finalHeal);
    }
    
    private static void ApplyDamage(SkillUseContext skillCtx, AffinityContext affinityCtx)
    {
        double finalDamage = GetDamageBasedOnAffinity(affinityCtx);
        
        if (finalDamage > 0)
        {
            UnitActionManager.ApplyDamageTaken(skillCtx.Target, finalDamage);
        }
        else if (finalDamage == -1)
        {
            UnitActionManager.Heal(affinityCtx.Target, affinityCtx.BaseDamage);
        }
        else if (finalDamage == -2)
        {
            UnitActionManager.ApplyDamageTaken(affinityCtx.Caster, affinityCtx.BaseDamage);
        }
        else if (finalDamage == 0)
        {
            UnitActionManager.ApplyDamageTaken(skillCtx.Caster, finalDamage);
        }
    }
    
    public static double ApplyEffectForBasicAttack(AffinityContext affinityCtx)
    {
        string affinityType = AffinityResolver.GetAffinity(affinityCtx.Target, affinityCtx.AttackType);
        double finalDamage = GetDamageBasedOnAffinity(affinityCtx);
        int damageTaken = 0;
        
        if (finalDamage > 0)
        {
            UnitActionManager.ApplyDamageTaken(affinityCtx.Target, finalDamage);
        }
        else if (finalDamage == -1)
        {
            UnitActionManager.Heal(affinityCtx.Target, affinityCtx.BaseDamage);
        }
        else if (finalDamage == -2)
        {
            damageTaken = UnitActionManager.ApplyDamageTaken(affinityCtx.Caster, affinityCtx.BaseDamage);
        }
        else if (finalDamage == 0)
        {
            damageTaken = UnitActionManager.ApplyDamageTaken(affinityCtx.Caster, finalDamage);
        }
        
        CombatUI.DisplayAffinityMessage(affinityCtx);
        CombatUI.ManageDisplayAffinity(affinityType, affinityCtx, finalDamage);
        
        return finalDamage;
    }
    
    public static double GetDamageBasedOnAffinity(AffinityContext affinityCtx)
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
                return -1;

            case "Rp":
                return -2;

            default:
                return affinityCtx.BaseDamage;
        }
    }
}
