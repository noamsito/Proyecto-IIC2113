namespace Shin_Megami_Tensei.Managers;

public static class AttackExecutor
{
    public static int ExecutePhysicalAttack(Unit attacker, Unit target, double modifier)
    {
        int str = attacker.GetBaseStats().GetStatByName("Str");
        double rawDamage = str * modifier * GameConstants.ConstantOfDamage;
        int damage = (int)Math.Floor(rawDamage);

        target.ApplyDamageTaken(damage);
        return damage;
    }

    public static int ExecuteGunAttack(Unit attacker, Unit target, double modifier)
    {
        int skl = attacker.GetBaseStats().GetStatByName("Skl");
        double rawDamage = skl * modifier * GameConstants.ConstantOfDamage;
        int damage = (int)Math.Floor(rawDamage);

        target.ApplyDamageTaken(damage);
        return damage;
    }
}