using Shin_Megami_Tensei.Enums;

namespace Shin_Megami_Tensei.Gadgets;

public class Affinity
{
    private readonly Dictionary<string, string> _values;

    public Affinity(Dictionary<string, string> values)
    {
        _values = values;
    }
    
    public string GetAffinityForType(AttackType attackType)
    {
        if (_values.TryGetValue(attackType.ToString(), out string affinity))
        {
            return string.IsNullOrWhiteSpace(affinity) ? "-" : affinity;
        }

        return "-"; 
    }
}