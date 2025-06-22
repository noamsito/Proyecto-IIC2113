using Shin_Megami_Tensei.Controllers.Exceptions;
using Shin_Megami_Tensei.Managers;
using Shin_Megami_Tensei.Managers.Base;

public class HealEffect : ISkillEffect
{
    private readonly int _healingPower;

    public HealEffect(int healingPower)
    {
        _healingPower = healingPower;
    }

    public void Apply(SkillExecutionContext context)
    {
        if (!CanApply(context))
            throw new InvalidActionException("Cannot apply heal effect");

        // Implementation would have a target parameter in a real scenario
        // For now, this is a simplified version
        var healAmount = CalculateHealAmount(context);
        ApplyHealing(context, healAmount);
    }

    public bool CanApply(SkillExecutionContext context)
    {
        return true; // Healing can always be attempted
    }

    private double CalculateHealAmount(SkillExecutionContext context)
    {
        // This would target a specific unit in a real implementation
        var baseHP = 100; // Placeholder
        return Math.Floor((_healingPower / 100.0) * baseHP);
    }

    private void ApplyHealing(SkillExecutionContext context, double healAmount)
    {
        // Apply healing logic here
        // This would be applied to the selected target(s)
    }
}