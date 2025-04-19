using Shin_Megami_Tensei.Gadgets;

namespace Shin_Megami_Tensei.Data;

public class DemonData
{
    public string Name { get; init; }
    public Stat BaseStats { get; init; }
    public Affinity Affinity { get; init; }
    public List<Skill> Skills { get; init; }

    public DemonData(string name, Stat baseStats, Affinity affinity, List<Skill> skills)
    {
        Name = name;
        BaseStats = baseStats;
        Affinity = affinity;
        Skills = skills;
    }
}