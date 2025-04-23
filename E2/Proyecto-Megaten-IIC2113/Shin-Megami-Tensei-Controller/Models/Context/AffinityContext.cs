using Shin_Megami_Tensei_View;

namespace Shin_Megami_Tensei.Combat;

public class AffinityContext
{
    public Unit Caster { get; }
    public Unit Target { get; }
    public string AttackType { get; }
    public int BaseDamage { get; }

    public AffinityContext(Unit caster, Unit target, string attackType, int baseDamage)
    {
        Caster = caster;
        Target = target;
        AttackType = attackType;
        BaseDamage = baseDamage;
    }
}
