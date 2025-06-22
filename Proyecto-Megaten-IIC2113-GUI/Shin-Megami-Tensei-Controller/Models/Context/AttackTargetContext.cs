using Shin_Megami_Tensei_View;

namespace Shin_Megami_Tensei.Combat;

public class AttackTargetContext
{
    public Unit Attacker { get; }
    public Player Opponent { get; }
    public string AttackType { get; }

    public AttackTargetContext(Unit attacker, Player opponent, string attackType)
    {
        Attacker = attacker;
        Opponent = opponent;
        AttackType = attackType;
    }
}