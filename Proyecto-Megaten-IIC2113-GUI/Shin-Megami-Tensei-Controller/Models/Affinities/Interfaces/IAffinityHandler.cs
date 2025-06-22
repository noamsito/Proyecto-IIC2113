using Shin_Megami_Tensei.Enums;
using Shin_Megami_Tensei.Managers.Base;

namespace Shin_Megami_Tensei.Affinities.Interfaces;

public interface IAffinityHandler
{
    void ApplyDamage(Unit caster, SkillExecutionContext context, double baseDamage);
    void ApplyInstantKill(Unit caster, SkillExecutionContext context, int accuracy);
    AffinityType GetAffinity(Unit target, AttackType attackType);
}
