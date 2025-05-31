using Shin_Megami_Tensei.Combat;

namespace Shin_Megami_Tensei.Data;

public class HitExecutionResults
{
    public List<AffinityContext> AllAffinityContexts { get; }
    public List<Unit> RepelTargets { get; }
    public double TotalRepelDamage { get; set; }

    public HitExecutionResults()
    {
        AllAffinityContexts = new List<AffinityContext>();
        RepelTargets = new List<Unit>();
        TotalRepelDamage = 0;
    }
}