using System.Diagnostics.Contracts;

namespace Shin_Megami_Tensei;

public class PlayerCombatState
{
    private readonly Player _player;
    private bool _ableToContinue;
    private int _hitCounter = 0;

    public PlayerCombatState(Player player)
    {
        _player = player;
        _ableToContinue = true;
    }

    public bool IsTeamAbleToContinue() => _ableToContinue;

    public void SetTeamUnableToContinue()
    {
        _ableToContinue = false;
    }
    
    public bool IsPlayerCancelling(string input, int optionsCount)
    {
        return input == $"{optionsCount + 1}";
    }

    public void UpdateTeamContinuationStatus()
    {
        if (_player.HasSurrendered())
        {
            _ableToContinue = false;
            return;
        }

        _ableToContinue = _player.UnitManager.HasAnyHealthyUnit();
    }

    public void CheckIfTeamIsAbleToContinue()
    {
        if (_player.HasSurrendered())
        {
            _ableToContinue = false;
            return;
        }

        var activeUnits = _player.UnitManager.GetActiveUnits();
        foreach (Unit unit in activeUnits)
        {
            if (unit != null && unit.GetCurrentStats().GetStatByName("HP") > 0)
            {
                _ableToContinue = true;
                return;
            }
        }

        _ableToContinue = false;
    }

    
    public int GetHitCounter() => _hitCounter;

    public void IncreaseHitCounter()
    {
        _hitCounter++;
    }
    
    public string GetPlayerInputWithSeparator()
    {
        return CombatUI.GetUserInput();
    }

    public List<int> GetValidSlotsFromActiveUnitsAndDisplayIt()
    {
        List<Unit> activeUnits = _player.UnitManager.GetActiveUnits();
        List<int> validSlots = new();

        PrepareAndDisplaySlots(activeUnits, validSlots);
        CombatUI.DisplayCancelOption(validSlots.Count);

        return validSlots;
    }

    private void PrepareAndDisplaySlots(List<Unit> activeUnits, List<int> validSlots)
    {
        for (int iterator = 1; iterator < activeUnits.Count; iterator++)
        {
            if (IsSlotEmptyOrUnitDead(activeUnits[iterator]))
            {
                CombatUI.DisplayEmptySlot(validSlots, iterator);
            }
            else
            {
                CombatUI.DisplayDemonInSlot(validSlots, iterator, activeUnits[iterator]);
            }

            validSlots.Add(iterator);
        }
    }

    private bool IsSlotEmptyOrUnitDead(Unit unit)
    {
        return unit == null || unit.GetCurrentStats().GetStatByName("HP") <= 0;
    }
}