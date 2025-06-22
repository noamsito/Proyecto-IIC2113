using Shin_Megami_Tensei_View;
using Shin_Megami_Tensei;
using Shin_Megami_Tensei.Combat;
using Shin_Megami_Tensei.Controllers;

public static class DemonActionHandler
{
    public static void Handle(Demon demon, CombatContext combatContext, TurnContext turnContext)
    {
        BaseActionHandler.HandleDemonAction(demon, combatContext, turnContext);
    }
}