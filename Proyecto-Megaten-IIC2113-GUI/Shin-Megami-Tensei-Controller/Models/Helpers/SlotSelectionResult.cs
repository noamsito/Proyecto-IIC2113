namespace Shin_Megami_Tensei.Managers;

public class SlotSelectionResult
{
    public bool Success { get; private set; }
    public int SelectedSlot { get; private set; }

    private SlotSelectionResult(bool success, int slot = 0)
    {
        Success = success;
        SelectedSlot = slot;
    }

    public static SlotSelectionResult FromSuccess(int slot) => new(true, slot);
    public static SlotSelectionResult Cancelled() => new(false);
}