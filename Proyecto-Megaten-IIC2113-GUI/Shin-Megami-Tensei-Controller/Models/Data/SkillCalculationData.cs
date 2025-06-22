namespace Shin_Megami_Tensei.Data;

public class SkillCalculationData
{
    public int CasterLuck { get; }
    public int TargetLuck { get; }
    public double SkillPower { get; }

    public SkillCalculationData(int casterLuck, int targetLuck, double skillPower)
    {
        CasterLuck = casterLuck;
        TargetLuck = targetLuck;
        SkillPower = skillPower;
    }
}