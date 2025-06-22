using Shin_Megami_Tensei;

namespace Shin_Megami_Tensei_View.Implementation.Interfaces;

public interface IHealingDisplayer
{
    void DisplayHealingForSingleTarget(Unit target, double amount);
    void DisplayHealingForMultiTargets(Unit caster, Unit target, double amount);
    void DisplayReviveForMultiTargets(Unit caster, Unit revived, double healAmount);
}