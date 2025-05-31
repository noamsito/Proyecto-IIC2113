using Shin_Megami_Tensei_View;
using Shin_Megami_Tensei.Combat;
using Shin_Megami_Tensei.Gadgets;
using Shin_Megami_Tensei.String_Handlers;

namespace Shin_Megami_Tensei.Managers;

public static class TurnManager
{
    public static void PrepareNewRound(Player player, int playerNumber)
    {
        Samurai samurai = player.GetTeam().Samurai;
        PlayerTurnManager turnManagerPlayer = player.TurnManager;
        PlayerUnitManager unitManagerPlayer = player.UnitManager;
        
        turnManagerPlayer.SetTurns();
        unitManagerPlayer.SetOrderOfAttackOfActiveUnits();
        CombatUI.DisplayRoundPlayer(samurai, playerNumber);
    }

    public static Unit? GetCurrentUnit(Player player)
    {
        PlayerUnitManager unitManagerPlayer = player.UnitManager;

        var sortedUnits = unitManagerPlayer.GetSortedActiveUnitsByOrderOfAttack();
        return sortedUnits.FirstOrDefault(); 
    }
    
    public static void UpdateTurnStatesForDisplay(TurnContext ctx)
    {
        PlayerTurnManager turnManagerPlayer = ctx.Attacker.TurnManager;
        PlayerUnitManager unitManagerOpponent = ctx.Defender.UnitManager;
        PlayerTeamManager teamManagerOpponent = ctx.Defender.TeamManager;
        
        int fullNow = turnManagerPlayer.GetFullTurns();
        int blinkNow = turnManagerPlayer.GetBlinkingTurns();

        int fullConsumed = ctx.FullStart - fullNow;
        int blinkingConsumed = Math.Max(0, ctx.BlinkStart - blinkNow);
        int blinkingGained = Math.Max(0, blinkNow - ctx.BlinkStart);

        CombatUI.DisplayTurnChanges(fullConsumed, blinkingConsumed, blinkingGained);

        unitManagerOpponent.RemoveFromActiveUnitsIfDead();
        teamManagerOpponent.ReorderReserveBasedOnJsonOrder();
    }
    
    public static void UpdateTurnsForInvocationSkill(TurnContext turnCtx)
    {
        PlayerTurnManager turnManagerPlayer = turnCtx.Attacker.TurnManager;
        
        if (turnManagerPlayer.GetBlinkingTurns() > 0)
        {
            turnManagerPlayer.ConsumeBlinkingTurn(1);
        }
        else
        {
            turnManagerPlayer.ConsumeFullTurn(1);
        }
    }

    public static void ConsumeTurnsWhenPassedTurn(TurnContext turnCtx)
    {
        PlayerTurnManager turnManagerPlayer = turnCtx.Attacker.TurnManager;
        
        if (turnManagerPlayer.GetBlinkingTurns() > 0)
        {
            turnManagerPlayer.ConsumeBlinkingTurn(1);
        }
        else
        {
            turnManagerPlayer.ConsumeFullTurn(1);
            turnManagerPlayer.GainBlinkingTurn(1);
        }
    }
    
    public static void ConsumeTurn(TurnContext turnCtx)
    {
        int fullTurnsToConsume = 1;
        Player playerAttacker = turnCtx.Attacker;
        PlayerTurnManager turnManagerPlayer = playerAttacker.TurnManager;
        
        if (turnManagerPlayer.GetBlinkingTurns() > 0)
        {
            turnManagerPlayer.ConsumeBlinkingTurn(fullTurnsToConsume);
        }
        else if (turnManagerPlayer.GetFullTurns() > 0)
        {
            turnManagerPlayer.ConsumeFullTurn(fullTurnsToConsume);
        }
    }

    
    public static void ManageTurnsWhenPassedTurn(TurnContext turnCtx)
    {
        PlayerUnitManager unitManagerPlayer = turnCtx.Attacker.UnitManager;
        
        ConsumeTurnsWhenPassedTurn(turnCtx);
        UpdateTurnStatesForDisplay(turnCtx);
        unitManagerPlayer.RearrangeSortedUnitsWhenAttacked();
    }
    
    public static void ConsumeAllTurns(Player player)
    {
        PlayerTurnManager turnManagerPlayer = player.TurnManager;
        
        turnManagerPlayer.ConsumeFullTurn(turnManagerPlayer.GetFullTurns());
        turnManagerPlayer.ConsumeBlinkingTurn(turnManagerPlayer.GetBlinkingTurns());
    }

