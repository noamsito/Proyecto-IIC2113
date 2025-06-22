using Shin_Megami_Tensei.Controllers.Exceptions;
using Shin_Megami_Tensei.Enums;

namespace Shin_Megami_Tensei.Controllers.Collections;

public class ActiveUnitsCollection
    {
        private readonly Unit[] _slots;
        private readonly int _maxSlots;

        public ActiveUnitsCollection(int maxSlots = 4)
        {
            _maxSlots = maxSlots;
            _slots = new Unit[maxSlots];
        }

        public int MaxSlots => _maxSlots;
        public int OccupiedSlots => _slots.Count(slot => slot != null);

        public Unit GetUnitInSlot(int slotIndex)
        {
            ValidateSlotIndex(slotIndex);
            return _slots[slotIndex];
        }

        public void SetUnitInSlot(int slotIndex, Unit unit)
        {
            ValidateSlotIndex(slotIndex);
            _slots[slotIndex] = unit;
        }

        public void RemoveUnitFromSlot(int slotIndex)
        {
            ValidateSlotIndex(slotIndex);
            _slots[slotIndex] = null;
        }

        public bool IsSlotEmpty(int slotIndex)
        {
            ValidateSlotIndex(slotIndex);
            return _slots[slotIndex] == null;
        }

        public bool IsSlotOccupied(int slotIndex)
        {
            ValidateSlotIndex(slotIndex);
            return _slots[slotIndex] != null;
        }

        public IReadOnlyList<Unit> GetAllUnits()
        {
            return _slots.ToList().AsReadOnly();
        }

        public IReadOnlyList<Unit> GetAliveUnits()
        {
            return _slots.Where(u => u != null && IsUnitAlive(u)).ToList().AsReadOnly();
        }

        public IReadOnlyList<int> GetEmptySlotIndices()
        {
            var emptySlots = new List<int>();
            for (int i = 0; i < _maxSlots; i++)
            {
                if (_slots[i] == null)
                    emptySlots.Add(i);
            }
            return emptySlots.AsReadOnly();
        }

        public IReadOnlyList<int> GetOccupiedSlotIndices()
        {
            var occupiedSlots = new List<int>();
            for (int i = 0; i < _maxSlots; i++)
            {
                if (_slots[i] != null)
                    occupiedSlots.Add(i);
            }
            return occupiedSlots.AsReadOnly();
        }

        public int FindUnitSlotIndex(Unit unit)
        {
            if (unit == null)
                throw new ArgumentNullException(nameof(unit));

            for (int i = 0; i < _maxSlots; i++)
            {
                if (_slots[i] == unit)
                    return i;
            }

            throw new UnitNotFoundException(unit.GetName());
        }

        public bool TryFindUnitSlotIndex(Unit unit, out int slotIndex)
        {
            slotIndex = -1;
            if (unit == null)
                return false;

            for (int i = 0; i < _maxSlots; i++)
            {
                if (_slots[i] == unit)
                {
                    slotIndex = i;
                    return true;
                }
            }

            return false;
        }

        public void SwapUnits(int slot1, int slot2)
        {
            ValidateSlotIndex(slot1);
            ValidateSlotIndex(slot2);

            var temp = _slots[slot1];
            _slots[slot1] = _slots[slot2];
            _slots[slot2] = temp;
        }

        public bool HasAliveUnits()
        {
            return _slots.Any(u => u != null && IsUnitAlive(u));
        }

        public int CountAliveUnits()
        {
            return _slots.Count(u => u != null && IsUnitAlive(u));
        }

        private void ValidateSlotIndex(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _maxSlots)
                throw new InvalidSlotException(slotIndex);
        }

        private static bool IsUnitAlive(Unit unit)
        {
            return unit.GetCurrentStats().GetStatByName(StatType.HP.ToGameString()) > 0;
        }
    }
