using Shin_Megami_Tensei.Combat;
using Shin_Megami_Tensei.Gadgets;

namespace Shin_Megami_Tensei.Managers;

public static class HealSkillsManager
{
    public static bool HandleMultiTargetHealSkill(SkillUseContext skillCtx, TurnContext turnCtx)
    {
        List<Unit> targets = TurnManager.GetTargetsForMultiTargetSkill(skillCtx);
        
        foreach (Unit target in targets)
        { 
            double healAmount = CalculateHeal(target, skillCtx);
            ApplyHealEffect(skillCtx, target, healAmount);
        }

        ApplySpecificEffectsForMultiHealSkill(skillCtx);
        CombatUI.DisplaySpecificForHealSkill(skillCtx);
        
        TurnManager.ConsumeTurn(turnCtx);
        
        CombatUI.DisplaySeparator();
        return true;
    }
    
    private static void ApplyHealEffect(SkillUseContext skillCtx, Unit target, double healAmount)
    {
        Unit caster = skillCtx.Caster;
        int hpBeforeHeal = target.GetCurrentStats().GetStatByName("HP");
        bool isTargetDead = hpBeforeHeal <= 0;
        bool isReviveSkill = GameConstants._reviveOnlySkillsNames.Contains(skillCtx.Skill.Name);
        bool isReviveAndHealSkill = GameConstants._reviveAndHealSkillsNames.Contains(skillCtx.Skill.Name);
        
        if ((isReviveSkill && isTargetDead) || (!isReviveSkill && !isTargetDead) || isReviveAndHealSkill)
        {
            UnitActionManager.ApplyHealToUnit(target, healAmount);
            int hpAfterHeal = target.GetCurrentStats().GetStatByName("HP");
            int healedAmount = hpAfterHeal - hpBeforeHeal;

            if ((isReviveSkill && isTargetDead) || (isReviveAndHealSkill && isTargetDead))
            {
                CombatUI.DisplayReviveForMultiTargets(caster, target, healedAmount);
            }
            else
            {
                CombatUI.DisplayHealingForMultiTargets(caster, target, healAmount);
            }
        }
    }

    private static void ApplySpecificEffectsForMultiHealSkill(SkillUseContext skillCtx)
    {
        switch (skillCtx.Skill.Name)
        {
            case "Recarmdra":
                PlayerUnitManager unitManagerPlayer = skillCtx.Attacker.UnitManager;
                Unit unitCaster = skillCtx.Caster;
                unitCaster.GetCurrentStats().SetStatByName("HP", 0);
                
                unitManagerPlayer.RemoveFromActiveUnitsIfDead();
                break;
        }
    }
    
    private static bool IsUnitAlive(Unit unit)
    {
        return unit.GetCurrentStats().GetStatByName("HP") > 0;
    }
    
    public static bool HandleSingleTargetHealSkill(SkillUseContext skillCtx, TurnContext turnCtx)
    {
        string skillName = skillCtx.Skill.Name;
        Skill skill = skillCtx.Skill;
        int numberHits = SkillManager.CalculateNumberHits(skill.Hits, skillCtx.Attacker);
        int stat = AffinityEffectManager.GetStatForSkill(skillCtx);
        bool hasBeenRevived = false;
        bool usedSkill;
        
        double baseDamage = AffinityEffectManager.CalculateBaseDamage(stat, skill.Power);
        var affinityCtx = AffinityEffectManager.CreateAffinityContext(skillCtx, baseDamage);
        
        switch (skillName)
        {
            case "Invitation":
                affinityCtx.Target = skillCtx.Target;
                usedSkill = SummonManager.SummonBySkillInvitation(skillCtx, turnCtx);
                break;
            
            case "Recarm":
                affinityCtx.Target = skillCtx.Target;
                
                usedSkill = HandleRecarmSkill(skillCtx);
                
                TurnManager.ConsumeTurnsBasedOnAffinity(affinityCtx, turnCtx);
                
                break;

            default:
                for (int i = 0; i < numberHits; i++)
                {
                    AffinityEffectManager.ApplyHeal(skillCtx);
                }

                usedSkill = true;
                
                CombatUI.DisplayCombatUiForSkill(skillCtx, affinityCtx, numberHits);
                TurnManager.ConsumeTurnsBasedOnAffinity(affinityCtx, turnCtx);
                break;
        }
        
        return usedSkill;
    }
    
    private static bool HandleRecarmSkill(SkillUseContext skillCtx)
    {
        Player attackerPlayer = skillCtx.Attacker;
        Unit unitTarget = skillCtx.Target;
        
        PlayerUnitManager playerUnitManager = attackerPlayer.UnitManager;
        
        List<Unit> activeUnitsList = playerUnitManager.GetActiveUnits();
        
        CombatUI.DisplaySkillUsage(skillCtx.Caster, skillCtx.Skill, skillCtx.Target);
        double amountHealed = HealHalfTotalHp(skillCtx);
    
        if (activeUnitsList.Contains(unitTarget)) playerUnitManager.AddUnitInSortedList(skillCtx.Target);
    
        CombatUI.DisplayHealingForSingleTarget(skillCtx.Target, amountHealed);
        CombatUI.DisplaySeparator();
        return true;
    }
    
    private static double HealHalfTotalHp(SkillUseContext skillCtx)
    {
        double amountHealed = CalculateHalfHp(skillCtx.Target, skillCtx);
        AffinityEffectManager.ApplyHalfHeal(skillCtx);
        return amountHealed;
    }
    
    public static double CalculateHeal(Unit targetUnit, SkillUseContext skillCtx)
    {
        Skill currentSkill = skillCtx.Skill;
        
        int skillPower = currentSkill.Power;
        int baseHealth = targetUnit.GetBaseStats().GetStatByName("HP");
        
        return Math.Floor((skillPower / 100.0) * baseHealth);
    }

    public static double CalculateHalfHp(Unit targetUnit, SkillUseContext skillCtx)
    {
        int baseHealth = targetUnit.GetBaseStats().GetStatByName("HP");
        
        return Math.Floor(baseHealth / 2.0);
    }
    
}