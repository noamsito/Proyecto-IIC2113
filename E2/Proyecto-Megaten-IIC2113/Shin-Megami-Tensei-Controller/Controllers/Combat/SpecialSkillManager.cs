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
        //bien 
        
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
        
        // CombatUI.DisplaySummonOptions(reservedUnits);
        // for (int i = 0; i < validSlots.Count; i++)
        // {
        //     int slotIndex = validSlots[i];
        //     CombatUI.DisplayEmptySlot(validSlots, slotIndex);
        // }
        // CombatUI.DisplayCancelOption(validSlots.Count);

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
