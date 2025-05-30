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
        switch (unit)
        {
            case Samurai samurai:
                SamuraiActionHandler.Handle(samurai, combatCtx, turnCtx);
                break;
            case Demon demon:
                DemonActionHandler.Handle(demon, combatCtx, turnCtx);
                break;
            default:
                throw new ArgumentException($"Tipo de unidad no soportado: {unit.GetType()}");
        }
    }
    
    public static void ApplyDamageTaken(Unit unitHurt, double damage)
    {
        Stat currentStats = unitHurt.GetCurrentStats();
        
        int currentHP = currentStats.GetStatByName("HP");
        int damageToApply = Convert.ToInt32(Math.Floor(damage));
        int newHP = Math.Max(0, currentHP - damageToApply);
        
        currentStats.SetStatByName("HP", newHP);
    }

    public static void PutInReserveList(Player player, Unit unitDead)
    {
        if (player == null) 
            throw new ArgumentNullException(nameof(player));
        
        if (unitDead == null) 
            throw new ArgumentNullException(nameof(unitDead));

        PlayerUnitManager unitManagerPlayer = player.UnitManager;
        var reservedUnits = unitManagerPlayer.GetReservedUnits();
        
        reservedUnits.Add(unitDead);
    }

    public static void ApplyHealToUnit(Unit target, double amount)
    {
        if (target == null) 
            throw new ArgumentNullException(nameof(target));
        
        if (amount < 0) 
            throw new ArgumentException("La cantidad de curación no puede ser negativa", nameof(amount));

        Stat currentStats = target.GetCurrentStats();
        Stat baseStats = target.GetBaseStats();
        
        int currentHP = currentStats.GetStatByName("HP");
        int maxHP = baseStats.GetStatByName("HP");
        int healAmount = Convert.ToInt32(Math.Floor(amount));

        int newHP = Math.Min(maxHP, currentHP + healAmount);
        
        currentStats.SetStatByName("HP", newHP);
    }
}