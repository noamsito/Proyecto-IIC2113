namespace Shin_Megami_Tensei.Managers;

public class PlayerUnitManager
{
    private const string HP_STAT_NAME = "HP";
    private const string SPEED_STAT_NAME = "Spd";
    private const int MAX_ACTIVE_UNITS = 4;

    private readonly Player _player;
    private List<Unit> _activeUnits;
    private List<Unit> _reservedUnits;
    private List<Unit> _sortedActiveUnitsByOrderOfAttack;

    public PlayerUnitManager(Player player)
    {
        _player = player;
        InitializeUnitCollections();
    }

    private void InitializeUnitCollections()
    {
        _activeUnits = new List<Unit>(MAX_ACTIVE_UNITS);
        for (int i = 0; i < MAX_ACTIVE_UNITS; i++)
        {
            _activeUnits.Add(null);
        }

        _sortedActiveUnitsByOrderOfAttack = new List<Unit>(MAX_ACTIVE_UNITS);
        for (int i = 0; i < MAX_ACTIVE_UNITS; i++)
        {
            _sortedActiveUnitsByOrderOfAttack.Add(null);
        }

        _reservedUnits = new List<Unit>();
    }

    public List<Unit> GetActiveUnits() => _activeUnits;
    public List<Unit> GetReservedUnits() => _reservedUnits;
    public List<Unit> GetSortedActiveUnitsByOrderOfAttack() => _sortedActiveUnitsByOrderOfAttack;

    public void SetSamuraiInActiveSlot(Samurai samurai)
    {
        _activeUnits[0] = samurai;
    }

    public void SetDemonInActiveSlot(int slot, Demon demon)
    {
        if (slot >= 0 && slot < _activeUnits.Count)
        {
            _activeUnits[slot] = demon;
        }
    }

    public void ClearReservedUnits()
    {
        _reservedUnits.Clear();
    }

    public void AddToReservedUnits(Unit unit)
    {
        _reservedUnits.Add(unit);
    }

    public void SetReservedUnits(List<Unit> units)
    {
        _reservedUnits = units;
    }

    public int CountHealthyActiveUnits()
    {
        return _activeUnits.Count(unit => unit != null &&
                                          unit.GetCurrentStats().GetStatByName(HP_STAT_NAME) > 0);
    }

    public bool HasAnyHealthyUnit()
    {
        return _activeUnits.Any(unit => unit != null &&
                                        unit.GetCurrentStats().GetStatByName(HP_STAT_NAME) > 0);
    }

    public List<Unit> GetValidActiveUnits()
    {
        return _activeUnits.Where(unit => unit != null &&
                                          unit.GetCurrentStats().GetStatByName(HP_STAT_NAME) > 0).ToList();
    }

    public void SetOrderOfAttackOfActiveUnits()
    {
        _sortedActiveUnitsByOrderOfAttack = _activeUnits
            .Where(IsUnitActiveAndHealthy)
            .OrderByDescending(unit => unit.GetCurrentStats().GetStatByName(SPEED_STAT_NAME))
            .ToList();
    }

    private bool IsUnitActiveAndHealthy(Unit unit)
    {
        return unit != null &&
               !(unit is Samurai && unit.GetCurrentStats().GetStatByName(HP_STAT_NAME) <= 0) &&
               unit.GetCurrentStats().GetStatByName(HP_STAT_NAME) > 0;
    }

    public void RearrangeSortedUnitsWhenAttacked()
    {
        if (_sortedActiveUnitsByOrderOfAttack.Count > 0)
        {
            Unit firstUnit = _sortedActiveUnitsByOrderOfAttack[0];
            _sortedActiveUnitsByOrderOfAttack.RemoveAt(0);
            _sortedActiveUnitsByOrderOfAttack.Add(firstUnit);
        }
    }

    public void RemoveFromActiveUnitsIfDead()
    {
        for (int i = 0; i < _activeUnits.Count; i++)
        {
            if (IsUnitDeadOrInvalid(_activeUnits[i]))
            {
                ProcessDeadUnit(i);
            }
        }

        _player.CombatState.UpdateTeamContinuationStatus();
    }

    private bool IsUnitDeadOrInvalid(Unit unit)
    {
        return unit != null && unit.GetCurrentStats().GetStatByName(HP_STAT_NAME) <= 0;
    }

    private void ProcessDeadUnit(int unitIndex)
    {
        RemoveFromSortedUnits(_activeUnits[unitIndex].GetName());

        if (!(_activeUnits[unitIndex] is Samurai))
        {
            _reservedUnits.Add(_activeUnits[unitIndex]);
            _activeUnits[unitIndex] = null;
        }
    }

    public void RemoveFromSortedUnits(string nameUnit)
    {
        if (_sortedActiveUnitsByOrderOfAttack == null)
            return;

        for (int i = 0; i < _sortedActiveUnitsByOrderOfAttack.Count; i++)
        {
            if (_sortedActiveUnitsByOrderOfAttack[i] != null &&
                _sortedActiveUnitsByOrderOfAttack[i].GetName() == nameUnit)
            {
                _sortedActiveUnitsByOrderOfAttack[i] = null;
                break;
            }
        }
    }

    public void ReplaceFromSortedListWhenInvoked(Unit oldDemon, Unit newDemon)
    {
        for (int i = 0; i < _sortedActiveUnitsByOrderOfAttack.Count; i++)
        {
            if (_sortedActiveUnitsByOrderOfAttack[i] != null &&
                _sortedActiveUnitsByOrderOfAttack[i].GetName() == oldDemon.GetName())
            {
                _sortedActiveUnitsByOrderOfAttack[i] = newDemon;
            }
        }
    }

    public void AddDemonInTheLastSlot(Demon newDemon)
    {
        _sortedActiveUnitsByOrderOfAttack.Add(newDemon);
    }

    public void ReplaceFromReserveUnitsList(string newName, Demon replacedDemon)
    {
        for (int i = 0; i < _reservedUnits.Count; i++)
        {
            if (_reservedUnits[i] != null && _reservedUnits[i].GetName() == newName)
            {
                _reservedUnits[i] = replacedDemon;
                return;
            }
        }
    }

    public void AddUnitInSortedList(Unit newUnit)
    {
        _sortedActiveUnitsByOrderOfAttack.Add(newUnit);
    }
}