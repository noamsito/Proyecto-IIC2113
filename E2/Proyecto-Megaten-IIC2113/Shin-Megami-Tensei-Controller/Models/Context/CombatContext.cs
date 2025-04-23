using Shin_Megami_Tensei_View;

namespace Shin_Megami_Tensei.Combat;

public class CombatContext
{
    public Player CurrentPlayer { get; }
    public Player Opponent { get; }
    public View View { get; }

    public CombatContext(Player currentPlayer, Player opponent, View view)
    {
        CurrentPlayer = currentPlayer;
        Opponent = opponent;
        View = view;
    }
}