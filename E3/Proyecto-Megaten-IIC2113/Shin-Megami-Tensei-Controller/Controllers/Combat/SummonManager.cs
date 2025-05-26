using System.Diagnostics.Contracts;
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
        PlayerUnitManager playerUnitManager = player.UnitManager;
        PlayerCombatState combatManagerPlayer = player.CombatState;
        
        CombatUI.DisplaySummonPrompt();

        List<Unit> reserve = playerUnitManager.GetReservedUnits();
        var aliveReserve = reserve.Where(unit => unit.GetCurrentStats().GetStatByName("HP") > 0).ToList();

        CombatUI.DisplaySummonOptions(aliveReserve);

        string demonInput = CombatUI.GetUserInput();
        CombatUI.DisplaySeparator();

        if (IsCancelOption(demonInput, aliveReserve.Count))
        {
            return false;
        }

        Unit selectedDemon = SelectDemonFromRerseveOnlyAlive(reserve, demonInput);

        CombatUI.DisplaySlotSelectionPrompt();

        List<int> validSlots = combatManagerPlayer.GetValidSlotsFromActiveUnitsAndDisplayIt();
        string slotInput = CombatUI.GetUserInput();

        if (IsCancelOption(slotInput, validSlots.Count))
        {
            CombatUI.DisplaySeparator();
            return false;
        }

        int slot = SelectSlot(validSlots, slotInput);

        CombatUI.DisplaySeparator();
        SummonDemon(player, selectedDemon, slot);
        playerUnitManager.RearrangeSortedUnitsWhenAttacked();
        CombatUI.DisplaySeparator();
        return true;
    }

    public static bool MonsterSwap(Player player, Demon demonSummoned)
    {
        PlayerUnitManager playerUnitManager = player.UnitManager;
        
        CombatUI.DisplaySummonPrompt();

        List<Unit> reserve = playerUnitManager.GetReservedUnits();
        var aliveReserve = reserve.Where(unit => unit.GetCurrentStats().GetStatByName("HP") > 0).ToList();

        CombatUI.DisplaySummonOptions(aliveReserve);

        string input = CombatUI.GetUserInput();
        if (IsCancelOption(input, aliveReserve.Count))
        {
            CombatUI.DisplaySeparator();
            return false;
        }

        Demon selectedDemon = (Demon)SelectDemonFromReserveAlives(aliveReserve, input);

        int slotToReplace = FindSlotOfActiveDemon(player, demonSummoned);
        
        CombatUI.DisplaySeparator();
        SummonDemon(player, selectedDemon, slotToReplace);
        playerUnitManager.RearrangeSortedUnitsWhenAttacked();
        CombatUI.DisplaySeparator();

        return true;
    }

    public static bool SummonBySkillInvitation(SkillUseContext skillCtx, TurnContext turnCtx)
    {
        Player skillCtxAttacker = skillCtx.Attacker;

        if (!TrySelectDemonForInvitation(skillCtxAttacker, out Unit selectedDemon))
        {
            return false;
        }

        skillCtx.Target = selectedDemon;

        if (!TryGetSlotForSummon(skillCtxAttacker, out int slot))
        {
            CombatUI.DisplaySeparator();
            return false;
        }

        CombatUI.DisplaySeparator();
        SummonDemon(skillCtxAttacker, selectedDemon, slot);

        HandleResurrectionIfNeeded(skillCtx);

        TurnManager.UpdateTurnsForInvocationSkill(turnCtx);

        return true;
    }

    private static bool TrySelectDemonForInvitation(Player player, out Unit selectedDemon)
    {
        DisplaySummonInterface(player, out var reserve, out string demonInput);

        if (IsCancelOption(demonInput, reserve.Count))
        {
            selectedDemon = null;
            return false;
        }

        selectedDemon = SelectDemonFromReserveAlives(reserve, demonInput);
        return true;
    }

    private static void HandleResurrectionIfNeeded(SkillUseContext skillCtx)
    {
        bool resurrected = ResurrectDemonIfNeeded(skillCtx.Target);

        if (resurrected)
        {
            DisplayResurrectionEffects(skillCtx);
        }
        else
        {
            CombatUI.DisplaySeparator();
        }
    }

    private static void DisplayResurrectionEffects(SkillUseContext skillCtx)
    {
        CombatUI.DisplaySkillUsage(skillCtx.Caster, skillCtx.Skill, skillCtx.Target);
        double amountHealed = HealSkillsManager.CalculateHeal(skillCtx.Target, skillCtx);
        CombatUI.DisplayHealingForSingleTarget(skillCtx.Target, amountHealed);
        CombatUI.DisplaySeparator();
    }

    private static void DisplaySummonInterface(Player player, out List<Unit> reserve, out string demonInput)
    {
        PlayerUnitManager teamManagerPlayer = player.UnitManager;
        CombatUI.DisplaySummonPrompt();

        reserve = teamManagerPlayer.GetReservedUnits();

        CombatUI.DisplaySummonOptionsIncludingDead(reserve);

        demonInput = CombatUI.GetUserInput();
        CombatUI.DisplaySeparator();
    }

    private static bool TryGetSlotForSummon(Player player, out int slot)
    {
        PlayerCombatState combatManagerPlayer = player.CombatState;
        
        CombatUI.DisplaySlotSelectionPrompt();

        List<int> validSlots = combatManagerPlayer.GetValidSlotsFromActiveUnitsAndDisplayIt();
        string slotInput = CombatUI.GetUserInput();

        bool isCanceled = IsCancelOption(slotInput, validSlots.Count);

        slot = isCanceled ? 0 : SelectSlot(validSlots, slotInput);
        return !isCanceled;
    }

    private static bool ResurrectDemonIfNeeded(Unit selectedDemon)
    {
        int currentHP = selectedDemon.GetCurrentStats().GetStatByName("HP");
        if (currentHP <= 0)
        {
            int baseHP = selectedDemon.GetBaseStats().GetStatByName("HP");
            selectedDemon.GetCurrentStats().SetStatByName("HP", baseHP);
            return true;
        }

        return false;
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
    public static Unit SelectDemonFromReserveAlives(List<Unit> fullReserve, string input)
    {
        var aliveReserve = fullReserve.ToList();

        int index = ParseInputToIndex(input);
        Console.WriteLine(aliveReserve[index].GetName());
        
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
        
        CombatUI.DisplayHasBeenSummoned(newDemonAddedToActiveList);
    }

    public static Unit GetDemonToReplace(Player player, int slot)
    {
        PlayerUnitManager playerUnitManager = player.UnitManager;
        return playerUnitManager.GetActiveUnits()[slot];
    }

    public static void ReplaceActiveSlot(Player player, Unit newDemon, int slot)
    {
        PlayerUnitManager playerUnitManager = player.UnitManager;
        playerUnitManager.GetActiveUnits()[slot] = newDemon;
    }


    public static void UpdateReserveAfterSummon(Player player, Unit newDemon, Unit removedDemon)
    {
        PlayerUnitManager playerUnitManager = player.UnitManager;
        PlayerTeamManager playerTeamManager = player.TeamManager;
        
        if (removedDemon != null)
        {
            playerUnitManager.ReplaceFromReserveUnitsList(newDemon.GetName(), (Demon)removedDemon);
        }

        playerTeamManager.ReorderReserveBasedOnJsonOrder();
        playerUnitManager.GetReservedUnits().Remove(newDemon);
    }


    public static void UpdateSortedListAfterSummon(Player player, Unit newDemon, Unit removedDemon)
    {
        PlayerUnitManager playerUnitManager = player.UnitManager;
        if (removedDemon != null)
        {
            playerUnitManager.ReplaceFromSortedListWhenInvoked(removedDemon, newDemon);
        }
        else
        {
            playerUnitManager.AddDemonInTheLastSlot((Demon)newDemon);
        }
    }

    public static int FindSlotOfActiveDemon(Player player, Unit demon)
    {
        PlayerUnitManager playerUnitManager = player.UnitManager;

        var activeUnits = playerUnitManager.GetActiveUnits();
        for (int i = 0; i < activeUnits.Count; i++)
        {
            if (activeUnits[i] == demon)
                return i;
        }

        return -1;
    }
}