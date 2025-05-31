using System.Net.Http.Headers;
using Shin_Megami_Tensei_View;
using Shin_Megami_Tensei.Combat;
using Shin_Megami_Tensei.Data;
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
        
        bool isLightDarkSkill = (skill.Type == "Light" || skill.Type == "Dark");
        bool isDrainSkill = (skill.Type == "Almighty" && 
            skill.Name == "Life Drain" || 
            skill.Name == "Spirit Drain" || 
            skill.Name == "Energy Drain" ||
            skill.Name == "Serpent of Sheol");
        
        if (isLightDarkSkill)
        {
            for (int i = 0; i < numHits; i++)
            {
                CombatUI.DisplaySkillUsage(skillCtx.Caster, skill, skillCtx.Target);
                GetSuccessSkillsLightAndDark(affinityCtx, skillCtx);
                CombatUI.DisplayFinalHP(skillCtx.Target);
            }
            
            CombatUI.DisplaySeparator();
        }
        else if (isDrainSkill)
        {
            for (int i = 0; i < numHits; i++)
            {
                HandleDrainSkill(skillCtx, affinityCtx);
            }
        }
        else
        {
            for (int i = 0; i < numHits; i++)
            {
                ManageTargetDamage(skillCtx, affinityCtx);
            }
            
            CombatUI.DisplayCombatUiForSkill(skillCtx, affinityCtx, numHits);
        }

        TurnManager.ConsumeTurnsBasedOnAffinity(affinityCtx, turnCtx);
    }

    private static void HandleDrainSkill(SkillUseContext skillCtx, AffinityContext affinityCtx)
    {
        CombatUI.DisplaySkillUsage(skillCtx.Caster, skillCtx.Skill, skillCtx.Target);
        
        string skillName = skillCtx.Skill.Name;
        double baseDamage = affinityCtx.BaseDamage;
        
        if (skillName == "Life Drain")
        {
            HandleHPDrain(skillCtx.Caster, skillCtx.Target, baseDamage);
        }
        else if (skillName == "Spirit Drain")
        {
            HandleMPDrain(skillCtx.Caster, skillCtx.Target, baseDamage);
        }
        else if (skillName == "Energy Drain" || skillName == "Serpent of Sheol")
        {
            HandleHPDrain(skillCtx.Caster, skillCtx.Target, baseDamage);
            HandleMPDrain(skillCtx.Caster, skillCtx.Target, baseDamage);
        }
    }

    private static void HandleHPDrain(Unit caster, Unit target, double damage)
    {
        int targetCurrentHP = target.GetCurrentStats().GetStatByName("HP");
        int actualDrain = Math.Min((int)Math.Floor(damage), targetCurrentHP);
        
        UnitActionManager.ApplyDamageTaken(target, actualDrain);
        
        UnitActionManager.ApplyHealToUnit(caster, actualDrain);
        
        CombatUI.DisplayDrainHPMessage(target, actualDrain);
        CombatUI.DisplayFinalHP(target);
        CombatUI.DisplayFinalHP(caster);
    }

    private static void HandleMPDrain(Unit caster, Unit target, double damage)
    {
        int targetCurrentMP = target.GetCurrentStats().GetStatByName("MP");
        int actualDrain = Math.Min((int)Math.Floor(damage), targetCurrentMP);
        
        int currentMP = target.GetCurrentStats().GetStatByName("MP");
        target.GetCurrentStats().SetStatByName("MP", Math.Max(0, currentMP - actualDrain));
        
        int casterCurrentMP = caster.GetCurrentStats().GetStatByName("MP");
        int casterMaxMP = caster.GetBaseStats().GetStatByName("MP");
        int newCasterMP = Math.Min(casterMaxMP, casterCurrentMP + actualDrain);
        caster.GetCurrentStats().SetStatByName("MP", newCasterMP);
        
        CombatUI.DisplayDrainMPMessage(target, actualDrain);
        CombatUI.DisplayFinalMP(target);
        CombatUI.DisplayFinalMP(caster);
        CombatUI.DisplaySeparator();
    }
    
    public static void ApplyEffectForMultiTargetSkill(SkillUseContext skillCtx, TurnContext turnCtx, Unit target)
    {
        Skill skill = skillCtx.Skill;
        SkillUseContext specificSkillCtxForTarget = SkillUseContext.CreateSkillContext(skillCtx.Caster, target, skill, turnCtx);
    
        int numHits = SkillManager.CalculateNumberHits(skill.Hits, turnCtx.Attacker);
    
        int stat = GetStatForSkill(specificSkillCtxForTarget);
        double baseDamage = CalculateBaseDamage(stat, skill.Power);
    
        var affinityCtx = CreateAffinityContext(specificSkillCtxForTarget, baseDamage);
    
        for (int i = 0; i < numHits; i++)
        {
            CombatUI.DisplaySkillUsage(specificSkillCtxForTarget.Caster, skill, target);
            GetSuccessSkillsLightAndDark(affinityCtx, specificSkillCtxForTarget);
            CombatUI.DisplayFinalHP(target);
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

    public static void GetSuccessSkillsLightAndDark(AffinityContext affinityCtx, SkillUseContext skillCtx)
    {
        string affinityType = AffinityResolver.GetAffinity(affinityCtx.Target, affinityCtx.AttackType);
        var skillData = ExtractSkillCalculationData(skillCtx);
        
        switch (affinityType)
        {
            case "Wk":
                ProcessWeakAffinity(affinityCtx);
                break;
            case "Nu":
                ProcessNullAffinity(affinityCtx);
                break;
            case "Rs":
                ProcessResistAffinity(affinityCtx, skillData);
                break;
            case "Rp":
                ProcessRepelAffinity(affinityCtx);
                break;
            case "Dr":
                break;
            default:
                ProcessNeutralAffinity(affinityCtx, skillData);
                break;
        }
    }

    private static SkillCalculationData ExtractSkillCalculationData(SkillUseContext skillCtx)
    {
        int casterLuck = skillCtx.Caster.GetCurrentStats().GetStatByName("Lck");
        int targetLuck = skillCtx.Target.GetCurrentStats().GetStatByName("Lck");
        double skillPower = skillCtx.Skill.Power;
        
        return new SkillCalculationData(casterLuck, targetLuck, skillPower);
    }

    private static void ProcessWeakAffinity(AffinityContext affinityCtx)
    {
        CombatUI.DisplayWeakMessage(affinityCtx.Target, affinityCtx.Caster);
        KillTargetUnit(affinityCtx.Target);
        CombatUI.DisplayUnitEliminated(affinityCtx.Target);
    }

    private static void ProcessNullAffinity(AffinityContext affinityCtx)
    {
        CombatUI.DisplayBlockMessage(affinityCtx.Target, affinityCtx.Caster);
    }

    private static void ProcessResistAffinity(AffinityContext affinityCtx, SkillCalculationData skillData)
    {
        if (IsSkillSuccessfulAgainstResist(skillData))
        {
            ProcessSuccessfulResistAttack(affinityCtx);
        }
        else
        {
            ProcessMissedAttack(affinityCtx.Caster);
        }
    }

    private static void ProcessRepelAffinity(AffinityContext affinityCtx)
    {
        KillTargetUnit(affinityCtx.Caster);
        CombatUI.DisplayHasMissed(affinityCtx.Caster);
    }

    private static void ProcessNeutralAffinity(AffinityContext affinityCtx, SkillCalculationData skillData)
    {
        if (IsSkillSuccessfulAgainstNeutral(skillData))
        {
            ProcessSuccessfulNeutralAttack(affinityCtx);
        }
        else
        {
            ProcessMissedAttack(affinityCtx.Caster);
        }
    }

    private static bool IsSkillSuccessfulAgainstResist(SkillCalculationData skillData)
    {
        return (skillData.CasterLuck + skillData.SkillPower) >= (2 * skillData.TargetLuck);
    }

    private static bool IsSkillSuccessfulAgainstNeutral(SkillCalculationData skillData)
    {
        return skillData.CasterLuck + skillData.SkillPower >= skillData.TargetLuck;
    }

    private static void ProcessSuccessfulResistAttack(AffinityContext affinityCtx)
    {
        CombatUI.DisplayResistMessage(affinityCtx.Target, affinityCtx.Caster);
        KillTargetUnit(affinityCtx.Target);
        CombatUI.DisplayUnitEliminated(affinityCtx.Target);
    }

    private static void ProcessSuccessfulNeutralAttack(AffinityContext affinityCtx)
    {
        KillTargetUnit(affinityCtx.Target);
        CombatUI.DisplayUnitEliminated(affinityCtx.Target);
    }

    private static void ProcessMissedAttack(Unit caster)
    {
        CombatUI.DisplayHasMissed(caster);
    }

    private static void KillTargetUnit(Unit target)
    {
        double currentHP = target.GetCurrentStats().GetStatByName("HP");
        UnitActionManager.ApplyDamageTaken(target, currentHP);
    }

    public static Unit GetTargetWithHighestPriorityAffinity(
        SkillUseContext skillCtx,
        List<Unit>      targets)
    {
        Unit repelOrDrain = null;
        Unit nullBlock = null;
        Unit miss = null;
        Unit weak = null;
        Unit other = null;

        foreach (Unit t in targets)
        {
            string affinity = AffinityResolver.GetAffinity(t, skillCtx.Skill.Type);

            switch (affinity)
            {
                case "Rp":
                case "Dr":
                    if (repelOrDrain == null) repelOrDrain = t;
                    break;

                case "Nu":
                    if (nullBlock == null) nullBlock = t;
                    break;

                case "Miss":
                    if (miss == null) miss = t;
                    break;

                case "Wk":
                    if (weak == null) weak = t;
                    break;

                case "Rs":
                case "-":
                    if (other == null) other = t;
                    break;
            }
        }

        if (repelOrDrain != null) return repelOrDrain;
        if (nullBlock    != null) return nullBlock;
        if (miss         != null) return miss;
        if (weak         != null) return weak;
        if (other        != null) return other;

        return targets[0];
    }

}
