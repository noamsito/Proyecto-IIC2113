using Shin_Megami_Tensei;
using Shin_Megami_Tensei.Enums;

public class AttackData
{
    public Unit Attacker { get; }
    public Unit Target { get; }
    public AttackType AttackType { get; }
    public string AttackerName => Attacker.GetName();
    public string TargetName => Target.GetName();

    public AttackData(Unit attacker, Unit target, AttackType attackType)
    {
        Attacker = attacker;
        Target = target;
        AttackType = attackType;
    }
}