using Shin_Megami_Tensei_View;

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

    public static void PassTurn(Player player)
    {
        if (player.GetFullTurns() > 0)
        {
            player.DecreaseFullTurns(1);
            player.IncreaseBlinkingTurns(1);
        }
        else if (player.GetBlinkingTurns() > 0)
        {
            player.DecreaseBlinkingTurns(1);
        }
    }

    public static void ApplyAffinityPenalty(Player attacker, Unit target, string attackType)
    {
        attacker.UpdateTurnsBasedOnAffinity(attackType, target.GetName());
    }

    public static void UpdateTurnStates(Player attacker, Player? defender, int fullStart, int blinkStart, View view)
    {
        int fullNow = attacker.GetFullTurns();
        int blinkNow = attacker.GetBlinkingTurns();

        int fullConsumed = fullStart - fullNow;
        int blinkingConsumed = Math.Max(0, blinkStart - blinkNow);
        int blinkingGained = Math.Max(0, blinkNow - blinkStart);

        view.WriteLine($"Se han consumido {fullConsumed} Full Turn(s) y {blinkingConsumed} Blinking Turn(s)");
        view.WriteLine($"Se han obtenido {blinkingGained} Blinking Turn(s)");
        view.WriteLine(GameConstants.Separator);

        if (defender != null)
        {
            defender.RemoveFromActiveUnitsIfDead();
            attacker.SortUnitsWhenAnAttackHasBeenMade();
        }
    }
}
