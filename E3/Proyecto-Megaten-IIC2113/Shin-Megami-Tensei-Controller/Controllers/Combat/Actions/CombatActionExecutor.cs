using Shin_Megami_Tensei;
using Shin_Megami_Tensei.Combat;
using Shin_Megami_Tensei.Gadgets;
using Shin_Megami_Tensei.Managers;
using Shin_Megami_Tensei.Managers.Helpers;

public static class CombatActionExecutor
{
    public static bool ExecuteAttack(AttackTargetContext attackContext, TurnContext turnContext)
    {
        var targetResult = SelectAttackTarget(attackContext);
        if (!targetResult.WasSelected)
            return false;

        var attackData = new AttackData(attackContext.Attacker, targetResult.SelectedTarget, attackContext.AttackType);
        DisplayAndProcessAttack(attackData, turnContext);
        return true;
    }

    public static bool ExecuteSkill(Unit caster, CombatContext combatContext, TurnContext turnContext)
    {
        var skillResult = SelectSkillToUse(caster);
        if (!skillResult.WasSelected)
            return false;

        var executionResult = ProcessSelectedSkill(skillResult.SelectedSkill, caster, combatContext, turnContext);
        
        if (executionResult.WasSuccessful)
            IncrementSkillCounter(turnContext);

        return executionResult.WasSuccessful;
    }

    public static bool ExecuteSummon(Player currentPlayer, Unit currentUnit, TurnContext turnContext, SummonType summonType)
    {
        var summonResult = PerformSummonAction(currentPlayer, currentUnit, summonType);
        
        if (summonResult.WasSuccessful)
            SummonManager.ManageTurnsWhenSummoned(turnContext);

        return summonResult.WasSuccessful;
    }

    public static bool ExecutePassTurn(TurnContext turnContext)
    {
        TurnManager.ManageTurnsWhenPassedTurn(turnContext);
        return true;
    }

    private static AttackTargetResult SelectAttackTarget(AttackTargetContext attackContext)
    {
        Player opponent = attackContext.Opponent;
        PlayerCombatState playerCombatState = opponent.CombatState;
        int optionsCount = opponent.UnitManager.GetValidActiveUnits().Count;
        
        string targetInput = TargetSelector.SelectEnemy(attackContext);
        
        if (playerCombatState.IsPlayerCancelling(targetInput, optionsCount))
            return AttackTargetResult.Cancelled();

        var selectedTarget = TargetSelector.ResolveTarget(attackContext.Opponent, targetInput);
        return AttackTargetResult.Selected(selectedTarget);
    }

    private static void DisplayAndProcessAttack(AttackData attackData, TurnContext turnContext)
    {
        CombatUI.DisplayAttack(attackData.AttackerName, attackData.TargetName, attackData.AttackType);

        var affinityContext = CalculateAndCreateAffinityContext(attackData);
        AffinityEffectManager.ApplyEffectForBasicAttack(affinityContext);
        UpdateGameStateAfterAttack(affinityContext, turnContext);
    }

    private static AffinityContext CalculateAndCreateAffinityContext(AttackData attackData)
    {
        double baseDamage = CalculateBaseDamageByType(attackData);
        return new AffinityContext(attackData.Attacker, attackData.Target, attackData.AttackType, baseDamage);
    }

    private static double CalculateBaseDamageByType(AttackData attackData)
    {
        return attackData.AttackType == "Phys"
            ? AttackExecutor.ExecutePhysicalAttack(attackData.Attacker, attackData.Target, GameConstants.ModifierPhysDamage)
            : AttackExecutor.ExecuteGunAttack(attackData.Attacker, attackData.Target, GameConstants.ModifierGunDamage);
    }

    private static void UpdateGameStateAfterAttack(AffinityContext affinityContext, TurnContext turnContext)
    {
        TurnManager.ConsumeTurnsBasedOnAffinity(affinityContext, turnContext);
        TurnManager.UpdateTurnStatesForDisplay(turnContext);
        turnContext.Attacker.UnitManager.RearrangeSortedUnitsWhenAttacked();
    }

    private static SkillSelectionResult SelectSkillToUse(Unit caster)
    {
        CombatUI.DisplaySkillSelectionPrompt(caster.GetName());
        
        var selectedSkill = SkillManager.SelectSkill(caster);
        
        return selectedSkill == null 
            ? SkillSelectionResult.Cancelled() 
            : SkillSelectionResult.Selected(selectedSkill);
    }

    private static SkillExecutionResult ProcessSelectedSkill(Skill skill, Unit caster, CombatContext combatContext, TurnContext turnContext)
    {
        return skill.Type switch
        {
            "Special" => HandleSpecialSkill(skill, caster, turnContext),
            "Heal" => HandleHealSkill(skill, caster, combatContext, turnContext),
            _ => HandleDamageSkill(skill, caster, combatContext, turnContext)
        };
    }

    private static SkillExecutionResult HandleSpecialSkill(Skill skill, Unit caster, TurnContext turnContext)
    {
        var skillUseContext = SkillUseContext.CreateSkillContext(caster, null, skill, turnContext);
        bool skillUsed = SkillManager.HandleSpecialSkill(skillUseContext, turnContext);

        if (skillUsed)
            DisplayTurnChangesForSpecialSkills(turnContext);

        return new SkillExecutionResult(skillUsed);
    }

