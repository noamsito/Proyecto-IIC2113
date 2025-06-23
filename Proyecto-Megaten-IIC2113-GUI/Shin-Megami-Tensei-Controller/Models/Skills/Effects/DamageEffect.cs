using Shin_Megami_Tensei;
using Shin_Megami_Tensei.Affinities;
using Shin_Megami_Tensei.Controllers.Exceptions;
using Shin_Megami_Tensei.Enums;
using Shin_Megami_Tensei.Managers;
using Shin_Megami_Tensei.Managers.Base;

public class DamageEffect : ISkillEffect
{
    private readonly AttackType _attackType;
    private readonly int _power;

    public DamageEffect(AttackType attackType, int power)
    {
        _attackType = attackType;
        _power = power;
    }

    public void Apply(SkillExecutionContext context)
    {
        if (!CanApply(context))
            throw new InvalidActionException("Cannot apply damage effect");

        var baseDamage = CalculateBaseDamage(context);
        var affinityHandler = AffinityHandlerFactory.CreateHandler(_attackType);
            
        affinityHandler.ApplyDamage(context.Caster, context, baseDamage);
    }

    public bool CanApply(SkillExecutionContext context)
    {
        return true;
    }

    private double CalculateBaseDamage(SkillExecutionContext context)
    {
        var stat = GetRelevantStat(context.Caster);
        return Math.Sqrt(stat * _power);
    }

    private int GetRelevantStat(Unit caster)
    {
        return _attackType switch
        {
            AttackType.Phys => caster.GetCurrentStats().GetStatByName(StatType.Strength.ToGameString()),
            AttackType.Gun => caster.GetCurrentStats().GetStatByName(StatType.Skill.ToGameString()),
            AttackType.Fire or AttackType.Ice or AttackType.Elec or AttackType.Force or AttackType.Almighty 
                => caster.GetCurrentStats().GetStatByName(StatType.Magic.ToGameString()),
            _ => caster.GetCurrentStats().GetStatByName(StatType.Magic.ToGameString())
        };
    }
}