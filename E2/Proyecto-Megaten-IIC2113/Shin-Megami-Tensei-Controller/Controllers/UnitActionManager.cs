using Shin_Megami_Tensei_View;
using Shin_Megami_Tensei;
using Shin_Megami_Tensei.Combat;
using Shin_Megami_Tensei.Controllers;

public static class UnitActionManager
{
    public static void ExecuteAction(Unit unit, CombatContext combatCtx, TurnContext turnCtx)
    {
        if (unit is Samurai samurai)
            SamuraiActionHandler.Handle(samurai, combatCtx, turnCtx);
        else
            DemonActionHandler.Handle((Demon)unit, combatCtx);
    }
}
