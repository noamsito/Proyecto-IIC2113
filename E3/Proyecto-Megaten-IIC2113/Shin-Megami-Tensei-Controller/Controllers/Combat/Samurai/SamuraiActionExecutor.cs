using Shin_Megami_Tensei_View;
using Shin_Megami_Tensei.Combat;
using Shin_Megami_Tensei.Gadgets;
using Shin_Megami_Tensei.Managers;
using Shin_Megami_Tensei.String_Handlers;

namespace Shin_Megami_Tensei.Controllers;

public static class SamuraiActionExecutor
{
    private const string ATTACK_OPTION = "1";
    private const string SHOOT_OPTION = "2";
    private const string SKILL_OPTION = "3";
    private const string SUMMON_OPTION = "4";
    private const string PASS_TURN_OPTION = "5";
    private const string SURRENDER_OPTION = "6";

    public static bool Execute(string playerInput, Samurai samurai, CombatContext combatContext, TurnContext turnContext)
    {
        return playerInput switch
        {
            ATTACK_OPTION => ExecutePhysicalAttack(samurai, combatContext, turnContext),
            SHOOT_OPTION => ExecuteGunAttack(samurai, combatContext, turnContext),
            SKILL_OPTION => ExecuteSkillAction(samurai, combatContext, turnContext),
            SUMMON_OPTION => ExecuteSummonAction(samurai, combatContext, turnContext),
            PASS_TURN_OPTION => ExecutePassTurnAction(turnContext),
            SURRENDER_OPTION => ExecuteSurrenderAction(combatContext),
            _ => false
        };
    }

    private static bool ExecutePhysicalAttack(Samurai samurai, CombatContext combatContext, TurnContext turnContext)
    {
        var attackContext = new AttackTargetContext(samurai, combatContext.Opponent, "Phys");
        return CombatActionExecutor.ExecuteAttack(attackContext, turnContext);
    }

    private static bool ExecuteGunAttack(Samurai samurai, CombatContext combatContext, TurnContext turnContext)
    {
        var attackContext = new AttackTargetContext(samurai, combatContext.Opponent, "Gun");
        return CombatActionExecutor.ExecuteAttack(attackContext, turnContext);
    }

    private static bool ExecuteSkillAction(Samurai samurai, CombatContext combatContext, TurnContext turnContext)
    {
        return CombatActionExecutor.ExecuteSkill(samurai, combatContext, turnContext);
    }

    private static bool ExecuteSummonAction(Samurai samurai, CombatContext combatContext, TurnContext turnContext)
    {
        return CombatActionExecutor.ExecuteSummon(combatContext.CurrentPlayer, samurai, turnContext, SummonType.Samurai);
    }

    private static bool ExecutePassTurnAction(TurnContext turnContext)
    {
        return CombatActionExecutor.ExecutePassTurn(turnContext);
    }

    private static bool ExecuteSurrenderAction(CombatContext combatContext)
    {
        int playerNumber = DeterminePlayerNumber(combatContext);
        string samuraiName = GetCurrentSamuraiName(combatContext);
        
        ProcessSurrenderAction(combatContext);
        DisplaySurrenderMessage(combatContext.View, samuraiName, playerNumber);
        
        return true;
    }

    private static int DeterminePlayerNumber(CombatContext combatContext)
    {
        return combatContext.CurrentPlayer.GetName() == "Player 1" ? 1 : 2;
    }

    private static string GetCurrentSamuraiName(CombatContext combatContext)
    {
        return combatContext.CurrentPlayer.GetTeam().Samurai.GetName();
    }

    private static void ProcessSurrenderAction(CombatContext combatContext)
    {
        combatContext.CurrentPlayer.Surrender();
    }

    private static void DisplaySurrenderMessage(View gameView, string samuraiName, int playerNumber)
    {
        gameView.WriteLine($"{samuraiName} (J{playerNumber}) se rinde");
        gameView.WriteLine(GameConstants.Separator);
    }
}