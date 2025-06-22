using Shin_Megami_Tensei_GUI;
using Shin_Megami_Tensei;

public class UnitAdapter : IUnit
{
    public string Name { get; }
    public int HP { get; }
    public int MP { get; }
    public int MaxHP { get; }
    public int MaxMP { get; }

    public UnitAdapter(Unit unit)
    {
        if (unit == null) throw new ArgumentNullException(nameof(unit));

        Name = unit.GetName();
        HP = unit.GetCurrentStats().GetStatByName("HP");
        MP = unit.GetCurrentStats().GetStatByName("MP");
        MaxHP = unit.GetBaseStats().GetStatByName("HP");
        MaxMP = unit.GetBaseStats().GetStatByName("MP");
    }
}