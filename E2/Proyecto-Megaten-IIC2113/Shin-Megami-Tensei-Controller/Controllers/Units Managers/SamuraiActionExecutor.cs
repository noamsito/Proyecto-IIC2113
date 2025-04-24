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
        int playerNumber = GetPlayerNumber(ctx);
        Team currentTeam = ctx.CurrentPlayer.GetTeam();
        Samurai currentSamurai = currentTeam.Samurai;

        switch (input)
        {
            case "1":
                return PerformAttack("Phys", ctx, turnCtx);

            case "2":
                return PerformAttack("Gun", ctx, turnCtx);

            case "3":
                DisplaySkillSelectionPrompt(ctx);
                return ManageUseSkill(ctx, turnCtx);

            case "4":
                return HandleSummon(ctx, turnCtx);

            case "5":
                TurnManager.ManageTurnsWhenPassedTurn(turnCtx);
                return true;

            case "6":
                HandleSurrender(ctx, playerNumber, currentSamurai);
                return true;

            default:
                DisplayInvalidActionMessage(ctx);
                return false;
        }
    }

    private static int GetPlayerNumber(SamuraiActionContext ctx)
    {
        return ctx.CurrentPlayer.GetName() == "Player 1" ? 1 : 2;
    }

    private static void DisplaySkillSelectionPrompt(SamuraiActionContext ctx)
    {
        ctx.View.WriteLine($"Seleccione una habilidad para que {ctx.Samurai.GetName()} use");
    }

    private static bool HandleSummon(SamuraiActionContext ctx, TurnContext turnCtx)
    {
        bool hasBeenSummoned = SummonManager.SummonFromReserveBySamurai(ctx.CurrentPlayer, ctx.View);

        if (hasBeenSummoned)
        {
            SummonManager.ManageTurnsWhenSummoned(turnCtx);
        }

        return hasBeenSummoned;
    }

    private static void HandleSurrender(SamuraiActionContext ctx, int playerNumber, Samurai samurai)
    {
        ctx.CurrentPlayer.Surrender();

        ctx.View.WriteLine($"{samurai.GetName()} (J{playerNumber}) se rinde");
        ctx.View.WriteLine(GameConstants.Separator);
    }

    private static void DisplayInvalidActionMessage(SamuraiActionContext ctx)
    {
        ctx.View.WriteLine("Acción inválida.");
    }

    private static bool PerformAttack(string attackType, SamuraiActionContext samuraiCtx, TurnContext turnCtx)
    {
        Unit? target = SelectAttackTarget(samuraiCtx);
        if (target == null) return false;

        DisplayAttackInformation(samuraiCtx.Samurai, target, attackType);

        var affinityCtx = ApplyAttackAndGetAffinityContext(samuraiCtx.Samurai, target, attackType, turnCtx);

        UpdateGameStateAfterAttack(affinityCtx, turnCtx);

        return true;
    }

    private static Unit? SelectAttackTarget(SamuraiActionContext samuraiCtx)
    {
        var targetCtx = new AttackTargetContext(samuraiCtx.Samurai, samuraiCtx.Opponent, samuraiCtx.View);

        string targetInput = TargetSelector.SelectEnemy(targetCtx);
        int cancelNum = targetCtx.Opponent.GetValidActiveUnits().Count + 1;

        if (targetInput == cancelNum.ToString())
            return null;

        return TargetSelector.ResolveTarget(samuraiCtx.Opponent, targetInput);
    }

    private static void DisplayAttackInformation(Unit attacker, Unit target, string attackType)
    {
        CombatUI.DisplayAttack(attacker.GetName(), target.GetName(), attackType);
    }

    private static AffinityContext ApplyAttackAndGetAffinityContext(Unit attacker, Unit target, string attackType,
        TurnContext turnCtx)
    {
        int baseDamage = CalculateBaseDamage(attacker, target, attackType);

        var affinityCtx = new AffinityContext(attacker, target, attackType, baseDamage);
        int finalDamage = AffinityEffectManager.ApplyAffinityEffect(affinityCtx, turnCtx);

        ApplyDamageAndDisplayResult(target, finalDamage);

        return affinityCtx;
    }

    private static int CalculateBaseDamage(Unit attacker, Unit target, string attackType)
    {
        return attackType == "Phys"
            ? AttackExecutor.ExecutePhysicalAttack(attacker, target, GameConstants.ModifierPhysDamage)
            : AttackExecutor.ExecuteGunAttack(attacker, target, GameConstants.ModifierGunDamage);
    }

    private static void ApplyDamageAndDisplayResult(Unit target, int damage)
    {
        UnitActionManager.ApplyDamageTaken(target, damage);
        CombatUI.DisplayDamageResult(target, damage);
    }

    private static void UpdateGameStateAfterAttack(AffinityContext affinityCtx, TurnContext turnCtx)
    {
        TurnManager.ConsumeTurnsBasedOnAffinity(affinityCtx, turnCtx);
        TurnManager.UpdateTurnStates(turnCtx);
        turnCtx.Attacker.ReorderUnitsWhenAttacked();
    }

    private static bool ManageUseSkill(SamuraiActionContext samuraiCtx, TurnContext turnCtx)
    {
        Skill? skill = SelectSkill(samuraiCtx);
        if (skill == null) return false;

        Unit? target = SelectSkillTarget(skill, samuraiCtx, turnCtx);
        if (target == null) return false;

        ApplySkillEffectAndUpdateState(samuraiCtx.Samurai, target, skill, turnCtx);

        return true;
    }

    private static Skill? SelectSkill(SamuraiActionContext samuraiCtx)
    {
        return SkillManager.SelectSkill(samuraiCtx.View, samuraiCtx.Samurai);
    }

    private static Unit? SelectSkillTarget(Skill skill, SamuraiActionContext samuraiCtx, TurnContext turnCtx)
    {
        var targetCtx = new SkillTargetContext(
            skill,
            samuraiCtx.CurrentPlayer,
            samuraiCtx.Opponent,
            samuraiCtx.View
        );

        Player attackerPlayer = turnCtx.Attacker;
        Unit unitAttacking = attackerPlayer.GetTeam().Samurai;

        return TargetSelector.SelectSkillTarget(targetCtx, unitAttacking);
    }

    private static void ApplySkillEffectAndUpdateState(Unit caster, Unit target, Skill skill, TurnContext turnCtx)
    {
        int baseDamage = skill.Power;
        var affinityCtx = new AffinityContext(caster, target, skill.Type, baseDamage);

        int finalDamage = CalculateSkillDamage(affinityCtx, turnCtx);

        ApplyDamageAndDisplayResult(target, finalDamage);

        UpdateGameStateAfterSkill(turnCtx);
    }

    private static int CalculateSkillDamage(AffinityContext affinityCtx, TurnContext turnCtx)
    {
        return AffinityEffectManager.ApplyAffinityEffect(affinityCtx, turnCtx);
    }

    private static void UpdateGameStateAfterSkill(TurnContext turnCtx)
    {
        TurnManager.UpdateTurnStates(turnCtx);
        turnCtx.Attacker.ReorderUnitsWhenAttacked();
    }
}