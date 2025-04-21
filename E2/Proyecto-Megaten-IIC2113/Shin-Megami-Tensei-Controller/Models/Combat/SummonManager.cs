using Shin_Megami_Tensei_View;

namespace Shin_Megami_Tensei.Managers;

public static class SummonManager
{
    public static void InvokeFromReserve(Samurai samurai, Player player, View view)
    {
        view.WriteLine(GameConstants.Separator);
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
            view.WriteLine($"{i + 1}-{demon.GetName()} HP:{demon.GetCurrentStats().GetStatByName("HP")}/{demon.GetBaseStats().GetStatByName("HP")} MP:{demon.GetCurrentStats().GetStatByName("MP")}/{demon.GetBaseStats().GetStatByName("MP")}");
        }
        view.WriteLine($"{reserve.Count + 1}-Cancelar");

        string demonInput = view.ReadLine();
        if (demonInput == $"{reserve.Count + 1}") return;

        int demonIndex = Convert.ToInt32(demonInput) - 1;
        Demon selectedDemon = reserve[demonIndex];

        view.WriteLine(GameConstants.Separator);
        view.WriteLine("Seleccione una posición para invocar");

        var activeUnits = player.GetActiveUnits();
        List<int> validSlots = new();

        for (int i = 1; i < activeUnits.Count; i++)
        {
            string slotStatus = activeUnits[i] == null
                ? $"Vacío (Puesto {i + 1})"
                : $"{activeUnits[i].GetName()} HP:{activeUnits[i].GetCurrentStats().GetStatByName("HP")}/{activeUnits[i].GetBaseStats().GetStatByName("HP")} (Puesto {i + 1})";

            view.WriteLine($"{validSlots.Count + 1}-{slotStatus}");
            validSlots.Add(i);
        }

        view.WriteLine($"{validSlots.Count + 1}-Cancelar");
        string slotInput = view.ReadLine();
        if (slotInput == $"{validSlots.Count + 1}") return;

        int slot = validSlots[Convert.ToInt32(slotInput) - 1];

        player.GetActiveUnits()[slot] = selectedDemon;
        player.GetReservedUnits().Remove(selectedDemon);

        view.WriteLine(GameConstants.Separator);
        view.WriteLine($"{selectedDemon.GetName()} ha sido invocado");
        view.WriteLine(GameConstants.Separator);
    }

    public static void MonsterSwap(Player player, Unit summoner, View view)
    {
        view.WriteLine(GameConstants.Separator);
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
            view.WriteLine($"{i + 1}-{demon.GetName()} HP:{demon.GetCurrentStats().GetStatByName("HP")}/{demon.GetBaseStats().GetStatByName("HP")} MP:{demon.GetCurrentStats().GetStatByName("MP")}/{demon.GetBaseStats().GetStatByName("MP")}");
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
