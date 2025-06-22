using Shin_Megami_Tensei_View;
using Shin_Megami_Tensei;
using Shin_Megami_Tensei.Combat;

public class CombatActionContext
{
    public Unit Attacker { get; }
    public Player CurrentPlayer { get; }
    public Player Opponent { get; }
    public View View { get; }
    public TurnContext TurnContext { get; }

    public CombatActionContext(Unit attacker, Player currentPlayer, Player opponent, View view, TurnContext turnContext)
    {
        Attacker = attacker;
        CurrentPlayer = currentPlayer;
        Opponent = opponent;
        View = view;
        TurnContext = turnContext;
    }
}