using Shin_Megami_Tensei;
using Shin_Megami_Tensei.Gadgets;

namespace Shin_Megami_Tensei_View.Implementation.Interfaces;

public interface IOptionDisplayer
{
    void DisplaySkills(IReadOnlyList<Skill> skills);
    void DisplaySummonOptions(IReadOnlyList<Unit> availableUnits);
    void DisplaySummonOptionsIncludingDead(IReadOnlyList<Unit> allUnits);
    void DisplayUnitsGiven(IReadOnlyList<Unit> units);
}