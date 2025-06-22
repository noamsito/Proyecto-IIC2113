using Shin_Megami_Tensei;
using Shin_Megami_Tensei.Combat;

namespace Shin_Megami_Tensei_View.Implementation.Interfaces;

public interface IAffinityDisplayer
{
    void DisplayAffinityMessage(AffinityContext affinityContext);
    void DisplayWeakMessage(Unit target, Unit attacker);
    void DisplayResistMessage(Unit target, Unit attacker);
    void DisplayBlockMessage(Unit target, Unit attacker);
    void DisplayRepelMessage(Unit target, Unit caster, int damage);
    void DisplayDrainMessage(Unit target, int amount);
}