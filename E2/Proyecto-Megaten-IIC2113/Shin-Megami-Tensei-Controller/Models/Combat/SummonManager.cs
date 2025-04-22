using Shin_Megami_Tensei_View;
using Shin_Megami_Tensei.Combat;
using Shin_Megami_Tensei.Gadgets;

namespace Shin_Megami_Tensei.Managers;

public static class SummonManager
{
    public static void ManageInvokeAndTurns(Player player, TurnContext turnCtx, View view)
    {
        InvokeFromReserveBySamurai(player, view);
        
        TurnManager.UpdateTurnsStandard(turnCtx);
        TurnManager.UpdateTurnStates(turnCtx);
    }
    
    public static void InvokeFromReserveBySamurai(Player player, View view)
    {
        view.WriteLine("Seleccione un monstruo para invocar");
    
        var reserve = GetAvailableDemons(player);
        CombatUI.DisplayInvokeOptions(reserve, view);
    
        string demonInput = view.ReadLine();
        if (IsCancelOption(demonInput, reserve.Count)) return;
    
        Demon selectedDemon = SelectDemonFromReserve(reserve, demonInput);
    
        view.WriteLine(GameConstants.Separator);
        view.WriteLine("Seleccione una posición para invocar");
    
        var validSlots = GetValidSlots(player, view);
        string slotInput = view.ReadLine();
        if (IsCancelOption(slotInput, validSlots.Count)) return;
    
        int slot = SelectSlot(validSlots, slotInput);
    
        SummonDemon(player, selectedDemon, slot, view);
    }
    
    private static List<Demon> GetAvailableDemons(Player player)
    {
        return player.GetReservedUnits()
            .Where(unit => unit != null && unit.IsAlive())
            .Cast<Demon>()
            .ToList();
    }
    
    private static bool IsCancelOption(string input, int count)
    {
        return input == $"{count + 1}";
    }
    
    private static Demon SelectDemonFromReserve(List<Demon> reserve, string input)
    {
        int demonIndex = Convert.ToInt32(input) - 1;
        return reserve[demonIndex];
    }
    
    private static List<int> GetValidSlots(Player player, View view)
    {
        var activeUnits = player.GetActiveUnits();
        List<int> validSlots = new();
        
        for (int i = 1; i < activeUnits.Count; i++)
        {
            if (activeUnits[i] == null)
            {
                view.WriteLine($"{validSlots.Count + 1}-Vacío (Puesto {i + 1})");
            }
            else
            {
                Stat currentStats = activeUnits[i].GetCurrentStats();
                Stat baseStats = activeUnits[i].GetBaseStats();
        
                string slotStatus = $"{activeUnits[i].GetName()} " +
                                    $"HP:{currentStats.GetStatByName("HP")}/{baseStats.GetStatByName("HP")} " +
                                    $"MP:{currentStats.GetStatByName("MP")}/{baseStats.GetStatByName("MP")} (Puesto {i + 1})";
        
                view.WriteLine($"{validSlots.Count + 1}-{slotStatus}");
            }
            validSlots.Add(i);
        }
        view.WriteLine($"{validSlots.Count + 1}-Cancelar");
        return validSlots;
    }
    
    private static int SelectSlot(List<int> validSlots, string input)
    {
        return validSlots[Convert.ToInt32(input) - 1];
    }
    
    private static void SummonDemon(Player player, Demon newDemonAddedToActiveList, int slot, View view)
    {
        Unit removedDemonFromActiveList = player.GetActiveUnits()[slot];
        player.ReorderUnitsWhenAttacked();
        
        player.GetReservedUnits().Remove(newDemonAddedToActiveList);
        player.GetReservedUnits().Add((Demon)removedDemonFromActiveList);
        
        player.GetActiveUnits()[slot] = newDemonAddedToActiveList;
        player.ReplaceFromSortedListWhenInvoked(removedDemonFromActiveList.GetName(), newDemonAddedToActiveList);

        player.ReplaceFromReserveUnitsList(newDemonAddedToActiveList.GetName(), (Demon)removedDemonFromActiveList);
    
        view.WriteLine(GameConstants.Separator);
        view.WriteLine($"{newDemonAddedToActiveList.GetName()} ha sido invocado");
        view.WriteLine(GameConstants.Separator);
    }

    public static void MonsterSwap(Player player, Unit summoner, View view)
    {
        view.WriteLine("Seleccione un monstruo para invocar");

        var reserve = player.GetReservedUnits()
            .Where(unit => unit != null && unit.IsAlive())
            .Cast<Demon>()
            .ToList();

        if (reserve.Count == 0)
        {
            view.WriteLine("No hay demonios disponibles para invocar");
            return;
        }

        for (int i = 0; i < reserve.Count; i++)
        {
            var demon = reserve[i];
            view.WriteLine($"{i + 1}-{demon.GetName()} HP:{demon.GetCurrentStats().GetStatByName("HP")}/{demon.GetBaseStats().GetStatByName("HP")} " +
                           $"MP:{demon.GetCurrentStats().GetStatByName("MP")}/{demon.GetBaseStats().GetStatByName("MP")}");
        }
        view.WriteLine($"{reserve.Count + 1}-Cancelar");

        string input = view.ReadLine();
        if (input == $"{reserve.Count + 1}") return;

        int demonIndex = Convert.ToInt32(input) - 1;
        Demon selectedDemon = reserve[demonIndex];

        var activeUnits = player.GetActiveUnits();
        for (int i = 0; i < activeUnits.Count; i++)
        {
            if (activeUnits[i] == summoner)
            {
                activeUnits[i] = selectedDemon;
                player.GetReservedUnits().Remove(selectedDemon);
                player.GetReservedUnits().Add((Demon)summoner); ;
            }
        }

        view.WriteLine(GameConstants.Separator);
        view.WriteLine($"{selectedDemon.GetName()} ha sido invocado");
        view.WriteLine(GameConstants.Separator);
    }
}
