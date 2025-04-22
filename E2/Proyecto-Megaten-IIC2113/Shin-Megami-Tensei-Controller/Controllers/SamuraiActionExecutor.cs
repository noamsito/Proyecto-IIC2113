using Shin_Megami_Tensei_View;
using Shin_Megami_Tensei.Combat;
using Shin_Megami_Tensei.Gadgets;
using Shin_Megami_Tensei.Managers;
using Shin_Megami_Tensei.String_Handlers;

namespace Shin_Megami_Tensei.Controllers;

public static class SamuraiActionExecutor
{
    public static bool Execute(string input, SamuraiActionContext ctx, TurnContext turnCtx)
    {
        int playerNumber = ctx.CurrentPlayer.GetName() == "Player 1" ? 1 : 2;
        Team currentTeam = ctx.CurrentPlayer.GetTeam();
        Samurai currentSamurai = currentTeam.Samurai;
        
        switch (input)
        {
            case "1":
                return PerformAttack("Phys", ctx, turnCtx);

            case "2":
                return PerformAttack("Gun", ctx, turnCtx);

            case "3":
                ctx.View.WriteLine($"Seleccione una habilidad para que {ctx.Samurai.GetName()} use");
                return UseSkill(ctx, turnCtx);

            case "4":
                SummonManager.SummonFromReserveBySamurai(ctx.CurrentPlayer, ctx.View);
                SummonManager.ManageTurnsWhenSummoned(turnCtx);
                return true;

            case "5":
                TurnManager.ManageTurnsWhenPassedTurn(turnCtx);
                return true;


            case "6":
                ctx.CurrentPlayer.Surrender();
                
                ctx.View.WriteLine($"{currentSamurai.GetName()} (J{playerNumber}) se rinde");
                ctx.View.WriteLine(GameConstants.Separator);
                return true;

            default:
                ctx.View.WriteLine("Acción inválida.");
                return false;
        }
    }

    private static bool PerformAttack(string type, SamuraiActionContext ctx, TurnContext turnCtx)
    {
        var targetCtx = new AttackTargetContext(ctx.Samurai, ctx.Opponent, ctx.View);

        string targetInput = TargetSelector.SelectEnemy(targetCtx);
        int cancelNum = targetCtx.Opponent.GetValidActiveUnits().Count + 1;
        if (targetInput == cancelNum.ToString()) return false;

        Unit target = TargetSelector.ResolveTarget(ctx.Opponent, targetInput);

        CombatUI.DisplayAttack(ctx.Samurai.GetName(), target.GetName(), type);

        int damage = type == "Phys"
            ? AttackExecutor.ExecutePhysicalAttack(ctx.Samurai, target, GameConstants.ModifierPhysDamage)
            : AttackExecutor.ExecuteGunAttack(ctx.Samurai, target, GameConstants.ModifierGunDamage);

        CombatUI.DisplayDamageResult(target, damage);

        TurnManager.ApplyAffinityPenalty(ctx.CurrentPlayer, target, type);
        TurnManager.UpdateTurnStates(turnCtx);
        turnCtx.Attacker.ReorderUnitsWhenAttacked();

        return true;
    }


    private static bool UseSkill(SamuraiActionContext ctx, TurnContext turnCtx)
    {
        Skill? skill = SkillManager.SelectSkill(ctx.View, ctx.Samurai);
        if (skill == null) return false;

        var targetCtx = new SkillTargetContext(skill, ctx.CurrentPlayer, ctx.Opponent, ctx.View);
        Unit? target = TargetSelector.SelectSkillTarget(targetCtx);
        if (target == null) return false;

        var useCtx = new SkillUseContext(ctx.Samurai, target, skill, ctx.CurrentPlayer, ctx.Opponent, ctx.View);
        SkillManager.ApplySkillEffect(useCtx);

        TurnManager.UpdateTurnStates(turnCtx);
        turnCtx.Attacker.ReorderUnitsWhenAttacked();
        return true;
    }

}
