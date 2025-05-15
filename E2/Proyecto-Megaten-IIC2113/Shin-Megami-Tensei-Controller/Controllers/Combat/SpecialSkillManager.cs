using Shin_Megami_Tensei_View;
using Shin_Megami_Tensei.Combat;
using Shin_Megami_Tensei.Gadgets;

namespace Shin_Megami_Tensei.Managers;

public static class SpecialSkillManager
{
    public static void UseSpecialSkill(SkillUseContext skillCtx)
    {
        switch (skillCtx.Skill.Name)
        {
            case "Sabbatma":
                UseSabbatma(skillCtx);
                break;

            default:
                break;
        }
    }

    private static void UseSabbatma(SkillUseContext skillCtx)
    {
        var player = skillCtx.Attacker;
        var reservedUnits = player.GetReservedUnits()
            .Where(u => u != null && u.GetCurrentStats().GetStatByName("HP") > 0)
            .ToList();

        if (reservedUnits.Count == 0)
        {
            CombatUI.DisplaySeparator();
            CombatUI.DisplaySkillSelectionPrompt("No hay unidades vivas en la reserva para invocar.");
            return;
        }
        
        CombatUI.DisplaySummonPrompt();
        for (int i = 0; i < reservedUnits.Count; i++)
        {
            var u = reservedUnits[i];
            CombatUI.DisplayDemonsStats(new List<Unit> { u }); 
        }
        CombatUI.DisplayCancelOption(reservedUnits.Count); 
        
        int index = int.Parse(CombatUI.GetUserInput()) - 1;
        CombatUI.DisplaySeparator();
        
        if (index == reservedUnits.Count) return;
        Unit selectedUnit = reservedUnits[index];
        
        CombatUI.DisplaySlotSelectionPrompt();
        var validSlots = player.GetValidSlotsFromActiveUnitsAndDisplayIt();
        if (validSlots.Count == 0)
        {
            return;
        }
        
        int slotChoice = int.Parse(CombatUI.GetUserInput()) - 1;
        if (slotChoice == validSlots.Count) return;

        int finalSlot = validSlots[slotChoice];

        // Ejecutar invocación
        player.GetActiveUnits()[finalSlot] = selectedUnit;

        // Quitar el demonio invocado de la reserva
        player.GetReservedUnits().Remove(selectedUnit);

        // Añadirlo al final del orden de ataque
        player.AddDemonInTheLastSlot((Demon)selectedUnit);

        // Reordenar la reserva según orden original del JSON
        player.ReorderReserveBasedOnJsonOrder();

        // Reordenar lista de ataque si aplica
        player.ReorderUnitsWhenAttacked();

        
        CombatUI.DisplayHasBeenSummoned(selectedUnit);
    }
}
