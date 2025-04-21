using Shin_Megami_Tensei_View;

namespace Shin_Megami_Tensei.Combat;

public class ActionContext
{
    public Samurai Samurai { get; }
    public Player CurrentPlayer { get; }
    public Player Opponent { get; }
    public View View { get; }

    public int FullStart { get; }
    public int BlinkStart { get; }

    public ActionContext(Samurai samurai, Player currentPlayer, Player opponent, View view)
    {
        Samurai = samurai;
        CurrentPlayer = currentPlayer;
        Opponent = opponent;
        View = view;

        FullStart = currentPlayer.GetFullTurns();
        BlinkStart = currentPlayer.GetBlinkingTurns();
    }
}