namespace Shin_Megami_Tensei.Managers;

public static class AttackExecutor
{
    public static double ExecutePhysicalAttack(Unit attacker, double modifier)
    {
        int str = attacker.GetBaseStats().GetStatByName("Str");
        double rawDamage = str * modifier * GameConstants.CONSTANT_OF_DAMAGE;

        return rawDamage;
    }

    public static double ExecuteGunAttack(Unit attacker, double modifier)
    {
        int skl = attacker.GetBaseStats().GetStatByName("Skl");
        double rawDamage = skl * modifier * GameConstants.CONSTANT_OF_DAMAGE;

        return rawDamage;
    }
}