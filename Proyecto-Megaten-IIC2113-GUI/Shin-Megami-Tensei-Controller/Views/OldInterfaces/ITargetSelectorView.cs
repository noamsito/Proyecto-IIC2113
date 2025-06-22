using Shin_Megami_Tensei;
using Shin_Megami_Tensei.Gadgets;

public interface ITargetSelectorView
{
    Unit SelectTarget(Player targetPlayer);
    Skill SelectSkill(Unit unit);
    Unit SelectSummonTarget(Player player);
    int SelectSlot(Player player);
}