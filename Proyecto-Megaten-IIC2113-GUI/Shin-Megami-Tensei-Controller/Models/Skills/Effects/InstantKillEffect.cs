using Shin_Megami_Tensei.Affinities;
using Shin_Megami_Tensei.Controllers.Exceptions;
using Shin_Megami_Tensei.Enums;
using Shin_Megami_Tensei.Managers;
using Shin_Megami_Tensei.Managers.Base;

namespace Shin_Megami_Tensei.Skills.Effects;

public class InstantKillEffect : ISkillEffect
{
    private readonly AttackType _attackType;
    private readonly int _accuracy;

    public InstantKillEffect(AttackType attackType, int accuracy)
    {
        _attackType = attackType;
        _accuracy = accuracy;
    }

    public void Apply(SkillExecutionContext context)
    {
        if (!CanApply(context))
            throw new InvalidActionException("Cannot apply instant kill effect");

        var affinityHandler = AffinityHandlerFactory.CreateHandler(_attackType);
        affinityHandler.ApplyInstantKill(context.Caster, context, _accuracy);
    }

    public bool CanApply(SkillExecutionContext context)
    {
        return true;
    }
}
