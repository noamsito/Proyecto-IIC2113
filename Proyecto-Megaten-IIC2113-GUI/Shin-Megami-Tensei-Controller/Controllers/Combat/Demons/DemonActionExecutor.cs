using Shin_Megami_Tensei_View;
using Shin_Megami_Tensei.Combat;

namespace Shin_Megami_Tensei.Controllers;

public static class DemonActionExecutor
{
    private const string ATTACK_OPTION = "1";
    private const string SKILL_OPTION = "2";
    private const string SUMMON_OPTION = "3";
    private const string PASS_TURN_OPTION = "4";

    public static bool Execute(string playerInput, Demon demon, CombatContext combatContext, TurnContext turnContext)
    {
        return playerInput switch
        {
            ATTACK_OPTION => ExecutePhysicalAttack(demon, combatContext, turnContext),
            SKILL_OPTION => ExecuteSkillAction(demon, combatContext, turnContext),
            SUMMON_OPTION => ExecuteSummonAction(demon, combatContext, turnContext),
            PASS_TURN_OPTION => ExecutePassTurnAction(turnContext),
            _ => false
        };
    }

    private static bool ExecutePhysicalAttack(Demon demon, CombatContext combatContext, TurnContext turnContext)
    {
        var attackContext = new AttackTargetContext(demon, combatContext.Opponent, "Phys");
        return CombatActionExecutor.ExecuteAttack(attackContext, turnContext);
    }

    private static bool ExecuteSkillAction(Demon demon, CombatContext combatContext, TurnContext turnContext)
    {
        return CombatActionExecutor.ExecuteSkill(demon, combatContext, turnContext);
    }

    private static bool ExecuteSummonAction(Demon demon, CombatContext combatContext, TurnContext turnContext)
    {
        return CombatActionExecutor.ExecuteSummon(combatContext.CurrentPlayer, demon, turnContext, SummonType.Demon);
    }

    private static bool ExecutePassTurnAction(TurnContext turnContext)
    {
        return CombatActionExecutor.ExecutePassTurn(turnContext);
    }
}