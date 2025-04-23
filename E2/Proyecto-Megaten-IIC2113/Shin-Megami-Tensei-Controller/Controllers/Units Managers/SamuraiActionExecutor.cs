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
                return ManageUseSkill(ctx, turnCtx);

            case "4":
                bool hasBeenSummoned = SummonManager.SummonFromReserveBySamurai(ctx.CurrentPlayer, ctx.View);

                if (hasBeenSummoned)
                {   
                    SummonManager.ManageTurnsWhenSummoned(turnCtx);
                }
                return hasBeenSummoned;

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

    private static bool PerformAttack(string type, SamuraiActionContext samuraiCtx, TurnContext turnCtx)
    {
        var targetCtx = new AttackTargetContext(samuraiCtx.Samurai, samuraiCtx.Opponent, samuraiCtx.View);

        string targetInput = TargetSelector.SelectEnemy(targetCtx);
        int cancelNum = targetCtx.Opponent.GetValidActiveUnits().Count + 1;
        if (targetInput == cancelNum.ToString()) return false;

        Unit target = TargetSelector.ResolveTarget(samuraiCtx.Opponent, targetInput);

        CombatUI.DisplayAttack(samuraiCtx.Samurai.GetName(), target.GetName(), type);

        int baseDamage = type == "Phys"
            ? AttackExecutor.ExecutePhysicalAttack(samuraiCtx.Samurai, target, GameConstants.ModifierPhysDamage)
            : AttackExecutor.ExecuteGunAttack(samuraiCtx.Samurai, target, GameConstants.ModifierGunDamage);

        var affinityCtx = new AffinityContext(samuraiCtx.Samurai, target, type, baseDamage);
        int finalDamage = AffinityEffectManager.ApplyAffinityEffect(affinityCtx,  turnCtx);

        UnitActionManager.ApplyDamageTaken(target, finalDamage);
        CombatUI.DisplayDamageResult(target, finalDamage);

        TurnManager.ConsumeTurnsBasedOnAffinity(affinityCtx, turnCtx);
        TurnManager.UpdateTurnStates(turnCtx);
        turnCtx.Attacker.ReorderUnitsWhenAttacked();

        return true;
    }


    private static bool ManageUseSkill(SamuraiActionContext samuraiCtx, TurnContext turnCtx)
    {
        Skill? skill = SkillManager.SelectSkill(samuraiCtx.View, samuraiCtx.Samurai);
        if (skill == null) return false;

        var targetCtx = new SkillTargetContext(skill, samuraiCtx.CurrentPlayer, samuraiCtx.Opponent, samuraiCtx.View);
        Player attackerPlayer = turnCtx.Attacker;
        Unit unitAttacking = attackerPlayer.GetTeam().Samurai;
        
        Unit? target = TargetSelector.SelectSkillTarget(targetCtx, unitAttacking);
        if (target == null) return false;

        int baseDamage = skill.Power;

        var affinityCtx = new AffinityContext(samuraiCtx.Samurai, target, skill.Type, baseDamage);
        int finalDamage = AffinityEffectManager.ApplyAffinityEffect(affinityCtx, turnCtx);

        UnitActionManager.ApplyDamageTaken(target, finalDamage);
        CombatUI.DisplayDamageResult(target, finalDamage);

        TurnManager.UpdateTurnStates(turnCtx);
        turnCtx.Attacker.ReorderUnitsWhenAttacked();
        return true;
    }

}
