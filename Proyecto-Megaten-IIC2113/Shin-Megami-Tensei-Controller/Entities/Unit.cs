using System.Data;
using Shin_Megami_Tensei.Gadgets;

namespace Shin_Megami_Tensei;

public abstract class Unit
{
    protected string _name;
    protected List<Skill> _skills = new List<Skill>();
    protected Stat _stats;
    
    protected Unit(string name)
    {
        this._name = name;
    }

    public abstract void SetStatsFromJSON();
    public abstract void UpdateStatsFromJSON();
    public abstract void SetSkillsFromJSON(List<string> skillsList);
    public abstract void UpdateSkillsFromJSON();
    
    public string GetName()
    {
        return this._name;
    }
    
    public Stat GetStats()
    {
        return this._stats;
    }

    public List<Skill> GetSkills()
    {
        return this._skills;
    }
 }