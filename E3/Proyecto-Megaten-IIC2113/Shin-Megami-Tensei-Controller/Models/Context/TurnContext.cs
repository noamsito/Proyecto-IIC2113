namespace Shin_Megami_Tensei.Combat;

public class TurnContext
{
    public int FullStart { get; }
    public int BlinkStart { get; }
    public int FullNow { get; }
    public int BlinkNow { get; }

    public Player Attacker { get; }
    public Player? Defender { get; }

    public TurnContext(Player attacker, Player? defender, int fullStart, int blinkStart)
    {
        Attacker = attacker;
        Defender = defender;
        FullStart = fullStart;
        BlinkStart = blinkStart;
        
        FullNow = attacker.TurnManager.GetFullTurns();
        BlinkNow = attacker.TurnManager.GetBlinkingTurns();
    }

    public int GetFullConsumed() => FullStart - FullNow;
    public int GetBlinkingConsumed() => Math.Max(0, BlinkStart - BlinkNow);
    public int GetBlinkingGained() => Math.Max(0, BlinkNow - BlinkStart);
}