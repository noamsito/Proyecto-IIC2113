using Shin_Megami_Tensei.Combat;
using Shin_Megami_Tensei.Gadgets;

namespace Shin_Megami_Tensei.Managers;

public static class HealSkillsManager
{
    public static bool HandleMultiTargetHealSkill(SkillUseContext skillCtx, TurnContext turnCtx)
    {
        Skill skill = skillCtx.Skill;
        Unit caster = skillCtx.Caster;
        
        Player currentPlayer = skillCtx.Attacker;
        Player opponent = skillCtx.Defender;
        
        double healAmount = CalculateHeal(caster, skillCtx);
        bool isReviveSkill = GameConstants._reviveSkillsNames.Contains(skill.Name);
        
        List<Unit> targets = GetTargetsForMultiHealSkill(skillCtx);
        
        foreach (Unit target in targets)
        { 
            ApplyHealEffect(skillCtx, target, healAmount, isReviveSkill);
        }
        
        TurnManager.ConsumeTurnsForHealSkill(skill, turnCtx);
        TurnManager.ConsumeTurn(turnCtx);
        
        return true;
    }
    
    private static List<Unit> GetTargetsForMultiHealSkill(SkillUseContext skillCtx)
    {
        Skill skill = skillCtx.Skill;
        Unit caster = skillCtx.Caster;
        
        Player attackerPlayer = skillCtx.Attacker;
        Player defenderPlayer = skillCtx.Defender;
        
        List<Unit> targets = new List<Unit>();
        
        switch (skill.Target)
        {
            case "All":
                AddAllUnitsToTargets(attackerPlayer, ref targets, caster);
                break;
                
            case "Party":
                AddPartyUnitsToTargets(attackerPlayer, ref targets, caster);
                break;
        }
        
        return targets;
    }
    
    private static void AddPartyUnitsToTargets(Player player, ref List<Unit> targets, Unit caster)
    {
        foreach (var unit in player.UnitManager.GetActiveUnits())
        {
            if (unit != null && IsUnitAlive(unit))
            {
                targets.Add(unit);
            }
        }
    }
    
    private static bool IsUnitAlive(Unit unit)
    {
        return unit.GetCurrentStats().GetStatByName("HP") > 0;
    }
    
    private static void AddAllUnitsToTargets(Player player, ref List<Unit> targets, Unit caster)
    {
        foreach (var unit in player.UnitManager.GetActiveUnits())
        {
            if (unit != null && unit != caster)
            {
                targets.Add(unit);
            }
        }
        
        foreach (var unit in player.UnitManager.GetReservedUnits())
        {
            if (unit != null)
            {
                targets.Add(unit);
            }
        }
    }
    
    public static bool IsMultiTargetHealSkill(Skill skill)
    {
        return GameConstants._stringForMultiTarget.Contains(skill.Target);
    }
    
    public static bool HandleSingleTargetHealSkill(SkillUseContext skillCtx, TurnContext turnCtx)
    {
        Player attackerPlayer = skillCtx.Attacker;
        PlayerUnitManager playerUnitManager = attackerPlayer.UnitManager;
        
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
                TurnManager.UpdateTurnStatesForDisplay(turnCtx);
                playerUnitManager.RearrangeSortedUnitsWhenAttacked();
                
                break;

            default:
                for (int i = 0; i < numberHits; i++)
                {
                    AffinityEffectManager.ApplyHeal(skillCtx);
                }

                usedSkill = true;
                
                CombatUI.DisplayCombatUiForSkill(skillCtx, affinityCtx, numberHits);
                TurnManager.ConsumeTurnsBasedOnAffinity(affinityCtx, turnCtx);
                TurnManager.UpdateTurnStatesForDisplay(turnCtx);
                playerUnitManager.RearrangeSortedUnitsWhenAttacked();
                break;
        }

        if (usedSkill)
        {
            SkillManager.ConsumeMP(skillCtx.Caster, skill.Cost);
        }
        
        return usedSkill;
    }
    
    private static bool HandleRecarmSkill(SkillUseContext skillCtx)
    {
        Player attackerPlayer = skillCtx.Attacker;
        Unit unitCaster = skillCtx.Caster;
        Unit unitTarget = skillCtx.Target;
        Skill skillUsing = skillCtx.Skill;
        
        PlayerUnitManager playerUnitManager = attackerPlayer.UnitManager;
        
        List<Unit> activeUnitsList = playerUnitManager.GetActiveUnits();
        
        CombatUI.DisplaySkillUsage(skillCtx.Caster, skillCtx.Skill, skillCtx.Target);
        double amountHealed = HealHalfTotalHp(skillCtx);
    
        if (activeUnitsList.Contains(unitTarget)) playerUnitManager.AddUnitInSortedList(skillCtx.Target);
    
        CombatUI.DisplayHealing(skillCtx.Target, amountHealed);
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
    
    private static void ApplyHealEffect(SkillUseContext skillCtx, Unit target, double healAmount, bool isReviveSkill)
    {
        Skill skill = skillCtx.Skill;
        Unit caster = skillCtx.Caster;
        // Unit target = skillCtx.Target;
        
        CombatUI.DisplaySkillUsage(caster, skill, target);
        
        int hpBeforeHeal = target.GetCurrentStats().GetStatByName("HP");
        bool isTargetDead = hpBeforeHeal <= 0;
        
        if ((isReviveSkill && isTargetDead) || (!isReviveSkill && !isTargetDead))
        {
            UnitActionManager.ApplyHealToUnit(target, healAmount);
            
            int hpAfterHeal = target.GetCurrentStats().GetStatByName("HP");
            int healedAmount = hpAfterHeal - hpBeforeHeal;
            
            if (isReviveSkill && isTargetDead)
            {
                CombatUI.DisplayRevive(caster, target, healAmount);
            }
            else
            {
                CombatUI.DisplayHealing(target, healAmount);
            }
            
            // CombatUI.DisplayFinalHP(target);
        }
    }
}