namespace Shin_Megami_Tensei.Managers.Helpers;

public class SkillTargetResult
{
    public bool WasSelected { get; private set; }
    public Unit SelectedTarget { get; private set; }

    private SkillTargetResult(bool wasSelected, Unit target = null)
    {
        WasSelected = wasSelected;
        SelectedTarget = target;
    }

    public static SkillTargetResult Selected(Unit target) => new(true, target);
    public static SkillTargetResult Cancelled() => new(false);
}