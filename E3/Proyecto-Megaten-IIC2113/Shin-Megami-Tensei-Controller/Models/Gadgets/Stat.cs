namespace Shin_Megami_Tensei.Gadgets;

public class Stat
{
    private Dictionary<string, int> _stats = new Dictionary<string, int>();
    
    public Stat(int hp, int mp, int str, int skl, int mag, int spd, int lck)
    {
        _stats.Add("HP", hp);
        _stats.Add("MP", mp);
        _stats.Add("Str", str);
        _stats.Add("Skl", skl);
        _stats.Add("Mag", mag);
        _stats.Add("Spd", spd);
        _stats.Add("Lck", lck);
    }

    public int GetStatByName(string statName)
    {
        return _stats.TryGetValue(statName, out int value) ? value : 0;
    }
    
    public void SetStatByName(string statName, int value)
    {
        if (_stats.ContainsKey(statName))
        {
            _stats[statName] = value;
        }
    }
}