using Shin_Megami_Tensei_View;
using Shin_Megami_Tensei.Gadgets;

namespace Shin_Megami_Tensei.Managers;

public class SpecialSkillManager
{
    public static void UseSabbatma(Player casterPlayer, Skill skill, View view)
    {
        var aliveReserve = casterPlayer.GetReservedUnits()
            .Where(u => u != null && u.GetCurrentStats().GetStatByName("HP") > 0)
            .ToList();

        if (aliveReserve.Count == 0)
        {
            view.WriteLine("No hay unidades vivas en la reserva para invocar.");
            return;
        }

        view.WriteLine("Seleccione una unidad viva de la reserva para invocar:");
        for (int i = 0; i < aliveReserve.Count; i++)
        {
            var unit = aliveReserve[i];
            view.WriteLine($"{i + 1}-{unit.GetName()} HP:{unit.GetCurrentStats().GetStatByName("HP")}/{unit.GetBaseStats().GetStatByName("HP")} MP:{unit.GetCurrentStats().GetStatByName("MP")}/{unit.GetBaseStats().GetStatByName("MP")}");
        }
        view.WriteLine($"{aliveReserve.Count + 1}-Cancelar");

        int selectedIndex = GetValidatedInputFromView(view, 1, aliveReserve.Count + 1) - 1;
        if (selectedIndex == aliveReserve.Count)
            return; // Cancelar

        Unit selectedUnit = aliveReserve[selectedIndex];

        var validSlots = casterPlayer.GetValidSlotsFromActiveUnits(view);
        if (validSlots.Count == 0)
        {
            view.WriteLine("No hay espacios disponibles en el tablero para invocar.");
            return;
        }

        view.WriteLine("Seleccione el puesto donde invocar a la unidad:");
        for (int i = 0; i < validSlots.Count; i++)
        {
            int slotIndex = validSlots[i];
            view.WriteLine($"{i + 1}-Vacío (Puesto {slotIndex + 1})");
        }
        view.WriteLine($"{validSlots.Count + 1}-Cancelar");

        int slotChoiceIndex = GetValidatedInputFromView(view, 1, validSlots.Count + 1) - 1;
        if (slotChoiceIndex == validSlots.Count)
            return;

        int finalSlot = validSlots[slotChoiceIndex];

        casterPlayer.GetActiveUnits()[finalSlot] = selectedUnit;
        casterPlayer.GetReservedUnits().Remove(selectedUnit);
        casterPlayer.AddDemonInTheLastSlot((Demon)selectedUnit);
        casterPlayer.ReorderReserveBasedOnJsonOrder();

        view.WriteLine($"{selectedUnit.GetName()} ha sido invocado");
    }

    private static int GetValidatedInputFromView(View view, int min, int max)
    {
        while (true)
        {
            string input = view.ReadLine()?.Trim();
            if (int.TryParse(input, out int option) && option >= min && option <= max)
                return option;

            view.WriteLine("Por favor, ingrese una opción válida.");
        }
    }

}