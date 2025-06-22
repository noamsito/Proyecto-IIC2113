using Shin_Megami_Tensei;

public class TargetSelectionContext
{
    public Player SourcePlayer { get; }
    public Player TargetPlayer { get; }
    public string SelectionPrompt { get; }

    public TargetSelectionContext(Player sourcePlayer, Player targetPlayer, string selectionPrompt)
    {
        SourcePlayer = sourcePlayer ?? throw new ArgumentNullException(nameof(sourcePlayer));
        TargetPlayer = targetPlayer ?? throw new ArgumentNullException(nameof(targetPlayer));
        SelectionPrompt = selectionPrompt ?? throw new ArgumentNullException(nameof(selectionPrompt));
    }
}