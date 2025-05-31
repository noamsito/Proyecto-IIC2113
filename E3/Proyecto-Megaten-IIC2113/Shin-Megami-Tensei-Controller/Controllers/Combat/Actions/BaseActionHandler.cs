using Shin_Megami_Tensei_View;
using Shin_Megami_Tensei.Combat;

namespace Shin_Megami_Tensei.Controllers;

public static class BaseActionHandler
{
    public static void HandleSamuraiAction(Samurai samurai, CombatContext combatContext, TurnContext turnContext)
    {
        if (!IsUnitAlive(samurai))
            return;

        bool actionWasExecuted = false;

        while (!actionWasExecuted)
        {
            DisplaySamuraiMenu(samurai.GetName());
            string playerInput = CombatUI.GetUserInputWithSeparator();

            if (IsValidSamuraiInput(playerInput))
            {
                actionWasExecuted = SamuraiActionExecutor.Execute(playerInput, samurai, combatContext, turnContext);
            }
        }
    }

    public static void HandleDemonAction(Demon demon, CombatContext combatContext, TurnContext turnContext)
    {
        if (!IsUnitAlive(demon))
            return;

        bool actionWasExecuted = false;
    
        while (!actionWasExecuted)
        {
            DisplayDemonMenu(demon.GetName());
            string playerInput = CombatUI.GetUserInputWithSeparator();
            
            if (IsValidDemonInput(playerInput))
            {
                actionWasExecuted = DemonActionExecutor.Execute(playerInput, demon, combatContext, turnContext);
            }
            // Si el input es inválido, simplemente continúa el loop sin hacer nada
        }
    }

    private static void DisplaySamuraiMenu(string unitName)
    {
        ActionMenuProvider.DisplaySamuraiMenu(unitName);
    }

    private static void DisplayDemonMenu(string unitName)
    {
        ActionMenuProvider.DisplayDemonMenu(unitName);
    }

    private static bool IsValidSamuraiInput(string input)
    {
        return input is "1" or "2" or "3" or "4" or "5" or "6";
    }

    private static bool IsValidDemonInput(string input)
    {
        return input is "1" or "2" or "3" or "4";
    }

    private static bool IsUnitAlive(Unit unit)
    {
        return unit.GetCurrentStats().GetStatByName("HP") > 0;
    }
}