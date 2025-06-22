using Shin_Megami_Tensei.Combat;
using Shin_Megami_Tensei.Gadgets;

namespace Shin_Megami_Tensei.Data;

public class SkillExecutionData
{
    public SkillUseContext SkillContext { get; }
    public TurnContext TurnContext { get; }
    public Skill Skill { get; }
    public int NumberOfHits { get; }

    public SkillExecutionData(SkillUseContext skillCtx, TurnContext turnCtx, int numberOfHits)
    {
        SkillContext = skillCtx;
        TurnContext = turnCtx;
        Skill = skillCtx.Skill;
        NumberOfHits = numberOfHits;
    }
}
