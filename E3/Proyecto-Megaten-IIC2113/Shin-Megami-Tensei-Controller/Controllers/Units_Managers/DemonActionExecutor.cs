using Shin_Megami_Tensei_View;
using Shin_Megami_Tensei.Combat;

namespace Shin_Megami_Tensei.Controllers;

public static class DemonActionExecutor
{
    public static bool Execute(string input, Demon demon, CombatContext combatCtx, TurnContext turnCtx)
    {
        return input switch
        {
            "1" => CombatActionExecutor.ExecuteAttack(
                demon, combatCtx.Opponent, combatCtx.View, "Phys", turnCtx),
            
            "2" => CombatActionExecutor.ExecuteSkill(
                demon, combatCtx.CurrentPlayer, combatCtx.Opponent, combatCtx.View, turnCtx),
            
            "3" => CombatActionExecutor.ExecuteSummon(
                combatCtx.CurrentPlayer, demon, turnCtx, isSamurai: false),
            
            "4" => CombatActionExecutor.ExecutePassTurn(turnCtx),
            
            _ => HandleInvalidAction(combatCtx.View)
        };
    }

    private static bool HandleInvalidAction(View view)
    {
        view.WriteLine("Acción inválida.");
        return false;
    }
}