using Shin_Megami_Tensei.Managers.Base;

namespace Shin_Megami_Tensei.Affinities.Handlers;

public class RepelAffinityHandler : BaseAffinityHandler
{
    public override void ApplyDamage(Unit caster, SkillExecutionContext context, double baseDamage)
    {
        // Damage reflected back to caster
        ApplyDirectDamage(caster, baseDamage);
        // Display repel message
        // Consume all turns
    }

    public override void ApplyInstantKill(Unit caster, SkillExecutionContext context, int accuracy)
    {
        // Kill the caster instead
        KillUnit(caster);
        // Display miss message
    }
}
