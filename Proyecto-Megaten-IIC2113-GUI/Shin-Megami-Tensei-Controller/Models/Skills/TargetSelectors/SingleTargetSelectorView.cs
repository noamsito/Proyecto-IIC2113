using Shin_Megami_Tensei.Controllers.Exceptions;
using Shin_Megami_Tensei.Enums;
using Shin_Megami_Tensei.Gadgets;
using Shin_Megami_Tensei.Managers;
using Shin_Megami_Tensei.Managers.Base;

namespace Shin_Megami_Tensei.Skills.TargetSelectors;

public class SingleTargetSelectorView : ITargetSelector
{
    private readonly bool _targetsEnemies;

    public SingleTargetSelectorView(bool targetsEnemies = true)
    {
        _targetsEnemies = targetsEnemies;
    }

    public IEnumerable<Unit> SelectTargets(SkillExecutionContext context)
    {
        var targetPlayer = _targetsEnemies ? context.OpponentPlayer : context.CasterPlayer;
        var validTargets = GetValidTargets(targetPlayer);

        if (!validTargets.Any())
            throw new NoValidTargetsException();

        // In a real implementation, this would use a UI selection mechanism
        // For now, return the first valid target
        return new[] { validTargets.First() };
    }

    public bool HasValidTargets(SkillExecutionContext context)
    {
        var targetPlayer = _targetsEnemies ? context.OpponentPlayer : context.CasterPlayer;
        return GetValidTargets(targetPlayer).Any();
    }

    private IEnumerable<Unit> GetValidTargets(Player player)
    {
        return player.UnitManager.GetActiveUnits()
            .Where(u => u != null && u.GetCurrentStats().GetStatByName(StatType.HP.ToGameString()) > 0);
    }
}
