using Shin_Megami_Tensei_View;
using Shin_Megami_Tensei.Combat;
using Shin_Megami_Tensei.Gadgets;
using Shin_Megami_Tensei.Managers;
using Shin_Megami_Tensei.String_Handlers;

namespace Shin_Megami_Tensei.Controllers;

public static class SamuraiActionExecutor
{
    public static bool Execute(string input, SamuraiActionContext samuraiCtx, TurnContext turnCtx)
    {
        int playerNumber = GetPlayerNumber(samuraiCtx);
        Team currentTeam = samuraiCtx.CurrentPlayer.GetTeam();
        Samurai currentSamurai = currentTeam.Samurai;

        switch (input)
        {
            case "1":
                return PerformAttack("Phys", samuraiCtx, turnCtx);

            case "2":
                return PerformAttack("Gun", samuraiCtx, turnCtx);

            case "3":
                CombatUI.DisplaySkillSelectionPrompt(samuraiCtx.Samurai.GetName());
                bool usedSkill = ManageUseSkill(samuraiCtx, turnCtx);
                turnCtx.Attacker.IncreaseConstantKPlayer();
                
                return usedSkill;

            case "4":
                return HandleSummon(samuraiCtx, turnCtx);

            case "5":
                TurnManager.ManageTurnsWhenPassedTurn(turnCtx);
                return true;

            case "6":
                HandleSurrender(samuraiCtx, playerNumber, currentSamurai);
                return true;

            default:
                return false;
        }
    }

    private static int GetPlayerNumber(SamuraiActionContext ctx)
    {
        return ctx.CurrentPlayer.GetName() == "Player 1" ? 1 : 2;
    }
    
    private static bool HandleSummon(SamuraiActionContext ctx, TurnContext turnCtx)
    {
        bool hasBeenSummoned = SummonManager.SummonFromReserveBySamurai(ctx.CurrentPlayer);

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


    private static bool PerformAttack(string attackType, SamuraiActionContext samuraiCtx, TurnContext turnCtx)
    {
        Unit? target = SelectAttackTarget(samuraiCtx);
        if (target == null) return false;

        CombatUI.DisplayAttack(samuraiCtx.Samurai.GetName(), target.GetName(), attackType);

        var affinityCtx = ApplyAttackAndGetAffinityContext(samuraiCtx.Samurai, target, attackType);
        AffinityEffectManager.ApplyEffectForBasicAttack(affinityCtx);
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

    private static AffinityContext ApplyAttackAndGetAffinityContext(Unit attacker, Unit target, string attackType)
    {
        double baseDamage = CalculateBaseDamage(attacker, target, attackType);

        var affinityCtx = new AffinityContext(attacker, target, attackType, baseDamage);
        
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
        turnCtx.Attacker.RearrangeSortedUnitsWhenAttacked();
    }

    private static bool ManageUseSkill(SamuraiActionContext samuraiCtx, TurnContext turnCtx)
    {
        Skill? skill = SkillManager.SelectSkill(samuraiCtx.View, samuraiCtx.Samurai);
        if (skill == null) return false;
        
        if (skill.Type == "Special")
        {
            var skillCtx = new SkillUseContext(samuraiCtx.Samurai, null, skill, turnCtx.Attacker, turnCtx.Defender);
            SkillManager.HandleSpecialSkill(skillCtx, turnCtx);
            TurnManager.UpdateTurnStatesForDisplay(turnCtx);
        }
        else if (skill.Type == "Heal")
        {
            Unit? target = null;
            if (skill.Name != "Invitation")
            {
                target = SelectSkillTarget(skill, samuraiCtx, turnCtx);
                if (target == null)
                {
                    return false;
                }
            }
            
            var skillCtx = new SkillUseContext(samuraiCtx.Samurai, target, skill, turnCtx.Attacker, turnCtx.Defender);
            SkillManager.HandleHealSkills(skillCtx, turnCtx);
        }
        else
        {
            Unit? target = SelectSkillTarget(skill, samuraiCtx, turnCtx);
            if (target == null)
            {
                CombatUI.DisplaySeparator();
                return false;
            }

            int numberHits = SkillManager.CalculateNumberHits(skill.Hits, turnCtx.Attacker);
            var skillCtx = new SkillUseContext(samuraiCtx.Samurai, target, skill, turnCtx.Attacker, turnCtx.Defender);

            AffinityEffectManager.ApplyEffectForSkill(skillCtx, turnCtx, numberHits);
            UpdateGameStateAfterSkill(turnCtx);
        }
        
        return true;
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

    private static void UpdateGameStateAfterSkill(TurnContext turnCtx)
    {
        TurnManager.UpdateTurnStatesForDisplay(turnCtx);
        turnCtx.Attacker.RearrangeSortedUnitsWhenAttacked();
    }
}