    private static SkillExecutionResult HandleHealSkill(Skill skill, Unit caster, CombatContext combatContext, TurnContext turnContext)
    {
        var targetResult = SelectTargetForHealSkill(skill, caster, combatContext);
        if (!targetResult.WasSelected)
            return SkillExecutionResult.Failed();

        var skillUseContext = SkillUseContext.CreateSkillContext(caster, targetResult.SelectedTarget, skill, turnContext);
        bool skillUsed = SkillManager.HandleHealSkills(skillUseContext, turnContext);
        
        if (skillUsed)
            FinalizeSkillExecution(caster, skill, turnContext);

        return new SkillExecutionResult(skillUsed);
    }

    private static SkillExecutionResult HandleDamageSkill(Skill skill, Unit caster, CombatContext combatContext, TurnContext turnContext)
    {
        var targetResult = SelectTargetForDamageSkill(skill, caster, combatContext);
        if (!targetResult.WasSelected)
            return SkillExecutionResult.Failed();

        var skillUseContext = SkillUseContext.CreateSkillContext(caster, targetResult.SelectedTarget, skill, turnContext);
        bool skillUsed = SkillManager.HandleDamageSkills(skillUseContext, turnContext);

        if (skillUsed)
            FinalizeSkillExecution(caster, skill, turnContext);

        return new SkillExecutionResult(skillUsed);
    }

    private static SkillTargetResult SelectTargetForHealSkill(Skill skill, Unit caster, CombatContext combatContext)
    {
        if (!DoesHealSkillRequireTargetSelection(skill))
            return SkillTargetResult.Selected(null);

        return SelectSkillTarget(skill, caster, combatContext);
    }

    private static SkillTargetResult SelectTargetForDamageSkill(Skill skill, Unit caster, CombatContext combatContext)
    {
        if (IsMultiTargetSkill(skill))
            return SkillTargetResult.Selected(null);

        return SelectSkillTarget(skill, caster, combatContext);
    }

    private static bool DoesHealSkillRequireTargetSelection(Skill skill)
    {
        var skillsWithoutTargetSelection = GameConstants._skillsThatDontNeedSelectObjective;
        
        if (skill.Name == "Invitation" || skill.Target == "Multi")
            return false;
        
        return !skillsWithoutTargetSelection.Contains(skill.Name) || 
               (skill.Target != "All" && skill.Target != "Party");
    }

    private static bool IsMultiTargetSkill(Skill skill)
    {
        return skill.Target is "All" or "Party" or "Multi";
    }

    private static SkillTargetResult SelectSkillTarget(Skill skill, Unit caster, CombatContext combatContext)
    {
        var targetContext = new SkillTargetContext(skill, combatContext.CurrentPlayer, combatContext.Opponent, combatContext.View);
        var selectedTarget = TargetSelector.SelectSkillTarget(targetContext, caster);
        
        return selectedTarget == null 
            ? SkillTargetResult.Cancelled() 
            : SkillTargetResult.Selected(selectedTarget);
    }

    private static void FinalizeSkillExecution(Unit caster, Skill skill, TurnContext turnContext)
    {
        SkillManager.ConsumeMP(caster, skill.Cost);
        UpdateGameStateAfterSkill(turnContext);
    }

    private static void UpdateGameStateAfterSkill(TurnContext turnContext)
    {
        TurnManager.UpdateTurnStatesForDisplay(turnContext);
        turnContext.Attacker.UnitManager.RearrangeSortedUnitsWhenAttacked();
    }

    private static void IncrementSkillCounter(TurnContext turnContext)
    {
        turnContext.Attacker.TurnManager.IncreaseConstantKPlayer();
    }

    private static SummonExecutionResult PerformSummonAction(Player currentPlayer, Unit currentUnit, SummonType summonType)
    {
        bool success = summonType == SummonType.Samurai 
            ? SummonManager.SummonFromReserveBySamurai(currentPlayer)
            : SummonManager.MonsterSwap(currentPlayer, (Demon)currentUnit);
            
        return new SummonExecutionResult(success);
    }

    private static void DisplayTurnChangesForSpecialSkills(TurnContext turnContext)
    {
        var turnManager = turnContext.Attacker.TurnManager;
        var opponentUnitManager = turnContext.Defender.UnitManager;
        var opponentTeamManager = turnContext.Defender.TeamManager;
        
        int currentFull = turnManager.GetFullTurns();
        int currentBlinking = turnManager.GetBlinkingTurns();

        int fullConsumed = turnContext.FullStart - currentFull;
        int blinkingConsumed = Math.Max(0, turnContext.BlinkStart - currentBlinking);
        int blinkingGained = Math.Max(0, currentBlinking - turnContext.BlinkStart);

        CombatUI.DisplayTurnChanges(fullConsumed, blinkingConsumed, blinkingGained);

        opponentUnitManager.RemoveFromActiveUnitsIfDead();
        opponentTeamManager.ReorderReserveBasedOnJsonOrder();
    }
}