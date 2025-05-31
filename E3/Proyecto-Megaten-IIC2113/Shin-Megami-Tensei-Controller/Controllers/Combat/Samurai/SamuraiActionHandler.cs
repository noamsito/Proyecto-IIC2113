using Shin_Megami_Tensei;
using Shin_Megami_Tensei.Combat;
using Shin_Megami_Tensei.Controllers;

public static class SamuraiActionHandler
{
    public static void Handle(Samurai samurai, CombatContext combatContext, TurnContext turnContext)
    {
        BaseActionHandler.HandleSamuraiAction(samurai, combatContext, turnContext);
    }
}