using Shin_Megami_Tensei_View;
using Shin_Megami_Tensei.Combat;
using Shin_Megami_Tensei.Gadgets;
using Shin_Megami_Tensei.Managers;

namespace Shin_Megami_Tensei.Controllers;

public static class CombatActionExecutor
{
    public static bool ExecuteAttack(Unit attacker, Player opponent, View view, string attackType, TurnContext turnCtx)
    {
        Unit? target = SelectAttackTarget(attacker, opponent, view);
        if (target == null) return false;

        CombatUI.DisplayAttack(attacker.GetName(), target.GetName(), attackType);

        var affinityCtx = ApplyAttackAndGetAffinityContext(attacker, target, attackType);
        AffinityEffectManager.ApplyEffectForBasicAttack(affinityCtx);
        UpdateGameStateAfterAttack(affinityCtx, turnCtx);

        return true;
    }

    private static Unit? SelectAttackTarget(Unit attacker, Player opponent, View view)
    {
        var targetCtx = new AttackTargetContext(attacker, opponent, view);
        PlayerUnitManager unitManager = opponent.UnitManager;
        
        string targetInput = TargetSelector.SelectEnemy(targetCtx, opponent);
        int cancelNum = unitManager.GetValidActiveUnits().Count + 1;

        if (targetInput == cancelNum.ToString())
            return null;

        return TargetSelector.ResolveTarget(opponent, targetInput);
    }

    private static AffinityContext ApplyAttackAndGetAffinityContext(Unit attacker, Unit target, string attackType)
    {
        double baseDamage = CalculateBaseDamage(attacker, target, attackType);
        return new AffinityContext(attacker, target, attackType, baseDamage);
    }

    private static double CalculateBaseDamage(Unit attacker, Unit target, string attackType)
    {
        return attackType == "Phys"
            ? AttackExecutor.ExecutePhysicalAttack(attacker, target, GameConstants.ModifierPhysDamage)
            : AttackExecutor.ExecuteGunAttack(attacker, target, GameConstants.ModifierGunDamage);
    }

    private static void UpdateGameStateAfterAttack(AffinityContext affinityCtx, TurnContext turnCtx)
    {
        TurnManager.ConsumeTurnsBasedOnAffinity(affinityCtx, turnCtx);
        TurnManager.UpdateTurnStatesForDisplay(turnCtx);
        turnCtx.Attacker.UnitManager.RearrangeSortedUnitsWhenAttacked();
    }

    public static bool ExecuteSkill(Unit caster, Player currentPlayer, Player opponent, View view, TurnContext turnCtx)
    {
        CombatUI.DisplaySkillSelectionPrompt(caster.GetName());
        
        Skill? skill = SkillManager.SelectSkill(view, caster);
        if (skill == null) return false;

        bool skillUsed = ProcessSkillByType(skill, caster, currentPlayer, opponent, view, turnCtx);
        
        if (skillUsed)
        {
            turnCtx.Attacker.TurnManager.IncreaseConstantKPlayer();
        }

        return skillUsed;
    }

    private static bool ProcessSkillByType(Skill skill, Unit caster, Player currentPlayer, Player opponent, View view, TurnContext turnCtx)
    {
        switch (skill.Type)
        {
            case "Special":
                return HandleSpecialSkill(skill, caster, currentPlayer, opponent, turnCtx);
            case "Heal":
                return HandleHealSkill(skill, caster, currentPlayer, opponent, view, turnCtx);
            default:
                return HandleDamageSkill(skill, caster, currentPlayer, opponent, view, turnCtx);
        }
    }

    private static bool HandleSpecialSkill(Skill skill, Unit caster, Player currentPlayer, Player opponent, TurnContext turnCtx)
    {
        var skillCtx = SkillUseContext.CreateSkillContext(caster, null, skill, turnCtx);
        bool skillUsed = SkillManager.HandleSpecialSkill(skillCtx, turnCtx);

        if (skillUsed)
        {
            DisplayTurnChangesOnly(turnCtx);
        }

        return skillUsed;
    }

