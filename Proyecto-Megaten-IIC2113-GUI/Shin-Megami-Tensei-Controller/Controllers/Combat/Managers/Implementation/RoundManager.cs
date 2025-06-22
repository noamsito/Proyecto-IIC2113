using Shin_Megami_Tensei_View.Implementation.Implementation;
using Shin_Megami_Tensei.Managers.Managers.Interfaces;

namespace Shin_Megami_Tensei.Managers.Managers.Implementation;

public class RoundManager : IRoundManager
{
    private readonly CombatUIFacade _ui;

    public RoundManager(CombatUIFacade ui)
    {
        _ui = ui ?? throw new ArgumentNullException(nameof(ui));
    }

    public void InitializePlayersForCombat(Dictionary<string, Player> players)
    {
        foreach (var player in players.Values)
        {
            player.TurnManager.SetTurns();
        }
    }

    public void StartNewRound(Player player, int playerNumber)
    {
        PreparePlayerForNewRound(player);
        DisplayRoundStart(player, playerNumber);
    }

    private void PreparePlayerForNewRound(Player player)
    {
        var turnManager = player.TurnManager;
        var unitManager = player.UnitManager;
            
        turnManager.SetTurns();
        unitManager.SetOrderOfAttackOfActiveUnits();
    }

    private void DisplayRoundStart(Player player, int playerNumber)
    {
        var samurai = player.GetTeam().Samurai;
        _ui.GameState.DisplayRoundStart(samurai, playerNumber);
    }
}
