using Shin_Megami_Tensei.Enums;
using Shin_Megami_Tensei.Gadgets;

namespace Shin_Megami_Tensei.Managers;

public static class AffinityResolver
{
    public static string GetAffinity(Unit target, AttackType attackType)
    {
        Affinity affinity = target.GetAffinity();
        return affinity.GetAffinityForType(attackType);
    }
}
