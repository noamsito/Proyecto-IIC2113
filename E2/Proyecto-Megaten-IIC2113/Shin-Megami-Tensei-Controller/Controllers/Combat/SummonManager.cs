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

    public static bool SummonFromReserveBySamurai(Player player)
    {
        CombatUI.DisplaySummonPrompt();

        List<Unit> reserve = player.GetReservedUnits();
        var aliveReserve = reserve.Where(unit => unit.GetCurrentStats().GetStatByName("HP") > 0).ToList();

        CombatUI.DisplaySummonOptions(aliveReserve);

        string demonInput = CombatUI.GetUserInput();
        CombatUI.DisplaySeparator();

        if (IsCancelOption(demonInput, aliveReserve.Count))
            return false;

        Unit selectedDemon = SelectDemonFromReserveIncludingDead(reserve, demonInput);

        CombatUI.DisplaySlotSelectionPrompt();

        List<int> validSlots = player.GetValidSlotsFromActiveUnitsAndDisplayIt();
        string slotInput = CombatUI.GetUserInput();

        if (IsCancelOption(slotInput, validSlots.Count))
            return false;

        int slot = SelectSlot(validSlots, slotInput);

        SummonDemon(player, selectedDemon, slot);
        return true;
    }

    public static bool SummonBySkillInvitation(Player player)
    {
        CombatUI.DisplaySummonPrompt();
        
        var reserve = player.GetReservedUnits();
        
        CombatUI.DisplaySummonOptionsIncludingDead(reserve);

        string demonInput = CombatUI.GetUserInput();
        CombatUI.DisplaySeparator();

        if (IsCancelOption(demonInput, reserve.Count))
            return false;

        Unit selectedDemon = SelectDemonFromReserveIncludingDead(reserve, demonInput);

        CombatUI.DisplaySlotSelectionPrompt();

        List<int> validSlots = player.GetValidSlotsFromActiveUnitsAndDisplayIt();
        string slotInput = CombatUI.GetUserInput();

        if (IsCancelOption(slotInput, validSlots.Count))
            return false;

        int slot = SelectSlot(validSlots, slotInput);

        // falta el mensaje de que la unidad fue revivida
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

        Demon selectedDemon = (Demon)SelectDemonFromReserveIncludingDead(reserve, input);

        int slotToReplace = FindSlotOfActiveDemon(player, demonSummoned);

        SummonDemon(player, selectedDemon, slotToReplace);
    }

    private static bool IsCancelOption(string input, int count)
    {
        return input == $"{count + 1}";
    }

    public static Unit SelectDemonFromRerseveOnlyAlive(List<Unit> fullReserve, string input)
    {
        var aliveReserve = fullReserve
            .Where(unit => unit != null && unit.GetCurrentStats().GetStatByName("HP") > 0)
            .ToList();

        int index = ParseInputToIndex(input);
        return aliveReserve[index];
    }
    public static Unit SelectDemonFromReserveIncludingDead(List<Unit> fullReserve, string input)
    {
        var aliveReserve = fullReserve.ToList();

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

    public static void SummonDemon(Player player, Unit newDemonAddedToActiveList, int slot)
    {
        Unit removedDemonFromActiveList = GetDemonToReplace(player, slot);

        ReplaceActiveSlot(player, newDemonAddedToActiveList, slot);
        UpdateReserveAfterSummon(player, newDemonAddedToActiveList, removedDemonFromActiveList);
        UpdateSortedListAfterSummon(player, newDemonAddedToActiveList, removedDemonFromActiveList);
        player.RearrangeSortedUnitsWhenAttacked();
        
        CombatUI.DisplayHasBeenSummoned(newDemonAddedToActiveList);
    }

    public static Unit GetDemonToReplace(Player player, int slot)
    {
        return player.GetActiveUnits()[slot];
    }

    public static void ReplaceActiveSlot(Player player, Unit newDemon, int slot)
    {
        player.GetActiveUnits()[slot] = newDemon;
    }


    public static void UpdateReserveAfterSummon(Player player, Unit newDemon, Unit removedDemon)
    {
        if (removedDemon != null)
        {
            player.ReplaceFromReserveUnitsList(newDemon.GetName(), (Demon)removedDemon);
        }

        player.ReorderReserveBasedOnJsonOrder();
        player.GetReservedUnits().Remove(newDemon);
    }


    public static void UpdateSortedListAfterSummon(Player player, Unit newDemon, Unit removedDemon)
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

    public static int FindSlotOfActiveDemon(Player player, Unit demon)
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