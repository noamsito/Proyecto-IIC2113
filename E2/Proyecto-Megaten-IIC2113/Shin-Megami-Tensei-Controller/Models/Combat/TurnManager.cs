using Shin_Megami_Tensei_View;
using Shin_Megami_Tensei.Combat;

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
        return sortedUnits.FirstOrDefault(); // null si está vacío
    }

    public static bool IsOutOfTurns(Player player)
    {
        return player.IsPlayerOutOfTurns();
    }

    public static void ApplyAffinityPenalty(Player attacker, Unit target, string attackType)
    {
        attacker.UpdateTurnsBasedOnAffinity(attackType, target.GetName());
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
        ctx.Attacker.ReorderUnitsWhenAttacked();
    }



    public static void UpdateTurnsStandard(TurnContext ctx)
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
        UpdateTurnsStandard(ctx);
        UpdateTurnStates(ctx);
    }

    public static void UpdateTurnsWhenInvoked(TurnContext ctx)
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
}
