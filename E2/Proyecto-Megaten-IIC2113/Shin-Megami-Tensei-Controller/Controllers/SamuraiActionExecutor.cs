using Shin_Megami_Tensei_View;
using Shin_Megami_Tensei.Combat;
using Shin_Megami_Tensei.Gadgets;
using Shin_Megami_Tensei.Managers;
using Shin_Megami_Tensei.String_Handlers;

namespace Shin_Megami_Tensei.Controllers;

public static class SamuraiActionExecutor
{
    public static bool Execute(string input, SamuraiActionContext ctx)
    {
        switch (input)
        {
            case "1":
                return PerformAttack("Phys", ctx);

            case "2":
                return PerformAttack("Gun", ctx);

            case "3":
                ctx.View.WriteLine($"Seleccione una habilidad para que {ctx.Samurai.GetName()} use");
                return UseSkill(ctx);

            case "4":
                SummonManager.InvokeFromReserve(ctx.Samurai, ctx.CurrentPlayer, ctx.View);
                TurnManager.UpdateTurnStates(ctx.CurrentPlayer, ctx.Opponent, ctx.FullStart, ctx.BlinkStart, ctx.View);
                return true;

            case "5":
                TurnManager.PassTurn(ctx.CurrentPlayer);
                TurnManager.UpdateTurnStates(ctx.CurrentPlayer, null, ctx.FullStart, ctx.BlinkStart, ctx.View);
                return true;

            case "6":
                ctx.CurrentPlayer.Surrender();
                int playerNumber = ctx.CurrentPlayer.GetName() == "Player 1" ? 1 : 2;
                ctx.View.WriteLine($"{ctx.CurrentPlayer.GetTeam().Samurai.GetName()} (J{playerNumber}) se rinde");
                ctx.View.WriteLine(GameConstants.Separator);
                return true;

            default:
                ctx.View.WriteLine("Acción inválida.");
                return false;
        }
    }

    private static bool PerformAttack(string type, SamuraiActionContext ctx)
    {
        var targetCtx = new AttackTargetContext(ctx.Samurai, ctx.Opponent, ctx.View);

        string targetInput = TargetSelector.SelectEnemy(targetCtx);
        int cancelNum = targetCtx.Opponent.GetValidUnits().Count + 1;
        if (targetInput == cancelNum.ToString()) return false;

        Unit target = TargetSelector.ResolveTarget(ctx.Opponent, targetInput);

        CombatUI.DisplayAttack(ctx.Samurai.GetName(), target.GetName(), type, ctx.View);

        int damage = type == "Phys"
            ? AttackExecutor.ExecutePhysicalAttack(ctx.Samurai, target, GameConstants.ModifierPhysDamage)
            : AttackExecutor.ExecuteGunAttack(ctx.Samurai, target, GameConstants.ModifierGunDamage);

        CombatUI.DisplayDamageResult(target, damage, ctx.View);

        TurnManager.ApplyAffinityPenalty(ctx.CurrentPlayer, target, type);
        TurnManager.UpdateTurnStates(ctx.CurrentPlayer, ctx.Opponent, ctx.FullStart, ctx.BlinkStart, ctx.View);

        return true;
    }


    private static bool UseSkill(SamuraiActionContext ctx)
    {
        Skill? skill = SkillManager.SelectSkill(ctx.View, ctx.Samurai);
        if (skill == null) return false;

        var targetCtx = new SkillTargetContext(skill, ctx.CurrentPlayer, ctx.Opponent, ctx.View);
        Unit? target = TargetSelector.SelectSkillTarget(targetCtx);
        if (target == null) return false;

        var useCtx = new SkillUseContext(ctx.Samurai, target, skill, ctx.CurrentPlayer, ctx.Opponent, ctx.View);
        SkillManager.ApplySkillEffect(useCtx);

        TurnManager.UpdateTurnStates(ctx.CurrentPlayer, ctx.Opponent, ctx.FullStart, ctx.BlinkStart, ctx.View);
        return true;
    }

}
