using Shin_Megami_Tensei_View;
using Shin_Megami_Tensei.Combat;
using Shin_Megami_Tensei.Gadgets;

namespace Shin_Megami_Tensei.Managers;

public static class SummonManager
{
    public static void ManageTurnsWhenSummoned(TurnContext turnCtx)
    {
        TurnManager.ConsumeTurnsWhenPassedTurn(turnCtx);
        TurnManager.UpdateTurnStates(turnCtx);
    }
    
    public static bool SummonFromReserveBySamurai(Player player, View view)
    {
        view.WriteLine("Seleccione un monstruo para invocar");
    
        List<Unit> reserve = player.GetReservedUnits();
        CombatUI.DisplaySummonOptions(reserve, view);
    
        string demonInput = view.ReadLine();
        if (IsCancelOption(demonInput, reserve.Count))
        {
            view.WriteLine(GameConstants.Separator);
            return false;
        }
    
        Unit selectedDemon = SelectDemonFromReserve(reserve, demonInput);
    
        view.WriteLine(GameConstants.Separator);
        view.WriteLine("Seleccione una posición para invocar");
    
        var validSlots = player.GetValidSlotsFromActiveUnits(view);
        string slotInput = view.ReadLine();
        if (IsCancelOption(slotInput, validSlots.Count)) return false;
    
        int slot = SelectSlot(validSlots, slotInput);
    
        SummonDemon(player, selectedDemon, slot);
        return true;
    }
    
    private static bool IsCancelOption(string input, int count)
    {
        return input == $"{count + 1}";
    }
    
    private static Unit SelectDemonFromReserve(List<Unit> reserve, string input)
    {
        int demonIndex = Convert.ToInt32(input) - 1;
        return reserve[demonIndex];
    }
    
    private static int SelectSlot(List<int> validSlots, string input)
    {
        return validSlots[Convert.ToInt32(input) - 1];
    }
    
    private static void SummonDemon(Player player, Unit newDemonAddedToActiveList, int slot)
    {
        Unit removedDemonFromActiveList = player.GetActiveUnits()[slot];
        
        player.GetActiveUnits()[slot] = newDemonAddedToActiveList;
        
        player.ReplaceFromReserveUnitsList(newDemonAddedToActiveList.GetName(), (Demon)removedDemonFromActiveList);
        player.ReorderReserveBasedOnJsonOrder();
        player.GetReservedUnits().Remove(newDemonAddedToActiveList);

        if (removedDemonFromActiveList != null)
        {
            player.ReplaceFromSortedListWhenInvoked(removedDemonFromActiveList, newDemonAddedToActiveList);
        }
        else
        {
            player.AddDemonInTheLastSlot((Demon)newDemonAddedToActiveList);
        }
        
        player.ReorderUnitsWhenAttacked();
        CombatUI.DisplayHasBeenSummoned(newDemonAddedToActiveList);
    }

    public static void MonsterSwap(Player player, Demon demonSummoned, View view)
    {
        view.WriteLine("Seleccione un monstruo para invocar");

        List<Unit> reserve = player.GetReservedUnits();
        CombatUI.DisplaySummonOptions(reserve, view);

        string input = view.ReadLine();
        if (IsCancelOption(input, reserve.Count)) return;

        Demon selectedDemon = (Demon)SelectDemonFromReserve(reserve, input);

        int slotToReplace = FindSlotOfActiveDemon(player, demonSummoned);
        
        SummonDemon(player, selectedDemon, slotToReplace);
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
