using System.Data;

namespace Shin_Megami_Tensei;

public abstract class Unit
{
    public string Name;
    public Dictionary<string, int> Stats;

    protected Unit(string name)
    {
        this.Name = name;
    }

    public abstract void SetStats(Dictionary<string, int> stats);
    public abstract void UpdateStats();
}