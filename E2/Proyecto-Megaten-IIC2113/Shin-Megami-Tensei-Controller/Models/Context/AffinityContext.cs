using Shin_Megami_Tensei_View;

namespace Shin_Megami_Tensei.Combat;

public class AffinityContext
{
    public Unit Caster { get; }
    public Unit Target { get; set;  }
    public string AttackType { get; }
    public double BaseDamage { get; }

    public AffinityContext(Unit caster, Unit target, string attackType, double baseDamage)
    {
        Caster = caster;
        Target = target;
        AttackType = attackType;
        BaseDamage = baseDamage;
    }
}