    private static void DisplayTurnChangesOnly(TurnContext turnCtx)
    {
        PlayerTurnManager turnManagerPlayer = turnCtx.Attacker.TurnManager;
        PlayerUnitManager unitManagerOpponent = turnCtx.Defender.UnitManager;
        PlayerTeamManager teamManagerOpponent = turnCtx.Defender.TeamManager;
        
        int fullNow = turnManagerPlayer.GetFullTurns();
        int blinkNow = turnManagerPlayer.GetBlinkingTurns();

        int fullConsumed = turnCtx.FullStart - fullNow;
        int blinkingConsumed = Math.Max(0, turnCtx.BlinkStart - blinkNow);
        int blinkingGained = Math.Max(0, blinkNow - turnCtx.BlinkStart);

        CombatUI.DisplayTurnChanges(fullConsumed, blinkingConsumed, blinkingGained);

        unitManagerOpponent.RemoveFromActiveUnitsIfDead();
        teamManagerOpponent.ReorderReserveBasedOnJsonOrder();
    }

    private static bool HandleHealSkill(Skill skill, Unit caster, Player currentPlayer, Player opponent, View view, TurnContext turnCtx)
    {
        Unit? target = null;
        
        if (RequiresTargetSelection(skill))
        {
            target = SelectSkillTarget(skill, caster, currentPlayer, opponent, view);
            if (target == null)
            {
                return false;
            }
        }

        var skillCtx = SkillUseContext.CreateSkillContext(caster, target, skill, turnCtx);
        bool skillUsed = SkillManager.HandleHealSkills(skillCtx, turnCtx);
        
        if (skillUsed)
        {
            SkillManager.ConsumeMP(skillCtx.Caster, skillCtx.Skill.Cost);
            UpdateGameStateAfterSkill(turnCtx);
        }

        return skillUsed;
    }

    private static bool HandleDamageSkill(Skill skill, Unit caster, Player currentPlayer, Player opponent, View view, TurnContext turnCtx)
    {
        Unit? target = null;

        if (skill.Target != "All" && skill.Target != "Party" && skill.Target != "Multi")
        {
            target = SelectSkillTarget(skill, caster, currentPlayer, opponent, view);
        
            if (target == null)
            {
                return false;
            }
        }

        var skillCtx = SkillUseContext.CreateSkillContext(caster, target, skill, turnCtx);
        bool skillUsed = SkillManager.HandleDamageSkills(skillCtx, turnCtx);

        if (skillUsed)
        {
            SkillManager.ConsumeMP(skillCtx.Caster, skillCtx.Skill.Cost);
            UpdateGameStateAfterSkill(turnCtx);
        }

        return skillUsed;
    }

    private static bool RequiresTargetSelection(Skill skill)
    {
        var skillNamesNeedSelectTarget = GameConstants._skillsThatDontNeedSelectObjective;
    
        if (skill.Name == "Invitation")
            return false;
        
        if (skill.Target == "Multi")
            return false;
        
        return !skillNamesNeedSelectTarget.Contains(skill.Name) || 
               (skill.Target != "All" && skill.Target != "Party");
    }

    private static Unit? SelectSkillTarget(Skill skill, Unit caster, Player currentPlayer, Player opponent, View view)
    {
        var targetCtx = new SkillTargetContext(skill, currentPlayer, opponent, view);
        return TargetSelector.SelectSkillTarget(targetCtx, caster);
    }

    private static void UpdateGameStateAfterSkill(TurnContext turnCtx)
    {
        TurnManager.UpdateTurnStatesForDisplay(turnCtx);
        turnCtx.Attacker.UnitManager.RearrangeSortedUnitsWhenAttacked();
    }


    public static bool ExecuteSummon(Player currentPlayer, Unit currentUnit, TurnContext turnCtx, bool isSamurai = false)
    {
        bool hasSummoned = isSamurai 
            ? SummonManager.SummonFromReserveBySamurai(currentPlayer)
            : SummonManager.MonsterSwap(currentPlayer, (Demon)currentUnit);

        if (hasSummoned)
        {
            SummonManager.ManageTurnsWhenSummoned(turnCtx);
        }

        return hasSummoned;
    }


    public static bool ExecutePassTurn(TurnContext turnCtx)
    {
        TurnManager.ManageTurnsWhenPassedTurn(turnCtx);
        return true;
    }

}