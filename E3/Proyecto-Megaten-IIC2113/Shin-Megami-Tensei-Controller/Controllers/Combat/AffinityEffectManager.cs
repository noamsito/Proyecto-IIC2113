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
        
        bool isLightDarkSkill = (skill.Type == "Light" || skill.Type == "Dark");
        bool isDrainSkill = (skill.Type == "Almighty" && (
            skill.Name.Contains("Drain") || 
            skill.Name == "Life Drain" || 
            skill.Name == "Spirit Drain" || 
            skill.Name == "Energy Drain" ||
            skill.Name == "Serpent of Sheol"));
        
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
            // Solo drena HP
            HandleHPDrain(skillCtx.Caster, skillCtx.Target, baseDamage);
        }
        else if (skillName == "Spirit Drain")
        {
            // Solo drena MP
            HandleMPDrain(skillCtx.Caster, skillCtx.Target, baseDamage);
        }
        else if (skillName == "Energy Drain" || skillName == "Serpent of Sheol")
        {
            // Drena HP y MP
            HandleHPDrain(skillCtx.Caster, skillCtx.Target, baseDamage);
            HandleMPDrain(skillCtx.Caster, skillCtx.Target, baseDamage);
        }
    }

    private static void HandleHPDrain(Unit caster, Unit target, double damage)
    {
        int targetCurrentHP = target.GetCurrentStats().GetStatByName("HP");
        int actualDrain = Math.Min((int)Math.Floor(damage), targetCurrentHP);
        
        // Drenar HP del target
        UnitActionManager.ApplyDamageTaken(target, actualDrain);
        
        // Curar HP al caster
        UnitActionManager.ApplyHealToUnit(caster, actualDrain);
        
        // Mostrar mensajes
        CombatUI.DisplayDrainHPMessage(target, actualDrain);
        CombatUI.DisplayFinalHP(target);
        CombatUI.DisplayFinalHP(caster);
    }

    private static void HandleMPDrain(Unit caster, Unit target, double damage)
    {
        int targetCurrentMP = target.GetCurrentStats().GetStatByName("MP");
        int actualDrain = Math.Min((int)Math.Floor(damage), targetCurrentMP);
        
        // Drenar MP del target
        int currentMP = target.GetCurrentStats().GetStatByName("MP");
        target.GetCurrentStats().SetStatByName("MP", Math.Max(0, currentMP - actualDrain));
        
        // Restaurar MP al caster
        int casterCurrentMP = caster.GetCurrentStats().GetStatByName("MP");
        int casterMaxMP = caster.GetBaseStats().GetStatByName("MP");
        int newCasterMP = Math.Min(casterMaxMP, casterCurrentMP + actualDrain);
        caster.GetCurrentStats().SetStatByName("MP", newCasterMP);
        
        // Mostrar mensajes
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
                break;

            case "Nu":
                CombatUI.DisplayBlockMessage(affinityCtx.Target, affinityCtx.Caster);
                break;

            case "Rs": 
                if ((lckCaster + skillPower) >= (2 * lckTarget))
                {
                    CombatUI.DisplayResistMessage(affinityCtx.Target, affinityCtx.Caster);
                    hpToKill = affinityCtx.Target
                        .GetCurrentStats()
                        .GetStatByName("HP");
                    UnitActionManager.ApplyDamageTaken(affinityCtx.Target, hpToKill); 
                    CombatUI.DisplayUnitEliminated(affinityCtx.Target);
                    break;
                }

                CombatUI.DisplayHasMissed(affinityCtx.Caster);
                break;

            case "Rp":
                hpToKill = affinityCtx.Target
                    .GetCurrentStats()
                    .GetStatByName("HP");
                UnitActionManager.ApplyDamageTaken(affinityCtx.Caster, hpToKill); 
                CombatUI.DisplayHasMissed(affinityCtx.Caster);
                break;

            case "Dr":
                break;
            default:
                if (lckCaster + skillPower >= lckTarget)
                {
                    hpToKill = affinityCtx.Target
                        .GetCurrentStats()
                        .GetStatByName("HP");
                    UnitActionManager.ApplyDamageTaken(affinityCtx.Target, hpToKill);
                    CombatUI.DisplayUnitEliminated(affinityCtx.Target);
                    break;
                }
                
                CombatUI.DisplayHasMissed(affinityCtx.Caster);
                break;
        }
    }
    
    public static Unit GetTargetWithHighestPriorityAffinity(
    SkillUseContext skillCtx,
    List<Unit>      targets)
    {
        Unit repelOrDrain = null;   // Rp o Dr
        Unit nullBlock    = null;   // Nu
        Unit weak         = null;   // Wk
        Unit other        = null;   // Miss, Rs o neutral "-"

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

                case "Wk":
                    if (weak == null) weak = t;
                    break;

                case "Miss":
                case "Rs":
                case "-":      // neutral
                    if (other == null) other = t;
                    break;
            }
        }

        if (repelOrDrain != null) return repelOrDrain;
        if (nullBlock    != null) return nullBlock;
        if (weak         != null) return weak;
        if (other        != null) return other;

        // fallback defensivo
        return targets[0];
    }

}
