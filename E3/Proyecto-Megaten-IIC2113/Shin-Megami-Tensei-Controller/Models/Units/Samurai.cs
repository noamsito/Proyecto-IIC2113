using Shin_Megami_Tensei.Data;
using Shin_Megami_Tensei.Files_Handlers;
using Shin_Megami_Tensei.Gadgets;
using Shin_Megami_Tensei.Units;

namespace Shin_Megami_Tensei;

public class Samurai : Unit
{
    public Samurai(string name, List<string> skills) : base(name)
    {
        SamuraiData samuraiData = SamuraiDataFactory.LoadFromJson(this.GetName(), skills);
        this.InitializeData(samuraiData);
    }

    public void InitializeData(SamuraiData data)
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

    public int GetSkillCount() => _skills.Count;
}