    public static void ConsumeTurnsBasedOnAffinity(AffinityContext affinityCtx, TurnContext turnCtx)
    {
        Player attackingPlayer = turnCtx.Attacker;
        PlayerTurnManager turnManagerPlayer = attackingPlayer.TurnManager;
        
        string affinity = AffinityResolver.GetAffinity(affinityCtx.Target, affinityCtx.AttackType);

        switch (affinity)
        {
            case "Rp":
                ConsumeAllTurns(attackingPlayer);
                break;
            
            case "Dr":
                ConsumeAllTurns(attackingPlayer);
                break;

            case "Nu":
                if (turnManagerPlayer.GetBlinkingTurns() >= 2)
                {
                    turnManagerPlayer.ConsumeBlinkingTurn(2);
                }
                else
                {
                    int blink = turnManagerPlayer.GetBlinkingTurns();
                    turnManagerPlayer.ConsumeBlinkingTurn(blink);
                    turnManagerPlayer.ConsumeFullTurn(2 - blink);
                }
                break;

            case "Miss":
                if (turnManagerPlayer.GetBlinkingTurns() >= 1)
                    turnManagerPlayer.ConsumeBlinkingTurn(1);
                else
                    turnManagerPlayer.ConsumeFullTurn(1);
                break;

            case "Wk":
                if (turnManagerPlayer.GetFullTurns() > 0)
                {
                    turnManagerPlayer.ConsumeFullTurn(1);
                    turnManagerPlayer.GainBlinkingTurn(1);
                }
                else
                {
                    turnManagerPlayer.ConsumeBlinkingTurn(1);
                }
                
                break;
            
            case "Rs":
            case "-":
                if (turnManagerPlayer.GetBlinkingTurns() > 0)
                {
                    turnManagerPlayer.ConsumeBlinkingTurn(1);
                }
                else
                {
                    turnManagerPlayer.ConsumeFullTurn(1);
                }
                break;
        }
    }

    public static List<Unit> GetTargetsForMultiTargetSkill(SkillUseContext skillCtx)
    {
        List<Unit> targets = new List<Unit>();
        
        ManageIfTargetsAreAlliesOrEnemies(skillCtx, ref targets);
        
        return targets;
    }

    private static void ManageIfTargetsAreAlliesOrEnemies(SkillUseContext skillCtx, ref List<Unit> targets)
    {
        Skill skill = skillCtx.Skill;
        bool isHealSkill = skill.Type == "Heal";
        
        switch (skill.Target)
        {
            case "Party":
                if (isHealSkill)
                    AddAllAlliesUnitsToTargets(skillCtx, ref targets);
                break;
            case "All":
                if (isHealSkill)
                {
                    AddAllAlliesUnitsToTargets(skillCtx, ref targets);
                }
                else
                {
                    AddAllEnemiesUnitsToTargets(skillCtx, ref targets);
                }
                break;
        }

    }
    
    private static void AddAllAlliesUnitsToTargets(SkillUseContext skillCtx, ref List<Unit> targets)
    {
        PlayerUnitManager playerUnitManaer = skillCtx.Attacker.UnitManager;
        Unit caster = skillCtx.Caster;
        Skill skill = skillCtx.Skill;
        
        foreach (var unit in playerUnitManaer.GetActiveUnits())
        {
            if (unit != null && unit != caster)
            {
                targets.Add(unit);
            }
        }
        
        foreach (var unit in playerUnitManaer.GetReservedUnits())
        {
            if (unit != null)
            {
                targets.Add(unit);
            }
        }
        
        
        if (!GameConstants._healsThatDontApplyToCaster.Contains(skill.Name)) targets.Add(caster);
    }
    
    private static void AddAllEnemiesUnitsToTargets(SkillUseContext skillCtx, ref List<Unit> targets)
    {
        Player defenderPlayer = skillCtx.Defender;
        PlayerUnitManager unitManagerDefender = defenderPlayer.UnitManager;
        
        foreach (var unit in unitManagerDefender.GetActiveUnits())
        {
            if (unit != null && unit.IsAlive())
            {
                targets.Add(unit);
            }
        }
        
        foreach (var unit in unitManagerDefender.GetReservedUnits())
        {
            if (unit != null && unit.IsAlive())
            {
                targets.Add(unit);
            }
        }
    }
}
