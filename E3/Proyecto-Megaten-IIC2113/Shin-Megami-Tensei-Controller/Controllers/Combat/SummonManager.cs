using System.Diagnostics.Contracts;
using Shin_Megami_Tensei_View;
using Shin_Megami_Tensei.Combat;
using Shin_Megami_Tensei.Gadgets;

namespace Shin_Megami_Tensei.Managers;

public static class SummonManager
{
    public static void ManageTurnsWhenSummoned(TurnContext turnContext)
    {
        TurnManager.ConsumeTurnsWhenPassedTurn(turnContext);
        TurnManager.UpdateTurnStatesForDisplay(turnContext);
    }

    public static bool SummonFromReserveBySamurai(Player currentPlayer)
    {
        var demonSelectionResult = HandleDemonSelection(currentPlayer);
        if (!demonSelectionResult.Success)
            return false;

        var slotSelectionResult = HandleSlotSelection(currentPlayer);
        if (!slotSelectionResult.Success)
            return false;

        CompleteSummonProcess(currentPlayer, demonSelectionResult.SelectedDemon, slotSelectionResult.SelectedSlot);
        return true;
    }

    public static bool MonsterSwap(Player currentPlayer, Demon activeDemon)
    {
        var demonSelectionResult = HandleDemonSelectionForSwap(currentPlayer);
        if (!demonSelectionResult.Success)
            return false;

        int targetSlot = FindActiveDemonSlot(currentPlayer, activeDemon);
        CompleteSummonProcess(currentPlayer, demonSelectionResult.SelectedDemon, targetSlot);
        return true;
    }

    public static bool SummonBySkillInvitation(SkillUseContext skillContext, TurnContext turnContext)
    {
        var demonSelectionResult = HandleDemonSelectionForInvitation(skillContext.Attacker);
        if (!demonSelectionResult.Success)
            return false;

        skillContext.Target = demonSelectionResult.SelectedDemon;

        var slotSelectionResult = HandleSlotSelection(skillContext.Attacker);
        if (!slotSelectionResult.Success)
            return false;

        CompleteInvitationProcess(skillContext, slotSelectionResult.SelectedSlot, turnContext);
        return true;
    }

    private static DemonSelectionResult HandleDemonSelection(Player currentPlayer)
    {
        PlayerCombatState playerCombatState = currentPlayer.CombatState;
        var availableDemons = GetLivingReserveDemons(currentPlayer);
        
        DisplaySummonSelectionInterface(availableDemons);
        string playerInput = GetPlayerInputWithSeparator();

        if (playerCombatState.IsPlayerCancelling(playerInput, availableDemons.Count))
            return DemonSelectionResult.Cancelled();

        var selectedDemon = SelectDemonFromLivingReserve(currentPlayer, playerInput);
        return DemonSelectionResult.FromSuccess(selectedDemon);
    }

    private static DemonSelectionResult HandleDemonSelectionForSwap(Player currentPlayer)
    {
        PlayerCombatState playerCombatState = currentPlayer.CombatState;
        var availableDemons = GetLivingReserveDemons(currentPlayer);
        
        DisplaySummonSelectionInterface(availableDemons);
        string playerInput = GetPlayerInputWithSeparator();

        if (playerCombatState.IsPlayerCancelling(playerInput, availableDemons.Count))
            return DemonSelectionResult.Cancelled();

        var selectedDemon = SelectFromLivingDemons(availableDemons, playerInput);
        return DemonSelectionResult.FromSuccess(selectedDemon);
    }

    private static DemonSelectionResult HandleDemonSelectionForInvitation(Player currentPlayer)
    {
        PlayerCombatState playerCombatState = currentPlayer.CombatState;
        var allReserveDemons = currentPlayer.UnitManager.GetReservedUnits();
        
        DisplayInvitationSelectionInterface(allReserveDemons);
        string playerInput = GetPlayerInputWithSeparator();
        
        if (playerCombatState.IsPlayerCancelling(playerInput, allReserveDemons.Count))
            return DemonSelectionResult.Cancelled();

        var selectedDemon = SelectFromAllReserveDemons(allReserveDemons, playerInput);
        return DemonSelectionResult.FromSuccess(selectedDemon);
    }

