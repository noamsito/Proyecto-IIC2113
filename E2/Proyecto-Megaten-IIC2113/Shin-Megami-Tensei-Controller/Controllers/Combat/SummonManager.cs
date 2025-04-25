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
        DisplaySummonPrompt(view);

        List<Unit> reserve = player.GetReservedUnits();
        DisplayReserveOptions(reserve, view);

        string demonInput = GetUserInput(view);
        if (IsCancelOption(demonInput, reserve.Count))
        {
            DisplaySeparator(view);
            return false;
        }

        Unit selectedDemon = SelectDemonFromReserve(reserve, demonInput);

        DisplaySlotSelectionPrompt(view);

        List<int> validSlots = GetValidSlots(player, view);
        string slotInput = GetUserInput(view);
        if (IsCancelOption(slotInput, validSlots.Count))
            return false;

        int slot = SelectSlot(validSlots, slotInput);

        SummonDemon(player, selectedDemon, slot);
        return true;
    }

    public static void MonsterSwap(Player player, Demon demonSummoned, View view)
    {
        DisplaySummonPrompt(view);

        List<Unit> reserve = player.GetReservedUnits();
        DisplayReserveOptions(reserve, view);

        string input = GetUserInput(view);
        if (IsCancelOption(input, reserve.Count))
            return;

        Demon selectedDemon = (Demon)SelectDemonFromReserve(reserve, input);

        int slotToReplace = FindSlotOfActiveDemon(player, demonSummoned);

        SummonDemon(player, selectedDemon, slotToReplace);
    }

    private static void DisplaySummonPrompt(View view)
    {
        view.WriteLine("Seleccione un monstruo para invocar");
    }

    private static void DisplayReserveOptions(List<Unit> reserve, View view)
    {
        CombatUI.DisplaySummonOptions(reserve, view);
    }

    private static string GetUserInput(View view)
    {
        return view.ReadLine();
    }

    private static void DisplaySeparator(View view)
    {
        view.WriteLine(GameConstants.Separator);
    }

    private static void DisplaySlotSelectionPrompt(View view)
    {
        DisplaySeparator(view);
        view.WriteLine("Seleccione una posición para invocar");
    }

    private static List<int> GetValidSlots(Player player, View view)
    {
        return player.GetValidSlotsFromActiveUnits(view);
    }

    private static bool IsCancelOption(string input, int count)
    {
        return input == $"{count + 1}";
    }

    private static Unit SelectDemonFromReserve(List<Unit> reserve, string input)
    {
        int demonIndex = ParseInputToIndex(input);
        return reserve[demonIndex];
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
        DisplaySummonSuccess(newDemonAddedToActiveList);
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
        player.ReplaceFromReserveUnitsList(newDemon.GetName(), (Demon)removedDemon);
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

    private static void DisplaySummonSuccess(Unit summonedDemon)
    {
        CombatUI.DisplayHasBeenSummoned(summonedDemon);
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