using Shin_Megami_Tensei.Data;
using Shin_Megami_Tensei.Files_Handlers;
using Shin_Megami_Tensei.Gadgets;

namespace Shin_Megami_Tensei;

public class Demon : Unit
{
    public Demon(string name) : base(name)
    {
        DemonData data = DemonDataFactory.LoadFromJson(name);
        InitializeData(data);
    }

    private void InitializeData(DemonData data)
    {
        _baseStats = data.BaseStats;
        _currentStats = new Stat(
            _baseStats.GetStatByName("HP"),
            _baseStats.GetStatByName("MP"),
            _baseStats.GetStatByName("Str"),
            _baseStats.GetStatByName("Skl"),
            _baseStats.GetStatByName("Mag"),
            _baseStats.GetStatByName("Spd"),
            _baseStats.GetStatByName("Lck")
        );
        _skills = data.Skills;
        _affinity = data.Affinity;
    }

    public override void SetStatsFromJson(string jsonPathFile) { }
    public override void UpdateStatsFromJson() { }
    public override void UpdateSkillsFromJson() { }
}