    private static SlotSelectionResult HandleSlotSelection(Player currentPlayer)
    {
        var availableSlots = DisplaySlotSelectionAndGetOptions(currentPlayer);
        string playerInput = GetPlayerInputWithSeparator();

        PlayerCombatState playerCombatState = currentPlayer.CombatState;
        if (playerCombatState.IsPlayerCancelling(playerInput, availableSlots.Count))
        {
            return SlotSelectionResult.Cancelled();
        }

        int selectedSlot = ConvertToSlotIndex(availableSlots, playerInput);
        return SlotSelectionResult.FromSuccess(selectedSlot);
    }

    private static void CompleteSummonProcess(Player currentPlayer, Unit selectedDemon, int targetSlot)
    {
        ExecuteSummonOperation(currentPlayer, selectedDemon, targetSlot);
        FinalizeUnitArrangement(currentPlayer);
        CombatUI.DisplaySeparator();
    }

    private static void CompleteInvitationProcess(SkillUseContext skillContext, int targetSlot, TurnContext turnContext)
    {
        ExecuteSummonOperation(skillContext.Attacker, skillContext.Target, targetSlot);
        
        ProcessResurrectionIfNeeded(skillContext);
        TurnManager.UpdateTurnsForInvocationSkill(turnContext);
    }

    private static void ProcessResurrectionIfNeeded(SkillUseContext skillContext)
    {
        bool wasResurrected = AttemptDemonResurrection(skillContext.Target);
        
        if (wasResurrected)
            DisplayResurrectionEffects(skillContext);
        else
            CombatUI.DisplaySeparator();
    }

    private static void DisplayResurrectionEffects(SkillUseContext skillContext)
    {
        CombatUI.DisplaySkillUsage(skillContext.Caster, skillContext.Skill, skillContext.Target);
        double healingAmount = HealSkillsManager.CalculateHeal(skillContext.Target, skillContext);
        CombatUI.DisplayHealingForSingleTarget(skillContext.Target, healingAmount);
        CombatUI.DisplaySeparator();
    }

    private static List<Unit> GetLivingReserveDemons(Player currentPlayer)
    {
        return currentPlayer.UnitManager.GetReservedUnits()
            .Where(demon => IsUnitAlive(demon))
            .ToList();
    }

    private static void DisplaySummonSelectionInterface(List<Unit> availableDemons)
    {
        CombatUI.DisplaySummonPrompt();
        CombatUI.DisplaySummonOptions(availableDemons);
    }

    private static void DisplayInvitationSelectionInterface(List<Unit> allDemons)
    {
        CombatUI.DisplaySummonPrompt();
        CombatUI.DisplaySummonOptionsIncludingDead(allDemons);
    }

    private static List<int> DisplaySlotSelectionAndGetOptions(Player currentPlayer)
    {
        CombatUI.DisplaySlotSelectionPrompt();
        return currentPlayer.CombatState.GetValidSlotsFromActiveUnitsAndDisplayIt();
    }

    private static string GetPlayerInputWithSeparator()
    {
        return CombatUI.GetUserInput();
    }

    private static Unit SelectDemonFromLivingReserve(Player currentPlayer, string playerInput)
    {
        var fullReserve = currentPlayer.UnitManager.GetReservedUnits();
        var livingReserve = fullReserve.Where(demon => IsUnitAlive(demon)).ToList();
        
        int demonIndex = ConvertToZeroBasedIndex(playerInput);
        return livingReserve[demonIndex];
    }

