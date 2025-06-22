namespace Shin_Megami_Tensei.Gadgets;

public class Affinity
{
    private readonly Dictionary<string, string> _values;

    public Affinity(Dictionary<string, string> values)
    {
        _values = values;
    }
    
    public string GetAffinityForType(string attackType)
    {
        if (_values.TryGetValue(attackType, out string affinity))
        {
            return string.IsNullOrWhiteSpace(affinity) ? "-" : affinity;
        }

        return "-"; 
    }
}