using Shin_Megami_Tensei_View;
using Shin_Megami_Tensei.Combat;
using Shin_Megami_Tensei.String_Handlers;

namespace Shin_Megami_Tensei.Managers;

public static class TurnManager
{
    public static void SetInitialTurns(IEnumerable<Player> players)
    {
        foreach (var player in players)
        {
            player.SetTurns();
        }
    }

    public static void PrepareNewRound(Player player, View view, int playerNumber)
    {
        player.SetTurns();
        player.SetOrderOfAttackOfActiveUnits();
        view.WriteLine($"Ronda de {player.GetTeam().Samurai.GetName()} (J{playerNumber})");
        view.WriteLine(GameConstants.Separator);
    }

    public static Unit? GetCurrentUnit(Player player)
    {
        var sortedUnits = player.GetSortedActiveUnitsByOrderOfAttack();
        return sortedUnits.FirstOrDefault(); 
    }

    public static void ApplyAffinityPenalty(AffinityContext affinityCtx, TurnContext turnContext)
    {
        UpdateTurnsBasedOnAffinity(affinityCtx, turnContext);
    }
    
    public static void UpdateTurnsBasedOnAffinity(AffinityContext affinityCtx, TurnContext turnContext)
    {
        string nameTarget = affinityCtx.Target.GetName();
        string typeAttack = affinityCtx.AttackType;
       
        // string targetAffinity = FileHelper.FindTargetInFileForStats(typeAttack, nameTarget);
        // ConsumeTurnsBasedOnAffinity(targetAffinity);
       
        ConsumeTurnsBasedOnAffinity(affinityCtx, turnContext);
    }

    public static void UpdateTurnStates(TurnContext ctx)
    {
        int fullNow = ctx.Attacker.GetFullTurns();
        int blinkNow = ctx.Attacker.GetBlinkingTurns();

        int fullConsumed = ctx.FullStart - fullNow;
        int blinkingConsumed = Math.Max(0, ctx.BlinkStart - blinkNow);
        int blinkingGained = Math.Max(0, blinkNow - ctx.BlinkStart);

        CombatUI.DisplayTurnChanges(fullConsumed, blinkingConsumed, blinkingGained);

        ctx.Defender?.RemoveFromActiveUnitsIfDead();
    }



    public static void ConsumeTurnsWhenPassedTurn(TurnContext ctx)
    {
        if (ctx.Attacker.GetBlinkingTurns() > 0)
        {
            ctx.Attacker.ConsumeBlinkingTurn(1);
        }
        else
        {
            ctx.Attacker.ConsumeFullTurn(1);
            ctx.Attacker.GainBlinkingTurn(1);
        }
    }
    
    public static void ManageTurnsWhenPassedTurn(TurnContext ctx)
    {
        ConsumeTurnsWhenPassedTurn(ctx);
        UpdateTurnStates(ctx);
        ctx.Attacker.ReorderUnitsWhenAttacked();
    }
    
    public static void ConsumeAllTurns()
    {
        
    }

    public static void ConsumeTurnsBasedOnAffinity(AffinityContext ctx, TurnContext turnCtx)
    {
        Player attackingPlayer = turnCtx.Attacker;
        string affinity = AffinityResolver.GetAffinity(ctx.Target, ctx.AttackType);

        switch (affinity)
        {
            case "Rp":
            case "Dr":
                ConsumeAllTurns();
                break;

            case "Nu":
                if (attackingPlayer.GetBlinkingTurns() >= 2)
                {
                    attackingPlayer.ConsumeBlinkingTurn(2);
                }
                else
                {
                    int blink = attackingPlayer.GetBlinkingTurns();
                    attackingPlayer.ConsumeBlinkingTurn(blink);
                    attackingPlayer.ConsumeFullTurn(2 - blink);
                }
                break;

            case "Miss":
                if (attackingPlayer.GetBlinkingTurns() >= 1)
                    attackingPlayer.ConsumeBlinkingTurn(1);
                else
                    attackingPlayer.ConsumeFullTurn(1);
                break;

            case "Wk":
                attackingPlayer.ConsumeFullTurn(1);
                attackingPlayer.GainBlinkingTurn(1);
                break;

            case "Rs":
            case "-":
                if (attackingPlayer.GetBlinkingTurns() > 0)
                {
                    attackingPlayer.ConsumeBlinkingTurn(1);
                }
                else
                {
                    attackingPlayer.ConsumeFullTurn(1);
                }
                break;
        }
    }

}
