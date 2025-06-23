using Shin_Megami_Tensei_View;

namespace Shin_Megami_Tensei.Combat;

public class SamuraiActionContext
{
    public Samurai Samurai { get; }
    public Player CurrentPlayer { get; }
    public Player Opponent { get; }
    public View View { get; }

    public int FullStart { get; }
    public int BlinkStart { get; }

    public SamuraiActionContext(Samurai samurai, Player currentPlayer, Player opponent, View view)
    {
        Samurai = samurai;
        CurrentPlayer = currentPlayer;
        Opponent = opponent;
        View = view;

        FullStart = currentPlayer.TurnManager.GetFullTurns();
        BlinkStart = currentPlayer.TurnManager.GetBlinkingTurns();
    }
}