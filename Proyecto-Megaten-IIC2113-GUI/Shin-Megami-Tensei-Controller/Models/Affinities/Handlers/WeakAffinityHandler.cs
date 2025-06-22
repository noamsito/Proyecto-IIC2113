using Shin_Megami_Tensei.Managers.Base;

namespace Shin_Megami_Tensei.Affinities.Handlers;

public class WeakAffinityHandler : BaseAffinityHandler
{
    public override void ApplyDamage(Unit caster, SkillExecutionContext context, double baseDamage)
    {
        var amplifiedDamage = baseDamage * GameConstants.MULTIPLIER_WEAK_AFFINITY;
        // Apply amplified damage to target
        // Display weak message
        // Grant extra turn
    }

    public override void ApplyInstantKill(Unit caster, SkillExecutionContext context, int accuracy)
    {
        // Instant kill succeeds against weak affinity
        // Display messages and kill target
    }
}
