using Shin_Megami_Tensei;
using Shin_Megami_Tensei.Combat;
using Shin_Megami_Tensei.Gadgets;
using Shin_Megami_Tensei.Managers;

public static class DemonActionExecutor
{
    public static bool Execute(string input, DemonActionContext demonCtx)
    {
        var turnCtx = new TurnContext(demonCtx.CurrentPlayer, demonCtx.Opponent, demonCtx.FullStart, demonCtx.BlinkStart);
        Player currentPlayer = demonCtx.CurrentPlayer;
        PlayerTurnManager turnManager = currentPlayer.TurnManager;
        
        switch (input)
        {
            case "1":
                return PerformAttack("Phys", demonCtx, turnCtx);

            case "2":
                CombatUI.DisplaySkillSelectionPrompt(demonCtx.Demon.GetName());
                bool usedSkill = ManageUseSkill(demonCtx, turnCtx);
                if (usedSkill) turnManager.IncreaseConstantKPlayer();
                
                return usedSkill;

            case "3":
                bool hasSummoned = SummonManager.MonsterSwap(demonCtx.CurrentPlayer, demonCtx.Demon);
                if (hasSummoned)
                {
                    SummonManager.ManageTurnsWhenSummoned(turnCtx);
                }

                return hasSummoned;

            case "4":
                TurnManager.ManageTurnsWhenPassedTurn(turnCtx);
                return true;

            default:
                demonCtx.View.WriteLine("Acción inválida.");
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
        Player currentPlayer = demonCtx.CurrentPlayer;
        PlayerUnitManager unitManager = currentPlayer.UnitManager;
        
        string targetInput = TargetSelector.SelectEnemy(targetCtx);
        int cancelNum = unitManager.GetValidActiveUnits().Count + 1;

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
        Player currentPlayer = turnCtx.Attacker;
        PlayerUnitManager unitManager = currentPlayer.UnitManager;
        
        TurnManager.ConsumeTurnsBasedOnAffinity(affinityCtx, turnCtx);
        TurnManager.UpdateTurnStatesForDisplay(turnCtx);
        unitManager.RearrangeSortedUnitsWhenAttacked();
    }

    private static bool ManageUseSkill(DemonActionContext demonCtx, TurnContext turnCtx)
    {
        Skill? skill = SkillManager.SelectSkill(demonCtx.View, demonCtx.Demon);
        if (skill == null) return false;

        switch (skill.Type)
        {
            case "Special":
                return HandleSpecialSkill(skill, demonCtx, turnCtx);

            case "Heal":
                return HandleHealSkill(skill, demonCtx, turnCtx);

            default:
                return HandleDamageSkill(skill, demonCtx, turnCtx);
        }
    }

    private static bool HandleSpecialSkill(Skill skill, DemonActionContext demonCtx, TurnContext turnCtx)
    {
        var skillCtx = CreateSkillContext(demonCtx.Demon, null, skill, turnCtx);
        bool skillUsed = SkillManager.HandleSpecialSkill(skillCtx, turnCtx);

        if (skillUsed)
        {
            TurnManager.UpdateTurnStatesForDisplay(turnCtx);
        }

        return skillUsed;
    }

    private static bool HandleHealSkill(Skill skill, DemonActionContext demonCtx, TurnContext turnCtx)
    {
        Unit? target = null;

        if (skill.Name != "Invitation")
        {
            target = SelectSkillTarget(skill, demonCtx);
            if (target == null)
            {
                CombatUI.DisplaySeparator();
                return false;
            }
        }

        var skillCtx = CreateSkillContext(demonCtx.Demon, target, skill, turnCtx);
        return SkillManager.HandleHealSkills(skillCtx, turnCtx);
    }

    private static bool HandleDamageSkill(Skill skill, DemonActionContext demonCtx, TurnContext turnCtx)
    {
        Unit? target = SelectSkillTarget(skill, demonCtx);
        if (target == null)
        {
            CombatUI.DisplaySeparator();
            return false;
        }

        var skillCtx = CreateSkillContext(demonCtx.Demon, target, skill, turnCtx);
        int numberHits = SkillManager.CalculateNumberHits(skill.Hits, turnCtx.Attacker);

        AffinityEffectManager.ApplyEffectForSkill(skillCtx, turnCtx, numberHits);
        UpdateGameStateAfterSkill(turnCtx);

        return true;
    }

    private static SkillUseContext CreateSkillContext(Unit caster, Unit? target, Skill skill, TurnContext turnCtx)
    {
        return new SkillUseContext(caster, target, skill, turnCtx.Attacker, turnCtx.Defender);
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
    
    private static void UpdateGameStateAfterSkill(TurnContext turnCtx)
    {
        TurnManager.UpdateTurnStatesForDisplay(turnCtx);
        turnCtx.Attacker.RearrangeSortedUnitsWhenAttacked();
    }
}