    private static Unit SelectFromLivingDemons(List<Unit> livingDemons, string playerInput)
    {
        int demonIndex = ConvertToZeroBasedIndex(playerInput);
        Unit selectedDemon = livingDemons[demonIndex];
        Console.WriteLine(selectedDemon.GetName());
        return selectedDemon;
    }

    private static Unit SelectFromAllReserveDemons(List<Unit> allDemons, string playerInput)
    {
        int demonIndex = ConvertToZeroBasedIndex(playerInput);
        return allDemons[demonIndex];
    }

    private static int ConvertToSlotIndex(List<int> validSlots, string playerInput)
    {
        int slotIndex = ConvertToZeroBasedIndex(playerInput);
        return validSlots[slotIndex];
    }

    private static int ConvertToZeroBasedIndex(string oneBasedInput)
    {
        return Convert.ToInt32(oneBasedInput) - 1;
    }

    private static bool IsUnitAlive(Unit unit)
    {
        return unit != null && unit.GetCurrentStats().GetStatByName("HP") > 0;
    }

    private static bool AttemptDemonResurrection(Unit targetDemon)
    {
        int currentHP = targetDemon.GetCurrentStats().GetStatByName("HP");
        if (currentHP <= 0)
        {
            int maxHP = targetDemon.GetBaseStats().GetStatByName("HP");
            targetDemon.GetCurrentStats().SetStatByName("HP", maxHP);
            return true;
        }
        return false;
    }

    public static void ExecuteSummonOperation(Player currentPlayer, Unit newDemon, int targetSlot)
    {
        Unit replacedDemon = GetDemonInSlot(currentPlayer, targetSlot);
        
        PlaceDemonInActiveSlot(currentPlayer, newDemon, targetSlot);
        UpdateReserveAfterSummon(currentPlayer, newDemon, replacedDemon);
        UpdateSortedUnitsAfterSummon(currentPlayer, newDemon, replacedDemon);
        
        CombatUI.DisplayHasBeenSummoned(newDemon);
    }

    private static Unit GetDemonInSlot(Player currentPlayer, int slotIndex)
    {
        return currentPlayer.UnitManager.GetActiveUnits()[slotIndex];
    }

    private static void PlaceDemonInActiveSlot(Player currentPlayer, Unit newDemon, int slotIndex)
    {
        currentPlayer.UnitManager.GetActiveUnits()[slotIndex] = newDemon;
    }

    private static void UpdateReserveAfterSummon(Player currentPlayer, Unit newDemon, Unit replacedDemon)
    {
        var unitManager = currentPlayer.UnitManager;
        var teamManager = currentPlayer.TeamManager;
        
        if (replacedDemon != null)
        {
            unitManager.ReplaceFromReserveUnitsList(newDemon.GetName(), (Demon)replacedDemon);
        }

        teamManager.ReorderReserveBasedOnJsonOrder();
        unitManager.GetReservedUnits().Remove(newDemon);
    }

    private static void UpdateSortedUnitsAfterSummon(Player currentPlayer, Unit newDemon, Unit replacedDemon)
    {
        var unitManager = currentPlayer.UnitManager;
        
        if (replacedDemon != null)
        {
            unitManager.ReplaceFromSortedListWhenInvoked(replacedDemon, newDemon);
        }
        else
        {
            unitManager.AddDemonInTheLastSlot((Demon)newDemon);
        }
    }

    private static void FinalizeUnitArrangement(Player currentPlayer)
    {
        currentPlayer.UnitManager.RearrangeSortedUnitsWhenAttacked();
    }

    public static int FindActiveDemonSlot(Player currentPlayer, Unit targetDemon)
    {
        var activeUnits = currentPlayer.UnitManager.GetActiveUnits();
        
        for (int slotIndex = 0; slotIndex < activeUnits.Count; slotIndex++)
        {
            if (activeUnits[slotIndex] == targetDemon)
                return slotIndex;
        }
        
        return -1;
    }
}