using Shin_Megami_Tensei_View;
using Shin_Megami_Tensei.Combat;
using Shin_Megami_Tensei.Gadgets;

namespace Shin_Megami_Tensei.Managers;

public static class SummonManager
{
    public static void ManageTurnsWhenSummoned(TurnContext turnCtx)
    {
        TurnManager.ConsumeTurnsWhenPassedTurn(turnCtx);
        TurnManager.UpdateTurnStatesForDisplay(turnCtx);
    }

    public static bool SummonFromReserveBySamurai(Player player, View view)
    {
        CombatUI.DisplaySummonPrompt();

        List<Unit> reserve = player.GetReservedUnits();
        var aliveReserve = reserve.Where(unit => unit.GetCurrentStats().GetStatByName("HP") > 0).ToList();

        CombatUI.DisplaySummonOptions(aliveReserve);

        string demonInput = CombatUI.GetUserInput();
        CombatUI.DisplaySeparator();

        if (IsCancelOption(demonInput, aliveReserve.Count))
            return false;

        Unit selectedDemon = SelectDemonFromFilteredReserve(reserve, demonInput);

        CombatUI.DisplaySlotSelectionPrompt();

        List<int> validSlots = player.GetValidSlotsFromActiveUnitsAndDisplayIt();
        string slotInput = CombatUI.GetUserInput();

        if (IsCancelOption(slotInput, validSlots.Count))
            return false;

        int slot = SelectSlot(validSlots, slotInput);

        SummonDemon(player, selectedDemon, slot);
        return true;
    }

    public static void MonsterSwap(Player player, Demon demonSummoned)
    {
        CombatUI.DisplaySummonPrompt();

        List<Unit> reserve = player.GetReservedUnits();
        var aliveReserve = reserve.Where(unit => unit.GetCurrentStats().GetStatByName("HP") > 0).ToList();

        CombatUI.DisplaySummonOptions(aliveReserve);

        string input = CombatUI.GetUserInput();
        if (IsCancelOption(input, aliveReserve.Count))
            return;

        Demon selectedDemon = (Demon)SelectDemonFromFilteredReserve(reserve, input);

        int slotToReplace = FindSlotOfActiveDemon(player, demonSummoned);

        SummonDemon(player, selectedDemon, slotToReplace);
    }

    private static bool IsCancelOption(string input, int count)
    {
        return input == $"{count + 1}";
    }

    private static Unit SelectDemonFromFilteredReserve(List<Unit> fullReserve, string input)
    {
        var aliveReserve = fullReserve
            .Where(unit => unit != null && unit.GetCurrentStats().GetStatByName("HP") > 0)
            .ToList();

        int index = ParseInputToIndex(input);
        return aliveReserve[index];
    }


    private static int ParseInputToIndex(string input)
    {
        return Convert.ToInt32(input) - 1;
    }

    private static int SelectSlot(List<int> validSlots, string input)
    {
        int slotIndex = ParseInputToIndex(input);
        return validSlots[slotIndex];
    }

    private static void SummonDemon(Player player, Unit newDemonAddedToActiveList, int slot)
    {
        Unit removedDemonFromActiveList = GetDemonToReplace(player, slot);

        ReplaceActiveSlot(player, newDemonAddedToActiveList, slot);

        UpdateReserveAfterSummon(player, newDemonAddedToActiveList, removedDemonFromActiveList);

        UpdateSortedListAfterSummon(player, newDemonAddedToActiveList, removedDemonFromActiveList);

        player.ReorderUnitsWhenAttacked();
        CombatUI.DisplayHasBeenSummoned(newDemonAddedToActiveList);
    }

    private static Unit GetDemonToReplace(Player player, int slot)
    {
        return player.GetActiveUnits()[slot];
    }

    private static void ReplaceActiveSlot(Player player, Unit newDemon, int slot)
    {
        player.GetActiveUnits()[slot] = newDemon;
    }


    private static void UpdateReserveAfterSummon(Player player, Unit newDemon, Unit removedDemon)
    {
        if (removedDemon != null)
        {
            player.ReplaceFromReserveUnitsList(newDemon.GetName(), (Demon)removedDemon);
        }

        player.ReorderReserveBasedOnJsonOrder();
        player.GetReservedUnits().Remove(newDemon);
    }


    private static void UpdateSortedListAfterSummon(Player player, Unit newDemon, Unit removedDemon)
    {
        if (removedDemon != null)
        {
            player.ReplaceFromSortedListWhenInvoked(removedDemon, newDemon);
        }
        else
        {
            player.AddDemonInTheLastSlot((Demon)newDemon);
        }
    }

    private static int FindSlotOfActiveDemon(Player player, Unit demon)
    {
        var activeUnits = player.GetActiveUnits();
        for (int i = 0; i < activeUnits.Count; i++)
        {
            if (activeUnits[i] == demon)
                return i;
        }

        return -1;
    }
}