using Shin_Megami_Tensei;

public class AttackData
{
    public Unit Attacker { get; }
    public Unit Target { get; }
    public string AttackType { get; }
    public string AttackerName => Attacker.GetName();
    public string TargetName => Target.GetName();

    public AttackData(Unit attacker, Unit target, string attackType)
    {
        Attacker = attacker;
        Target = target;
        AttackType = attackType;
    }
}