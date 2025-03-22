using System.Data;
using Shin_Megami_Tensei.Gadgets;

namespace Shin_Megami_Tensei;

public abstract class Unit
{
    protected string _name;
    private List<Skill> _skills = new List<Skill>();
    private Stat _stats;
    
    protected Unit(string name)
    {
        this._name = name;
    }

    public abstract void SetStatsFromJSON();
    public abstract void UpdateStatsFromJSON();
    public abstract void SetSkillsFromJSON(string stringWithSkills);
    public abstract void UpdateSkillsFromJSON();
    
    public string GetName()
    {
        return this._name;
    }
    
    public Stat GetStats()
    {
        return this._stats;
    }
}