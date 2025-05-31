namespace Shin_Megami_Tensei.Managers.Helpers;

public class AttackTargetResult
{
    public bool WasSelected { get; private set; }
    public Unit SelectedTarget { get; private set; }

    private AttackTargetResult(bool wasSelected, Unit target = null)
    {
        WasSelected = wasSelected;
        SelectedTarget = target;
    }

    public static AttackTargetResult Selected(Unit target) => new(true, target);
    public static AttackTargetResult Cancelled() => new(false);
}
