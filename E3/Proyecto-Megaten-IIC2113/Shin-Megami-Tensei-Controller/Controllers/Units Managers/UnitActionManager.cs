using Shin_Megami_Tensei_View;
using Shin_Megami_Tensei;
using Shin_Megami_Tensei.Combat;
using Shin_Megami_Tensei.Controllers;
using Shin_Megami_Tensei.Gadgets;
using Shin_Megami_Tensei.Managers;

public static class UnitActionManager
{
    public static void ExecuteAction(Unit unit, CombatContext combatCtx, TurnContext turnCtx)
    {
        if (unit is Samurai samurai)
            SamuraiActionHandler.Handle(samurai, combatCtx, turnCtx);
        else
            DemonActionHandler.Handle((Demon)unit, combatCtx);
    }
    
    public static int ApplyDamageTaken(Unit unitHurt, double damage)
    {
        Stat currentStats = unitHurt.GetCurrentStats();
        
        int currentHP = currentStats.GetStatByName("HP");
        int newHP = Math.Max(0, currentHP - Convert.ToInt32(Math.Floor(damage)));
        currentStats.SetStatByName("HP", newHP);
        
        return newHP;
    }

    public static void PutInReserveList(Player player, Unit unitDead)
    {
        PlayerUnitManager unitManagerPlayer = player.UnitManager;
        var reservedUnits = unitManagerPlayer.GetReservedUnits();
        
        reservedUnits.Add(unitDead);
    }

    public static void Heal(Unit target, double amount)
    {
        Stat currentStats = target.GetCurrentStats();
        Stat baseStats = target.GetBaseStats();
        
        int currentHP = currentStats.GetStatByName("HP");
        int baseHP = baseStats.GetStatByName("HP");

        int healHP = Math.Max(0, currentHP + Convert.ToInt32(Math.Floor(amount)));
        
        int newHP = Math.Min(baseHP, healHP);
        
        currentStats.SetStatByName("HP", newHP);
    }
}
