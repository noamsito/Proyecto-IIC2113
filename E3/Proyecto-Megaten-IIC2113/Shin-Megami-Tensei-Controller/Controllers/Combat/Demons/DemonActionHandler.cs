using Shin_Megami_Tensei_View;
using Shin_Megami_Tensei;
using Shin_Megami_Tensei.Combat;
using Shin_Megami_Tensei.Controllers;

public static class DemonActionHandler
{
    public static void Handle(Demon demon, CombatContext ctx, TurnContext turnCtx)
    {
        BaseActionHandler.HandleUnitAction(
            demon, 
            ctx, 
            turnCtx,
            ActionMenuProvider.UnitType.Demon,
            (input, unit, combatCtx, turnContext) => 
                DemonActionExecutor.Execute(input, unit, combatCtx, turnContext)
        );
    }
}