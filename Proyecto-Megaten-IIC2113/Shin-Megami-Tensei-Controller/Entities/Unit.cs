using System.Data;
using Shin_Megami_Tensei.Gadgets;

namespace Shin_Megami_Tensei;

public abstract class Unit
{
    protected string _name;
    protected List<Skill> _skills = new List<Skill>();
    protected Dictionary<string, string> Affinity;
    protected const string JSON_FILE_SAMURAI = "data/samurai.json";
    protected const string JSON_FILE_MONSTERS = "data/monsters.json";
    protected const string JSON_FILE_SKILLS = "data/samurai.json";
    protected Stat _baseStats;
    protected Stat _currentStats;
    
    protected Unit(string name)
    {
        this._name = name;
    }

    public abstract void SetStatsFromJSON();
    public abstract void UpdateStatsFromJSON();
    public abstract void UpdateSkillsFromJSON();
    
    public string GetName()
    {
        return this._name;
    }
    
    public Stat GetBaseStats()
    {
        return this._baseStats;
    }
    
    public Stat GetCurrentStats()
    {
        return this._currentStats;
    }

    public List<Skill> GetSkills()
    {
        return this._skills;
    }
 }