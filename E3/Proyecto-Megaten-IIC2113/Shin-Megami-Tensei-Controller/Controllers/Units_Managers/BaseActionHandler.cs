using Shin_Megami_Tensei_View;
using Shin_Megami_Tensei.Combat;

namespace Shin_Megami_Tensei.Controllers;

/// <summary>
/// Handler base que centraliza la lógica común de manejo de acciones
/// Elimina la duplicación entre SamuraiActionHandler y DemonActionHandler
/// </summary>
public static class BaseActionHandler
{
    public static void HandleUnitAction<T>(
        T unit, 
        CombatContext combatCtx, 
        TurnContext turnCtx,
        ActionMenuProvider.UnitType unitType,
        Func<string, T, CombatContext, TurnContext, bool> executeAction) 
        where T : Unit
    {
        bool actionExecuted = false;

        while (!actionExecuted)
        {
            ActionMenuProvider.DisplayMenu(unitType, unit.GetName(), combatCtx.View);
            
            string input = GetValidInput(unitType, combatCtx.View);
            combatCtx.View.WriteLine(GameConstants.Separator);

            actionExecuted = executeAction(input, unit, combatCtx, turnCtx);
        }
    }

    private static string GetValidInput(ActionMenuProvider.UnitType unitType, View view)
    {
        string input;
        do
        {
            input = view.ReadLine();
            if (!ActionMenuProvider.IsValidInput(unitType, input))
            {
                view.WriteLine("Entrada inválida. Intente nuevamente.");
            }
        } 
        while (!ActionMenuProvider.IsValidInput(unitType, input));

        return input;
    }
}