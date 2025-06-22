using Shin_Megami_Tensei.Affinities.Handlers;
using Shin_Megami_Tensei.Affinities.Interfaces;
using Shin_Megami_Tensei.Enums;

namespace Shin_Megami_Tensei.Affinities;

public static class AffinityHandlerFactory
{
    private static readonly Dictionary<AffinityType, IAffinityHandler> _handlers = new()
    {
        { AffinityType.Weak, new WeakAffinityHandler() },
        { AffinityType.Resistant, new ResistantAffinityHandler() },
        { AffinityType.Null, new NullAffinityHandler() },
        { AffinityType.Repel, new RepelAffinityHandler() },
        { AffinityType.Drain, new DrainAffinityHandler() },
        { AffinityType.Normal, new NormalAffinityHandler() }
    };

    public static IAffinityHandler CreateHandler(AttackType attackType)
    {
        // This would be more complex in a real implementation
        // For now, return a normal handler
        return _handlers[AffinityType.Normal];
    }

    public static IAffinityHandler CreateHandler(AffinityType affinityType)
    {
        if (_handlers.TryGetValue(affinityType, out var handler))
            return handler;

        return _handlers[AffinityType.Normal];
    }
}
