using System.Net.Http.Headers;
using Shin_Megami_Tensei_View;
using Shin_Megami_Tensei.Combat;
using Shin_Megami_Tensei.Gadgets;

namespace Shin_Megami_Tensei.Managers;

public static class AffinityEffectManager
{
    public static void ApplyEffectForSingleTargetSkill(SkillUseContext skillCtx, TurnContext turnCtx)
    {
        Skill skill = skillCtx.Skill;
        int numHits = SkillManager.CalculateNumberHits(skill.Hits, turnCtx.Attacker);
        
        int stat = GetStatForSkill(skillCtx);
        double baseDamage = CalculateBaseDamage(stat, skill.Power);
    
        var affinityCtx = CreateAffinityContext(skillCtx, baseDamage);
        
        for (int i = 0; i < numHits; i++)
        {
            ManageTargetDamage(skillCtx, affinityCtx);
        }
        
        SkillManager.ConsumeMP(skillCtx.Caster, skill.Cost);
        TurnManager.ConsumeTurnsBasedOnAffinity(affinityCtx, turnCtx);
        CombatUI.DisplayCombatUiForSkill(skillCtx, affinityCtx, numHits);
    }
    public static void ApplyEffectForMultiTargetSkill(SkillUseContext skillCtx, TurnContext turnCtx, Unit target)
    {
        Skill skill = skillCtx.Skill;
        SkillUseContext specificSkillCtxForTarget = SkillUseContext.CreateSkillContext(skillCtx.Caster, target, skill, turnCtx);
        
        int numHits = SkillManager.CalculateNumberHits(skill.Hits, turnCtx.Attacker);
        
        int stat = GetStatForSkill(specificSkillCtxForTarget);
        double baseDamage = CalculateBaseDamage(stat, skill.Power);
        
        var affinityCtx = CreateAffinityContext(specificSkillCtxForTarget, baseDamage);
        bool success = true;
        
        for (int i = 0; i < numHits; i++)
        {
            CombatUI.DisplaySkillUsage(specificSkillCtxForTarget.Caster, skill, target);
            success = GetSuccessSkillsLightAndDark(affinityCtx, specificSkillCtxForTarget);
            CombatUI.DisplayFinalHP(target);
            
            TurnManager.ConsumeTurnsBasedOnAffinity(affinityCtx, turnCtx);
        }
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

    public static void ApplyHeal(SkillUseContext skillCtx)
    {
        double finalHeal = HealSkillsManager.CalculateHeal(skillCtx.Target, skillCtx);
        UnitActionManager.ApplyHealToUnit(skillCtx.Target, finalHeal);
    }
    
    public static void ApplyHalfHeal(SkillUseContext skillCtx)
    {
        double finalHeal = HealSkillsManager.CalculateHalfHp(skillCtx.Target, skillCtx);
        UnitActionManager.ApplyHealToUnit(skillCtx.Target, finalHeal);
    }
    
    private static void ManageTargetDamage(SkillUseContext skillCtx, AffinityContext affinityCtx)
    {
        double finalDamage = GetDamageBasedOnAffinity(affinityCtx);
        
        if (finalDamage > 0)
        {
            UnitActionManager.ApplyDamageTaken(skillCtx.Target, finalDamage);
        }
        else if (finalDamage == -1)
        {
            UnitActionManager.ApplyHealToUnit(affinityCtx.Target, affinityCtx.BaseDamage);
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
    
    public static void ApplyEffectForBasicAttack(AffinityContext affinityCtx)
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
            UnitActionManager.ApplyHealToUnit(affinityCtx.Target, affinityCtx.BaseDamage);
        }
        else if (finalDamage == -2)
        {
            UnitActionManager.ApplyDamageTaken(affinityCtx.Caster, affinityCtx.BaseDamage);
        }
        else if (finalDamage == 0)
        {
            UnitActionManager.ApplyDamageTaken(affinityCtx.Caster, finalDamage);
        }
        
        CombatUI.DisplayAffinityMessage(affinityCtx);
        CombatUI.ManageDisplayAffinity(affinityType, affinityCtx, finalDamage);
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

    public static bool GetSuccessSkillsLightAndDark(AffinityContext affinityCtx, SkillUseContext skillCtx)
    {
        string affinityType = AffinityResolver.GetAffinity(affinityCtx.Target, affinityCtx.AttackType);
        
        int lckCaster = skillCtx.Caster.GetCurrentStats().GetStatByName("Lck");
        int lckTarget = skillCtx.Target.GetCurrentStats().GetStatByName("Lck");
        double skillPower = skillCtx.Skill.Power;
        double hpToKill = 0;
        
        switch (affinityType)
        {
            case "Wk":
                CombatUI.DisplayWeakMessage(affinityCtx.Target, affinityCtx.Caster);
                hpToKill = affinityCtx.Target
                                  .GetCurrentStats()
                                  .GetStatByName("HP");
                UnitActionManager.ApplyDamageTaken(affinityCtx.Target, hpToKill);
                CombatUI.DisplayUnitEliminated(affinityCtx.Target);
                return true;

            case "Nu":
                CombatUI.DisplayBlockMessage(affinityCtx.Target, affinityCtx.Caster);
                return true;

            case "Rs": 
                if ((lckCaster + skillPower) >= (2 * lckTarget))
                {
                    CombatUI.DisplayResistMessage(affinityCtx.Target, affinityCtx.Caster);
                    hpToKill = affinityCtx.Target
                        .GetCurrentStats()
                        .GetStatByName("HP");
                    UnitActionManager.ApplyDamageTaken(affinityCtx.Target, hpToKill); 
                    CombatUI.DisplayUnitEliminated(affinityCtx.Target);
                    return true;
                }

                CombatUI.DisplayHasMissed(affinityCtx.Caster);
                return false;

            case "Rp":
                hpToKill = affinityCtx.Target
                    .GetCurrentStats()
                    .GetStatByName("HP");
                UnitActionManager.ApplyDamageTaken(affinityCtx.Caster, hpToKill); 
                CombatUI.DisplayHasMissed(affinityCtx.Caster);
                return false;

            case "Dr":
                return false;

            default:
                if (lckCaster + skillPower >= lckTarget)
                {
                    hpToKill = affinityCtx.Target
                        .GetCurrentStats()
                        .GetStatByName("HP");
                    UnitActionManager.ApplyDamageTaken(affinityCtx.Target, hpToKill);
                    CombatUI.DisplayUnitEliminated(affinityCtx.Target);
                    return true;
                }
                
                CombatUI.DisplayHasMissed(affinityCtx.Caster);
                return false;
        }
    }
}
