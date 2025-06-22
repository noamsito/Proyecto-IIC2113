using Shin_Megami_Tensei;
using Shin_Megami_Tensei.Enums;
using Shin_Megami_Tensei.Managers.Base;

public class ResistantAffinityHandler : BaseAffinityHandler
{
    public override void ApplyDamage(Unit caster, SkillExecutionContext context, double baseDamage)
    {
        var reducedDamage = baseDamage / GameConstants.MULTIPLIER_RESISTANT_AFFINITY;
        // Apply reduced damage to target
        // Display resistant message
    }

    public override void ApplyInstantKill(Unit caster, SkillExecutionContext context, int accuracy)
    {
        var casterLuck = caster.GetCurrentStats().GetStatByName(StatType.Luck.ToGameString());
        // Calculate success based on 2x target luck requirement
        // Apply effect or show miss message
    }
}