using Shin_Megami_Tensei.Gadgets;

namespace Shin_Megami_Tensei;

public abstract class Unit
{
    protected string _name;
    protected List<Skill> _skills;
    protected Affinity _affinity;

    protected Stat _baseStats;
    protected Stat _currentStats;

    protected Unit(string name)
    {
        _name = name;
        _skills = new List<Skill>();
    }

    public abstract void SetStatsFromJson(string jsonPathFile);

    public abstract void UpdateStatsFromJson();
    public abstract void UpdateSkillsFromJson();

    public string GetName() => _name;

    public Stat GetBaseStats() => _baseStats;
    public Stat GetCurrentStats() => _currentStats;

    public IReadOnlyList<Skill> GetSkills() => _skills.AsReadOnly();

    public Affinity GetAffinity() => _affinity;

    public void ApplyDamageTaken(int damage)
    {
        int currentHP = _currentStats.GetStatByName("HP");
        int newHP = Math.Max(0, currentHP - damage);
        _currentStats.SetStatByName("HP", newHP);
    }

    public bool IsAlive() => _currentStats.GetStatByName("HP") > 0;
}