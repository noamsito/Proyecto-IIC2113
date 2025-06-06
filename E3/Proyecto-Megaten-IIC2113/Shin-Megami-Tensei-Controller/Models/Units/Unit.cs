﻿using Shin_Megami_Tensei.Gadgets;

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
    
    public string GetName() => _name;

    public Stat GetBaseStats() => _baseStats;
    public Stat GetCurrentStats() => _currentStats;

    public IReadOnlyList<Skill> GetSkills() => _skills.AsReadOnly();

    public Affinity GetAffinity() => _affinity;

    public bool IsAlive() => _currentStats.GetStatByName("HP") > 0;
}