using Shin_Megami_Tensei_View;
using Shin_Megami_Tensei.Combat;
using Shin_Megami_Tensei.Gadgets;
using Shin_Megami_Tensei.Managers;
using Shin_Megami_Tensei.String_Handlers;

namespace Shin_Megami_Tensei.Controllers;

public static class SamuraiActionExecutor
{
    public static bool Execute(string input, Samurai samurai, CombatContext combatCtx, TurnContext turnCtx)
    {
        return input switch
        {
            "1" => CombatActionExecutor.ExecuteAttack(
                samurai, combatCtx.Opponent, combatCtx.View, "Phys", turnCtx),
            
            "2" => CombatActionExecutor.ExecuteAttack(
                samurai, combatCtx.Opponent, combatCtx.View, "Gun", turnCtx),
            
            "3" => CombatActionExecutor.ExecuteSkill(
                samurai, combatCtx.CurrentPlayer, combatCtx.Opponent, combatCtx.View, turnCtx),
            
            "4" => CombatActionExecutor.ExecuteSummon(
                combatCtx.CurrentPlayer, samurai, turnCtx, isSamurai: true),
            
            "5" => CombatActionExecutor.ExecutePassTurn(turnCtx),
            
            "6" => HandleSurrender(combatCtx),
            
            _ => false
        };
    }

    private static bool HandleSurrender(CombatContext ctx)
    {
        int playerNumber = GetPlayerNumber(ctx);
        Samurai samurai = ctx.CurrentPlayer.GetTeam().Samurai;
        
        ctx.CurrentPlayer.Surrender();
        ctx.View.WriteLine($"{samurai.GetName()} (J{playerNumber}) se rinde");
        ctx.View.WriteLine(GameConstants.Separator);
        
        return true;
    }

    private static int GetPlayerNumber(CombatContext ctx)
    {
        return ctx.CurrentPlayer.GetName() == "Player 1" ? 1 : 2;
    }
}