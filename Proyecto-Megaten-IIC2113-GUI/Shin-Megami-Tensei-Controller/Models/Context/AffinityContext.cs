using Shin_Megami_Tensei_View;
using Shin_Megami_Tensei.Enums;

namespace Shin_Megami_Tensei.Combat;

public class AffinityContext
{
    public Unit Caster { get; }
    public Unit Target { get; set;  }
    public AttackType AttackType { get; }
    public double BaseDamage { get; }

    public AffinityContext(Unit caster, Unit target, AttackType attackType, double baseDamage)
    {
        Caster = caster;
        Target = target;
        AttackType = attackType;
        BaseDamage = baseDamage;
    }
}
