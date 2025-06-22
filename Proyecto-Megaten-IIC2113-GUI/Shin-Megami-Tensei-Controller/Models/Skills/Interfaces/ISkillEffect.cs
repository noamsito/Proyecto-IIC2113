using Shin_Megami_Tensei.Managers.Base;

namespace Shin_Megami_Tensei.Managers;

public interface ISkillEffect
{
    void Apply(SkillExecutionContext context);
    bool CanApply(SkillExecutionContext context);
}
