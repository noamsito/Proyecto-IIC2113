using Shin_Megami_Tensei;
using Shin_Megami_Tensei.Combat;
using Shin_Megami_Tensei.Gadgets;
using Shin_Megami_Tensei.Managers;

public static class DemonActionExecutor
{
    public static bool Execute(string input, DemonActionContext ctx)
    {
        switch (input)
        {
            case "1":
                return PerformAttack("Phys", ctx);

            case "2":
                ctx.View.WriteLine($"Seleccione una habilidad para que {ctx.Demon.GetName()} use");
                return UseSkill(ctx);

            case "3":
                SummonManager.MonsterSwap(ctx.CurrentPlayer, ctx.Demon, ctx.View);
                TurnManager.UpdateTurnStates(ctx.CurrentPlayer, ctx.Opponent, ctx.FullStart, ctx.BlinkStart, ctx.View);
                return true;

            case "4":
                TurnManager.PassTurn(ctx.CurrentPlayer);
                TurnManager.UpdateTurnStates(ctx.CurrentPlayer, null, ctx.FullStart, ctx.BlinkStart, ctx.View);
                return true;

            default:
                ctx.View.WriteLine("Acción inválida.");
                return false;
        }
    }

    private static bool PerformAttack(string type, DemonActionContext ctx)
    {
        var targetCtx = new AttackTargetContext(ctx.Demon, ctx.Opponent, ctx.View);

        string targetInput = TargetSelector.SelectEnemy(targetCtx);
        int cancelNum = targetCtx.Opponent.GetValidUnits().Count + 1;
        if (targetInput == cancelNum.ToString()) return false;

        Unit target = TargetSelector.ResolveTarget(ctx.Opponent, targetInput);

        CombatUI.DisplayAttack(ctx.Demon.GetName(), target.GetName(), type, ctx.View);

        int damage = type == "Phys"
            ? AttackExecutor.ExecutePhysicalAttack(ctx.Demon, target, GameConstants.ModifierPhysDamage)
            : AttackExecutor.ExecuteGunAttack(ctx.Demon, target, GameConstants.ModifierGunDamage);

        CombatUI.DisplayDamageResult(target, damage, ctx.View);

        TurnManager.ApplyAffinityPenalty(ctx.CurrentPlayer, target, type);
        TurnManager.UpdateTurnStates(ctx.CurrentPlayer, ctx.Opponent, ctx.FullStart, ctx.BlinkStart, ctx.View);

        return true;
    }



    private static bool UseSkill(DemonActionContext ctx)
    {
        Skill? skill = SkillManager.SelectSkill(ctx.View, ctx.Demon);
        if (skill == null) return false;

        var targetCtx = new SkillTargetContext(skill, ctx.CurrentPlayer, ctx.Opponent, ctx.View);
        Unit? target = TargetSelector.SelectSkillTarget(targetCtx);
        if (target == null) return false;

        var useCtx = new SkillUseContext(ctx.Demon, target, skill, ctx.CurrentPlayer, ctx.Opponent, ctx.View);
        SkillManager.ApplySkillEffect(useCtx);

        TurnManager.UpdateTurnStates(ctx.CurrentPlayer, ctx.Opponent, ctx.FullStart, ctx.BlinkStart, ctx.View);
        return true;
    }

}
