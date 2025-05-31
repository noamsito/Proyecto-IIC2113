namespace Shin_Megami_Tensei.Models.Helpers;

public class UnitActionResult
{
    public bool ShouldCheckVictory { get; private set; }

    private UnitActionResult(bool shouldCheckVictory)
    {
        ShouldCheckVictory = shouldCheckVictory;
    }

    public static UnitActionResult ActionExecuted() => new(true);
    public static UnitActionResult SkippedDeadUnit() => new(false);
}