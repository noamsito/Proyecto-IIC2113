using Shin_Megami_Tensei_View;
using Shin_Megami_Tensei.Enums;

namespace Shin_Megami_Tensei.Combat;

public class AttackTargetContext
{
    public Unit Attacker { get; }
    public Player Opponent { get; }
    public AttackType AttackType { get; }

    public AttackTargetContext(Unit attacker, Player opponent, AttackType attackType)
    {
        Attacker = attacker;
        Opponent = opponent;
        AttackType = attackType;
    }
}