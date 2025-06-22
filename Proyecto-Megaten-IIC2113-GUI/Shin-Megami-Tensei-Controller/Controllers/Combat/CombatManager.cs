using Shin_Megami_Tensei_View;
using Shin_Megami_Tensei.Combat;

namespace Shin_Megami_Tensei.Managers;

public class CombatManager
{
    private readonly View _view;
    private readonly Dictionary<string, Player> _players;
    private bool _gameWon;
    private bool _isNewRound;

    private const string Player1Key = "Player 1";
    private const string Player2Key = "Player 2";

    public CombatManager(View view, Dictionary<string, Player> players)
    {
        _view = view;
        _players = players;
        _gameWon = false;
        _isNewRound = true;
    }

    public void StartCombat()
    {
        InitializePlayersForCombat();
        ExecuteCombatLoop();
    }

    private void InitializePlayersForCombat()
    {
        foreach (var player in _players.Values)
        {
            PlayerTurnManager turnManager = player.TurnManager;
            turnManager.SetTurns();
        }
    }

    private void ExecuteCombatLoop()
    {
        Player currentPlayer = _players[Player1Key];

        _view.WriteLine(GameConstants.Separator);
        while (!_gameWon)
        {
            ProcessPlayerTurn(currentPlayer);

            if (ShouldSwitchPlayer(currentPlayer))
            {
                currentPlayer = GetOpponent(currentPlayer);
                _isNewRound = true;
            }
        }
    }

    private void ProcessPlayerTurn(Player currentPlayer)
    {
        int playerNumber = GetPlayerNumber(currentPlayer);

        HandleNewRoundIfNeeded(currentPlayer, playerNumber);
        DisplayGameState(currentPlayer);

        bool actionWasExecuted = ExecuteUnitAction(currentPlayer);
        
        if (!actionWasExecuted)
        {
            ConsumeCurrentTurn(currentPlayer);
        }
        
        CheckForVictory(currentPlayer);
    }

    private void HandleNewRoundIfNeeded(Player currentPlayer, int playerNumber)
    {
        if (_isNewRound)
        {
            PrepareNewRound(currentPlayer, playerNumber);
            _isNewRound = false;
        }
    }

    private void DisplayGameState(Player currentPlayer)
    {
        CombatUI.DisplayBoardState(_players);
        CombatUI.DisplayTurnInfo(currentPlayer);
        CombatUI.DisplaySortedUnits(currentPlayer);
    }

    private bool ExecuteUnitAction(Player currentPlayer)
    {
        Unit? activeUnit = TurnManager.GetCurrentUnit(currentPlayer);
        
        if (activeUnit == null || !IsUnitAlive(activeUnit))
        {
            return false;
        }

        CombatContext combatContext = CreateCombatContext(currentPlayer);
        TurnContext turnContext = CreateTurnContext(combatContext, currentPlayer);

        UnitActionManager.ExecuteAction(activeUnit, combatContext, turnContext);
        return true;
    }

    private void ConsumeCurrentTurn(Player currentPlayer)
    {
        PlayerTurnManager turnManager = currentPlayer.TurnManager;
        
        if (turnManager.GetBlinkingTurns() > 0)
        {
            turnManager.ConsumeBlinkingTurn(1);
        }
        else if (turnManager.GetFullTurns() > 0)
        {
            turnManager.ConsumeFullTurn(1);
        }
        
        currentPlayer.UnitManager.RearrangeSortedUnitsWhenAttacked();
    }

    private bool IsUnitAlive(Unit unit)
    {
        return unit.GetCurrentStats().GetStatByName("HP") > 0;
    }

    private CombatContext CreateCombatContext(Player currentPlayer)
    {
        return new CombatContext(currentPlayer, GetOpponent(currentPlayer), _view);
    }

    private TurnContext CreateTurnContext(CombatContext combatContext, Player currentPlayer)
    {
        PlayerTurnManager turnManagerCurrentPlayer = currentPlayer.TurnManager;
        
        int fullStartTurns = turnManagerCurrentPlayer.GetFullTurns();
        int blinkingStartTurns = turnManagerCurrentPlayer.GetBlinkingTurns();

        return new TurnContext(
            combatContext.CurrentPlayer,
            combatContext.Opponent,
            fullStartTurns,
            blinkingStartTurns
        );
    }

    private int GetPlayerNumber(Player player)
    {
        return player.GetName() == Player1Key ? 1 : 2;
    }

    private bool ShouldSwitchPlayer(Player currentPlayer)
    {
        return currentPlayer.TurnManager.IsPlayerOutOfTurns();
    }

    public Player GetOpponent(Player currentPlayer)
    {
        return currentPlayer.GetName() == Player1Key ? _players[Player2Key] : _players[Player1Key];
    }

    private void CheckForVictory(Player currentPlayer)
    {
        Player opponent = GetOpponent(currentPlayer);

        UpdateTeamsStatus(currentPlayer, opponent);

        if (!currentPlayer.CombatState.IsTeamAbleToContinue())
        {
            AnnounceWinner(opponent);
            return;
        }

        if (!opponent.CombatState.IsTeamAbleToContinue())
        {
            AnnounceWinner(currentPlayer);
        }
    }

    private void UpdateTeamsStatus(Player currentPlayer, Player opponent)
    {
        currentPlayer.CombatState.CheckIfTeamIsAbleToContinue();
        opponent.CombatState.CheckIfTeamIsAbleToContinue();
    }

    private void AnnounceWinner(Player winner)
    {
        CombatUI.DisplayWinner(winner);
        _gameWon = true;
    }
    
    public static void PrepareNewRound(Player player, int playerNumber)
    {
        Samurai samurai = player.GetTeam().Samurai;
        PlayerTurnManager turnManagerPlayer = player.TurnManager;
        PlayerUnitManager unitManagerPlayer = player.UnitManager;
        
        turnManagerPlayer.SetTurns();
        unitManagerPlayer.SetOrderOfAttackOfActiveUnits();
        CombatUI.DisplayRoundPlayer(samurai, playerNumber);
    }
}