namespace Shin_Megami_Tensei.Managers;

public static class AttackExecutor
{
    public static double ExecutePhysicalAttack(Unit attacker, Unit target, double modifier)
    {
        int str = attacker.GetBaseStats().GetStatByName("Str");
        double rawDamage = str * modifier * GameConstants.ConstantOfDamage;

        return rawDamage;
    }

    public static double ExecuteGunAttack(Unit attacker, Unit target, double modifier)
    {
        int skl = attacker.GetBaseStats().GetStatByName("Skl");
        double rawDamage = skl * modifier * GameConstants.ConstantOfDamage;

        return rawDamage;
    }
}