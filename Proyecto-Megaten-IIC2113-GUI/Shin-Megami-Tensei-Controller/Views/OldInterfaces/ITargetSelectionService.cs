using Shin_Megami_Tensei.Gadgets;

namespace Shin_Megami_Tensei.Views.Interfaces;

public interface ITargetSelectionService
{
    Unit SelectTargetUnit(TargetSelectionContext context);
    Skill SelectSkillFromUnit(Unit unit);
    Unit SelectSummonableUnit(Player player);
    int SelectBoardSlot(Player player);
}
