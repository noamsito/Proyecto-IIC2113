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
        List<int> validSlots = player.GetValidSlotsFromActiveUnitsAndDisplayIt();
        if (validSlots.Count == 0)
        {
            return;
        }
        
        int slotChoice = int.Parse(CombatUI.GetUserInput());
        if (slotChoice == validSlots.Count) return;

        if (skillCtx.Caster is Samurai)
        {
            SummonManager.SummonFromReserveBySamurai(player);
        }
        else
        {
            SummonManager.MonsterSwap(player, (Demon)skillCtx.Caster);
        }
        
        //
        // int finalSlot = validSlots[slotChoice] - 1;
        // Unit removedDemonFromActiveList = SummonManager.GetDemonToReplace(player, finalSlot);

        // int casterSlot = SummonManager.FindSlotOfActiveDemon(player, skillCtx.Caster);

        // player.GetActiveUnits()[finalSlot] = selectedUnit;
        // player.GetReservedUnits().Remove(selectedUnit);
        // player.SetOrderOfAttackOfActiveUnits();
        // player.ReorderReserveBasedOnJsonOrder();
        // player.ReorderUnitsWhenAttacked();
        
        
        
        CombatUI.DisplayHasBeenSummoned(selectedUnit);
    }
}
