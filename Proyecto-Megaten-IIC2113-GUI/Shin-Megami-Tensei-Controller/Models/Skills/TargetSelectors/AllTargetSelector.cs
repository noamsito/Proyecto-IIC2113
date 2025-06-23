using Shin_Megami_Tensei;
using Shin_Megami_Tensei.Enums;
using Shin_Megami_Tensei.Managers;
using Shin_Megami_Tensei.Managers.Base;

public class AllTargetSelector : ITargetSelector
{
    private readonly bool _targetsEnemies;

    public AllTargetSelector(bool targetsEnemies = true)
    {
        _targetsEnemies = targetsEnemies;
    }

    public IEnumerable<Unit> SelectTargets(SkillExecutionContext context)
    {
        var targetPlayer = _targetsEnemies ? context.OpponentPlayer : context.CasterPlayer;
        return GetValidTargets(targetPlayer);
    }

    public bool HasValidTargets(SkillExecutionContext context)
    {
        var targetPlayer = _targetsEnemies ? context.OpponentPlayer : context.CasterPlayer;
        return GetValidTargets(targetPlayer).Any();
    }

    private IEnumerable<Unit> GetValidTargets(Player player)
    {
        var targets = new List<Unit>();
            
        targets.AddRange(player.UnitManager.GetActiveUnits()
            .Where(u => u != null && u.GetCurrentStats().GetStatByName(StatType.Hp.ToGameString()) > 0));
            
        if (!_targetsEnemies)
        {
            targets.AddRange(player.UnitManager.GetReservedUnits()
                .Where(u => u != null && u.GetCurrentStats().GetStatByName(StatType.Hp.ToGameString()) > 0));
        }

        return targets;
    }
}