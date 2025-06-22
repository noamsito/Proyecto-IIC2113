using Shin_Megami_Tensei;

public class DemonSelectionResult
{
    public bool Success { get; private set; }
    public Unit SelectedDemon { get; private set; }

    private DemonSelectionResult(bool success, Unit demon = null)
    {
        Success = success;
        SelectedDemon = demon;
    }

    public static DemonSelectionResult FromSuccess(Unit demon) => new(true, demon);
    public static DemonSelectionResult Cancelled() => new(false);
}