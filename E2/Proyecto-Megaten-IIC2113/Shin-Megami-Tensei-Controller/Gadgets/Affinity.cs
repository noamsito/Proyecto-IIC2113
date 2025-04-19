namespace Shin_Megami_Tensei.Gadgets;

public class Affinity
{
    private readonly Dictionary<string, string> _values;

    public Affinity(Dictionary<string, string> values)
    {
        _values = values;
    }

    public string GetReactionTo(string attackType)
    {
        return _values.ContainsKey(attackType) ? _values[attackType] : "-";
    }

    public Dictionary<string, string> AsDictionary() => new(_values);
}