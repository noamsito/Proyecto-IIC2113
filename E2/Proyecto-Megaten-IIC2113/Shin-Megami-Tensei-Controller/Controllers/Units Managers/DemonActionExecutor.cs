using Shin_Megami_Tensei;
using Shin_Megami_Tensei.Combat;
using Shin_Megami_Tensei.Gadgets;
using Shin_Megami_Tensei.Managers;

public static class DemonActionExecutor
{
    public static bool Execute(string input, DemonActionContext ctx)
    {
        var turnCtx = new TurnContext(ctx.CurrentPlayer, ctx.Opponent, ctx.FullStart, ctx.BlinkStart);
        switch (input)
        {
            case "1":
                return PerformAttack("Phys", ctx, turnCtx);

            case "2":
                ctx.View.WriteLine($"Seleccione una habilidad para que {ctx.Demon.GetName()} use");
                return UseSkill(ctx, turnCtx);

            case "3":
                SummonManager.MonsterSwap(ctx.CurrentPlayer, ctx.Demon, ctx.View);
                SummonManager.ManageTurnsWhenSummoned(turnCtx);
                return true;

            case "4":
                TurnManager.ManageTurnsWhenPassedTurn(turnCtx);
                return true;

            default:
                ctx.View.WriteLine("Acción inválida.");
                return false;
        }
    }

    private static bool PerformAttack(string type, DemonActionContext demonCtx, TurnContext turnCtx)
    {
        var targetCtx = new AttackTargetContext(demonCtx.Demon, demonCtx.Opponent, demonCtx.View);

        string targetInput = TargetSelector.SelectEnemy(targetCtx);
        int cancelNum = targetCtx.Opponent.GetValidActiveUnits().Count + 1;
        if (targetInput == cancelNum.ToString()) return false;

        Unit target = TargetSelector.ResolveTarget(demonCtx.Opponent, targetInput);

        CombatUI.DisplayAttack(demonCtx.Demon.GetName(), target.GetName(), type);

        int baseDamage = type == "Phys"
            ? AttackExecutor.ExecutePhysicalAttack(demonCtx.Demon, target, GameConstants.ModifierPhysDamage)
            : AttackExecutor.ExecuteGunAttack(demonCtx.Demon, target, GameConstants.ModifierGunDamage);

        var affinityCtx = new AffinityContext(demonCtx.Demon, target, type, baseDamage);
        int finalDamage = AffinityEffectManager.ApplyAffinityEffect(affinityCtx, turnCtx);

        UnitActionManager.ApplyDamageTaken(target, finalDamage);
        CombatUI.DisplayDamageResult(target, finalDamage);
        
        TurnManager.ApplyAffinityPenalty(affinityCtx, turnCtx);
        TurnManager.UpdateTurnStates(turnCtx);
        turnCtx.Attacker.ReorderUnitsWhenAttacked();

        return true;
    }

    private static bool UseSkill(DemonActionContext demonCtx, TurnContext turnCtx)
    {
        Skill? skill = SkillManager.SelectSkill(demonCtx.View, demonCtx.Demon);
        if (skill == null) return false;

        var targetCtx = new SkillTargetContext(skill, demonCtx.CurrentPlayer, demonCtx.Opponent, demonCtx.View);
        Unit? target = TargetSelector.SelectSkillTarget(targetCtx, demonCtx.Demon);
        if (target == null) return false;

        CombatUI.DisplaySkillUsage(demonCtx.Demon, skill, target);

        int baseDamage = skill.Power;
        var affinityCtx = new AffinityContext(demonCtx.Demon, target, skill.Type, baseDamage);
        int finalDamage = AffinityEffectManager.ApplyAffinityEffect(affinityCtx,  turnCtx);

        UnitActionManager.ApplyDamageTaken(target, finalDamage);
        CombatUI.DisplayDamageResult(target, finalDamage);

        TurnManager.UpdateTurnStates(turnCtx);
        turnCtx.Attacker.ReorderUnitsWhenAttacked();

        return true;
    }

}
