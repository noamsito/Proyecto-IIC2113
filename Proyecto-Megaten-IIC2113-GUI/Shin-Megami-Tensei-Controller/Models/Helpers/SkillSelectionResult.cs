using Shin_Megami_Tensei.Gadgets;

namespace Shin_Megami_Tensei.Managers.Helpers;

public class SkillSelectionResult
{
    public bool WasSelected { get; private set; }
    public Skill SelectedSkill { get; private set; }

    private SkillSelectionResult(bool wasSelected, Skill skill = null)
    {
        WasSelected = wasSelected;
        SelectedSkill = skill;
    }

    public static SkillSelectionResult Selected(Skill skill) => new(true, skill);
    public static SkillSelectionResult Cancelled() => new(false);
}
