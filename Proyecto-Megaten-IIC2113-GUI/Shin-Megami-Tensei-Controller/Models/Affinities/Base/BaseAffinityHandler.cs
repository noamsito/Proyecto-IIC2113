using Shin_Megami_Tensei;
using Shin_Megami_Tensei.Affinities.Interfaces;
using Shin_Megami_Tensei.Enums;
using Shin_Megami_Tensei.Managers.Base;

public abstract class BaseAffinityHandler : IAffinityHandler
{
    public abstract void ApplyDamage(Unit caster, SkillExecutionContext context, double baseDamage);
    public abstract void ApplyInstantKill(Unit caster, SkillExecutionContext context, int accuracy);

    public AffinityType GetAffinity(Unit target, AttackType attackType)
    {
        var affinityString = target.GetAffinity().GetAffinityForType(attackType);
        return AffinityTypeExtensions.FromGameString(affinityString);
    }

    protected void ApplyDirectDamage(Unit target, double damage)
    {
        var currentHP = target.GetCurrentStats().GetStatByName(StatType.Hp.ToGameString());
        var damageToApply = Convert.ToInt32(Math.Floor(damage));
        var newHP = Math.Max(0, currentHP - damageToApply);
        target.GetCurrentStats().SetStatByName(StatType.Hp.ToGameString(), newHP);
    }

    protected void ApplyHealing(Unit target, double healAmount)
    {
        var currentHP = target.GetCurrentStats().GetStatByName(StatType.Hp.ToGameString());
        var maxHP = target.GetBaseStats().GetStatByName(StatType.Hp.ToGameString());
        var healing = Convert.ToInt32(Math.Floor(healAmount));
        var newHP = Math.Min(maxHP, currentHP + healing);
        target.GetCurrentStats().SetStatByName(StatType.Hp.ToGameString(), newHP);
    }

    protected void KillUnit(Unit target)
    {
        target.GetCurrentStats().SetStatByName(StatType.Hp.ToGameString(), 0);
    }
}