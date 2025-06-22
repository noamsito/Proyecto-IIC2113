using Shin_Megami_Tensei;
using Shin_Megami_Tensei.Managers.Base;

public class NullAffinityHandler : BaseAffinityHandler
{
    public override void ApplyDamage(Unit caster, SkillExecutionContext context, double baseDamage)
    {
        // No damage applied
        // Display null message
        // Consume extra turns
    }

    public override void ApplyInstantKill(Unit caster, SkillExecutionContext context, int accuracy)
    {
        // No effect
        // Display block message
    }
}