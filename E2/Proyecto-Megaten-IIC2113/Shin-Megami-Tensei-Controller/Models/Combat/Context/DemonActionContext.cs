using Shin_Megami_Tensei_View;

namespace Shin_Megami_Tensei.Combat;

public class DemonActionContext
{
    public Unit Demon { get; }
    public Player CurrentPlayer { get; }
    public Player Opponent { get; }
    public View View { get; }
    public int FullStart { get; }
    public int BlinkStart { get; }

    public DemonActionContext(Unit demon, Player currentPlayer, Player opponent, View view)
    {
        Demon = demon;
        CurrentPlayer = currentPlayer;
        Opponent = opponent;
        View = view;

        FullStart = currentPlayer.GetFullTurns();
        BlinkStart = currentPlayer.GetBlinkingTurns();
    }
}