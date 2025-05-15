using Shin_Megami_Tensei_View;
using Shin_Megami_Tensei.Combat;
using Shin_Megami_Tensei.Gadgets;

namespace Shin_Megami_Tensei.Managers;

public static class SpecialSkillManager
{
    public static void UseSpecialSkill(SkillUseContext ctx)
    {
        switch (ctx.Skill.Name)
        {
            case "Sabbatma":
                UseSabbatma(ctx);
                break;

            default:
                CombatUI.DisplaySeparator();
                CombatUI.GetUserInput(); 
                break;
        }
    }

    private static void UseSabbatma(SkillUseContext ctx)
    {
        var player = ctx.Attacker;
        var aliveReserve = player.GetReservedUnits()
            .Where(u => u != null && u.GetCurrentStats().GetStatByName("HP") > 0)
            .ToList();

        if (aliveReserve.Count == 0)
        {
            CombatUI.DisplaySeparator();
            CombatUI.DisplaySkillSelectionPrompt("No hay unidades vivas en la reserva para invocar.");
            return;
        }

        // Mostrar opciones de demonios vivos para invocar
        CombatUI.DisplaySkillSelectionPrompt("Seleccione una unidad viva de la reserva:");
        for (int i = 0; i < aliveReserve.Count; i++)
        {
            var u = aliveReserve[i];
            CombatUI.DisplayTargetOptions(new List<Unit> { u }); // reusar método para mostrar stats
        }
        CombatUI.DisplayCancelOption(aliveReserve.Count);

        int index = int.Parse(CombatUI.GetUserInput()) - 1;
        if (index == aliveReserve.Count) return;

        Unit selectedUnit = aliveReserve[index];

        // Mostrar opciones de slots vacíos
        var validSlots = player.GetValidSlotsFromActiveUnits();
        if (validSlots.Count == 0)
        {
            CombatUI.DisplaySkillSelectionPrompt("No hay espacios disponibles en el tablero.");
            return;
        }

        CombatUI.DisplaySkillSelectionPrompt("Seleccione el puesto donde invocar a la unidad:");
        for (int i = 0; i < validSlots.Count; i++)
        {
            int slotIndex = validSlots[i];
            CombatUI.DisplayEmptySlot(validSlots, slotIndex);
        }
        CombatUI.DisplayCancelOption(validSlots.Count);

        int slotChoice = int.Parse(CombatUI.GetUserInput()) - 1;
        if (slotChoice == validSlots.Count) return;

        int finalSlot = validSlots[slotChoice];

        // Ejecutar invocación
        player.GetActiveUnits()[finalSlot] = selectedUnit;
        player.GetReservedUnits().Remove(selectedUnit);
        player.AddDemonInTheLastSlot((Demon)selectedUnit);
        player.ReorderReserveBasedOnJsonOrder();

        CombatUI.DisplayHasBeenSummoned(selectedUnit);
    }
}
