using Shin_Megami_Tensei.Managers.Base;

namespace Shin_Megami_Tensei.Managers;

public interface ITargetSelector
{
    IEnumerable<Unit> SelectTargets(SkillExecutionContext context);
    bool HasValidTargets(SkillExecutionContext context);
}
