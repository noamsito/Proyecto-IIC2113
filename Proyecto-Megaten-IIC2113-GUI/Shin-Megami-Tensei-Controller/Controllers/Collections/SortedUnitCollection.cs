using Shin_Megami_Tensei;
using Shin_Megami_Tensei.Controllers.Exceptions;

public class SortedUnitCollection
    {
        private readonly List<Unit> _sortedUnits;

        public SortedUnitCollection()
        {
            _sortedUnits = new List<Unit>();
        }

        public int Count => _sortedUnits.Count;
        public bool IsEmpty => _sortedUnits.Count == 0;

        public Unit GetFirstUnit()
        {
            if (IsEmpty)
                throw new InvalidOperationException("No units in sorted collection");

            return _sortedUnits[0];
        }

        public bool TryGetFirstUnit(out Unit unit)
        {
            unit = null;
            if (IsEmpty)
                return false;

            unit = _sortedUnits[0];
            return true;
        }

        public void SetSortedUnits(IEnumerable<Unit> units)
        {
            if (units == null)
                throw new ArgumentNullException(nameof(units));

            _sortedUnits.Clear();
            _sortedUnits.AddRange(units.Where(u => u != null));
        }

        public void AddUnit(Unit unit)
        {
            if (unit == null)
                throw new ArgumentNullException(nameof(unit));

            _sortedUnits.Add(unit);
        }

        public void RemoveUnit(Unit unit)
        {
            if (unit == null)
                throw new ArgumentNullException(nameof(unit));

            if (!_sortedUnits.Remove(unit))
                throw new UnitNotFoundException(unit.GetName());
        }

        public bool TryRemoveUnit(Unit unit)
        {
            if (unit == null)
                return false;

            return _sortedUnits.Remove(unit);
        }

        public void ReplaceUnit(Unit oldUnit, Unit newUnit)
        {
            if (oldUnit == null)
                throw new ArgumentNullException(nameof(oldUnit));
            if (newUnit == null)
                throw new ArgumentNullException(nameof(newUnit));

            var index = _sortedUnits.IndexOf(oldUnit);
            if (index == -1)
                throw new UnitNotFoundException(oldUnit.GetName());

            _sortedUnits[index] = newUnit;
        }

        public void RotateToNext()
        {
            if (_sortedUnits.Count <= 1)
                return;

            var firstUnit = _sortedUnits[0];
            _sortedUnits.RemoveAt(0);
            _sortedUnits.Add(firstUnit);
        }

        public IReadOnlyList<Unit> GetAllUnits()
        {
            return _sortedUnits.AsReadOnly();
        }

        public void Clear()
        {
            _sortedUnits.Clear();
        }
    }
