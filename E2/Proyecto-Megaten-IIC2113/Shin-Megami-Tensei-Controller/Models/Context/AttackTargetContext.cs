using Shin_Megami_Tensei_View;

namespace Shin_Megami_Tensei.Combat;

public class AttackTargetContext
{
    public Unit Attacker { get; }
    public Player Opponent { get; }
    public View View { get; }

    public AttackTargetContext(Unit attacker, Player opponent, View view)
    {
        Attacker = attacker;
        Opponent = opponent;
        View = view;
    }
}