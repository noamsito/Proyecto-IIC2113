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
                CombatUI.DisplaySkillSelectionPrompt(ctx.Demon.GetName());
                return ManageUseSkill(ctx, turnCtx);

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

    private static bool PerformAttack(string attackType, DemonActionContext demonCtx, TurnContext turnCtx)
    {
        Unit? target = SelectAttackTarget(demonCtx);
        if (target == null) return false;

        DisplayAttackInformation(demonCtx.Demon, target, attackType);

        var affinityCtx = ApplyAttackAndGetAffinityContext(demonCtx.Demon, target, attackType);
        UpdateGameStateAfterAttack(affinityCtx, turnCtx);

        return true;
    }

    private static Unit? SelectAttackTarget(DemonActionContext demonCtx)
    {
        var targetCtx = new AttackTargetContext(demonCtx.Demon, demonCtx.Opponent, demonCtx.View);

        string targetInput = TargetSelector.SelectEnemy(targetCtx);
        int cancelNum = targetCtx.Opponent.GetValidActiveUnits().Count + 1;

        if (targetInput == cancelNum.ToString())
            return null;

        return TargetSelector.ResolveTarget(demonCtx.Opponent, targetInput);
    }

    private static void DisplayAttackInformation(Unit attacker, Unit target, string attackType)
    {
        CombatUI.DisplayAttack(attacker.GetName(), target.GetName(), attackType);
    }

    private static AffinityContext ApplyAttackAndGetAffinityContext(Unit attacker, Unit target, string attackType)
    {
        double baseDamage = CalculateBaseDamage(attacker, target, attackType);

        var affinityCtx = new AffinityContext(attacker, target, attackType, baseDamage);
        AffinityEffectManager.ApplyEffectForBasicAttack(affinityCtx);

        return affinityCtx;
    }

    private static double CalculateBaseDamage(Unit attacker, Unit target, string attackType)
    {
        return attackType == "Phys"
            ? AttackExecutor.ExecutePhysicalAttack(attacker, target, GameConstants.ModifierPhysDamage)
            : AttackExecutor.ExecuteGunAttack(attacker, target, GameConstants.ModifierGunDamage);
    }

    private static void UpdateGameStateAfterAttack(AffinityContext affinityCtx, TurnContext turnCtx)
    {
        TurnManager.ConsumeTurnsBasedOnAffinity(affinityCtx, turnCtx);
        TurnManager.UpdateTurnStatesForDisplay(turnCtx);
        turnCtx.Attacker.ReorderUnitsWhenAttacked();
    }

    private static bool ManageUseSkill(DemonActionContext demonCtx, TurnContext turnCtx)
    {
        Skill? skill = SkillManager.SelectSkill(demonCtx.View, demonCtx.Demon);
        if (skill == null) return false;

        Unit? target = SelectSkillTarget(skill, demonCtx);
        if (target == null) return false;

        ApplySkillEffect(demonCtx.Demon, target, skill, turnCtx);
        UpdateGameStateAfterSkill(turnCtx);
        
        return true;
    }

    private static Unit? SelectSkillTarget(Skill skill, DemonActionContext demonCtx)
    {
        var targetCtx = new SkillTargetContext(
            skill,
            demonCtx.CurrentPlayer,
            demonCtx.Opponent,
            demonCtx.View
        );

        return TargetSelector.SelectSkillTarget(targetCtx, demonCtx.Demon);
    }
    
    private static void ApplySkillEffect(Unit caster, Unit target, Skill skill, TurnContext turnCtx)
    {
        var skillCtx = new SkillUseContext(caster, target, skill, turnCtx.Attacker, turnCtx.Defender!);
        AffinityEffectManager.ApplyEffectForSkill(skillCtx, turnCtx);

    }
    
    private static void UpdateGameStateAfterSkill(TurnContext turnCtx)
    {
        TurnManager.UpdateTurnStatesForDisplay(turnCtx);
        turnCtx.Attacker.ReorderUnitsWhenAttacked();
    }
}