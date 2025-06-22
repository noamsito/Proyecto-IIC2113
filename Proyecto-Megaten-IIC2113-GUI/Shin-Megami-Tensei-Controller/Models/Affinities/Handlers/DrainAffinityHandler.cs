using Shin_Megami_Tensei.Managers.Base;

namespace Shin_Megami_Tensei.Affinities.Handlers;

public class DrainAffinityHandler : BaseAffinityHandler
{
    public override void ApplyDamage(Unit caster, SkillExecutionContext context, double baseDamage)
    {
        // Target absorbs damage as healing
        ApplyHealing(caster, baseDamage);
        // Display drain message
        // Consume all turns
    }

    public override void ApplyInstantKill(Unit caster, SkillExecutionContext context, int accuracy)
    {
        // No effect
    }
}
