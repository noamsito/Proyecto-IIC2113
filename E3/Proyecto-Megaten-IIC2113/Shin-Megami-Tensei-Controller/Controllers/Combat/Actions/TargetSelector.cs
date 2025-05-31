using Shin_Megami_Tensei_View;
using Shin_Megami_Tensei.Combat;
using Shin_Megami_Tensei.Gadgets;

namespace Shin_Megami_Tensei.Managers;

public static class TargetSelector
{
    private const int CANCEL_OPTION_OFFSET = 1;

    public static string SelectEnemy(AttackTargetContext attackContext)
    {
        Player opponent = attackContext.Opponent;
        PlayerCombatState combatState = opponent.CombatState;
        var availableEnemies = GetAvailableEnemies(attackContext.Opponent);
        
        ShowTargetSelectionMenu(attackContext.Attacker.GetName());
        DisplayTargetsAndCancelOption(availableEnemies);
        
        return combatState.GetPlayerInputWithSeparator();
    }

    public static Unit ResolveTarget(Player opponent, string playerInput)
    {
        int targetIndex = ConvertToZeroBasedIndex(playerInput);
        var availableTargets = GetAvailableEnemies(opponent);
        return availableTargets[targetIndex];
    }

    public static Unit? SelectSkillTarget(SkillTargetContext skillContext, Unit attackingUnit)
    {
        PlayerCombatState combatState = skillContext.CurrentPlayer.CombatState;;
        ShowTargetSelectionMenu(attackingUnit.GetName());

        var possibleTargets = DeterminePossibleTargets(skillContext);
        DisplayTargetsAndCancelOption(possibleTargets);

        string playerInput = combatState.GetPlayerInputWithSeparator();

        if (IsPlayerCancellingAction(playerInput, possibleTargets.Count))
            return null;
        
        return SelectTargetFromList(possibleTargets, playerInput);
    }

    private static List<Unit> GetAvailableEnemies(Player opponent)
    {
        return opponent.UnitManager.GetValidActiveUnits();
    }

    private static void ShowTargetSelectionMenu(string attackerName)
    {
        CombatUI.DisplaySelectTarget(attackerName);
    }

    private static void DisplayTargetsAndCancelOption(List<Unit> targets)
    {
        CombatUI.DisplayUnitsGiven(targets);
        CombatUI.DisplayCancelOption(targets.Count);
    }

    private static int ConvertToZeroBasedIndex(string oneBasedInput)
    {
        return Convert.ToInt32(oneBasedInput) - 1;
    }

    private static List<Unit> DeterminePossibleTargets(SkillTargetContext context)
    {
        bool isAllyTargeting = context.Skill.Target == "Ally";
        bool canReviveUnits = IsReviveSkill(context.Skill.Name);

        if (isAllyTargeting)
        {
            return canReviveUnits 
                ? GetDeadAllies(context.CurrentPlayer.UnitManager)
                : GetLivingAllies(context.CurrentPlayer.UnitManager);
        }

        return GetAvailableEnemies(context.Opponent);
    }

    private static bool IsReviveSkill(string skillName)
    {
        return skillName is "Recarm" or "Samarecarm" or "Invitation";
    }

    private static List<Unit> GetDeadAllies(PlayerUnitManager unitManager)
    {
        var deadActiveUnits = GetDeadUnitsFromCollection(unitManager.GetActiveUnits());
        var deadReservedUnits = GetDeadUnitsFromCollection(unitManager.GetReservedUnits());
        
        return deadActiveUnits.Concat(deadReservedUnits).ToList();
    }

    private static List<Unit> GetLivingAllies(PlayerUnitManager unitManager)
    {
        return GetLivingUnitsFromCollection(unitManager.GetActiveUnits());
    }

    private static List<Unit> GetDeadUnitsFromCollection(IEnumerable<Unit> units)
    {
        return units
            .Where(unit => unit != null && IsUnitDead(unit))
            .ToList();
    }

    private static List<Unit> GetLivingUnitsFromCollection(IEnumerable<Unit> units)
    {
        return units
            .Where(unit => unit != null && IsUnitAlive(unit))
            .ToList();
    }

    private static bool IsUnitDead(Unit unit)
    {
        return unit.GetCurrentStats().GetStatByName("HP") <= 0;
    }

    private static bool IsUnitAlive(Unit unit)
    {
        return unit.GetCurrentStats().GetStatByName("HP") > 0;
    }

    private static bool IsPlayerCancellingAction(string input, int totalOptions)
    {
        return input == $"{totalOptions + CANCEL_OPTION_OFFSET}";
    }

    private static Unit SelectTargetFromList(List<Unit> targets, string playerInput)
    {
        int targetIndex = ConvertToZeroBasedIndex(playerInput);
        return targets[targetIndex];
    }

    public static string FormatUnitStatus(Unit unit)
    {
        var currentStats = unit.GetCurrentStats();
        var baseStats = unit.GetBaseStats();

        int currentHP = currentStats.GetStatByName("HP");
        int maxHP = baseStats.GetStatByName("HP");
        int currentMP = currentStats.GetStatByName("MP");
        int maxMP = baseStats.GetStatByName("MP");

        return $"HP:{currentHP}/{maxHP} MP:{currentMP}/{maxMP}";
    }
}