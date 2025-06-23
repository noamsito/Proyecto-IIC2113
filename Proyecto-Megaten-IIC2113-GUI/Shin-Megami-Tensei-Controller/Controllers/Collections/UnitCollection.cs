using Shin_Megami_Tensei.Controllers.Exceptions;
using Shin_Megami_Tensei.Enums;

namespace Shin_Megami_Tensei.Controllers.Collections;

public class UnitCollection
    {
        private readonly List<Unit> _units;
        private readonly int _maxCapacity;

        public UnitCollection(int maxCapacity = int.MaxValue)
        {
            _units = new List<Unit>();
            _maxCapacity = maxCapacity;
        }

        public int Count => _units.Count;
        public bool IsFull => _units.Count >= _maxCapacity;
        public bool IsEmpty => _units.Count == 0;

        public void AddUnit(Unit unit)
        {
            if (unit == null)
                throw new ArgumentNullException(nameof(unit));

            if (IsFull)
                throw new InvalidOperationException($"Collection is at maximum capacity of {_maxCapacity}");

            _units.Add(unit);
        }

        public bool TryAddUnit(Unit unit)
        {
            if (unit == null || IsFull)
                return false;

            _units.Add(unit);
            return true;
        }

        public void RemoveUnit(Unit unit)
        {
            if (unit == null)
                throw new ArgumentNullException(nameof(unit));

            if (!_units.Remove(unit))
                throw new UnitNotFoundException(unit.GetName());
        }

        public bool TryRemoveUnit(Unit unit)
        {
            if (unit == null)
                return false;

            return _units.Remove(unit);
        }

        public Unit GetUnitByName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be null or empty", nameof(name));

            var unit = _units.FirstOrDefault(u => u.GetName().Equals(name, StringComparison.OrdinalIgnoreCase));
            if (unit == null)
                throw new UnitNotFoundException(name);

            return unit;
        }

        public bool TryGetUnitByName(string name, out Unit unit)
        {
            unit = null;
            if (string.IsNullOrWhiteSpace(name))
                return false;

            unit = _units.FirstOrDefault(u => u.GetName().Equals(name, StringComparison.OrdinalIgnoreCase));
            return unit != null;
        }

        public IReadOnlyList<Unit> GetAliveUnits()
        {
            return _units.Where(u => u != null && IsUnitAlive(u)).ToList().AsReadOnly();
        }

        public IReadOnlyList<Unit> GetDeadUnits()
        {
            return _units.Where(u => u != null && !IsUnitAlive(u)).ToList().AsReadOnly();
        }

        public IReadOnlyList<Unit> GetAllUnits()
        {
            return _units.AsReadOnly();
        }

        public bool HasAliveUnits()
        {
            return _units.Any(u => u != null && IsUnitAlive(u));
        }

        public int CountAliveUnits()
        {
            return _units.Count(u => u != null && IsUnitAlive(u));
        }

        public void Clear()
        {
            _units.Clear();
        }

        public bool Contains(Unit unit)
        {
            return unit != null && _units.Contains(unit);
        }

        private static bool IsUnitAlive(Unit unit)
        {
            return unit.GetCurrentStats().GetStatByName(StatType.Hp.ToGameString()) > 0;
        }
    }
