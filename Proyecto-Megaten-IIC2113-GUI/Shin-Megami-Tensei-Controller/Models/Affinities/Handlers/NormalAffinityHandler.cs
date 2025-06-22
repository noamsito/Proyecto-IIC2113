using Shin_Megami_Tensei.Enums;
using Shin_Megami_Tensei.Managers.Base;

namespace Shin_Megami_Tensei.Affinities.Handlers;

public class NormalAffinityHandler : BaseAffinityHandler
{
    public override void ApplyDamage(Unit caster, SkillExecutionContext context, double baseDamage)
    {
        // Apply normal damage
        // No special effects
    }

    public override void ApplyInstantKill(Unit caster, SkillExecutionContext context, int accuracy)
    {
        var casterLuck = caster.GetCurrentStats().GetStatByName(StatType.Luck.ToGameString());
        // Calculate success based on normal luck comparison
        // Apply effect or show miss message
    }
}
