using Shin_Megami_Tensei_View;
using Shin_Megami_Tensei.Combat;

namespace Shin_Megami_Tensei.Managers;

public class CombatManager
{
    public View view;
    private readonly Dictionary<string, Player> _players;
    private bool _gameWon;
    private bool _isNewRound;

    public CombatManager(View view, Dictionary<string, Player> players)
    {
        this.view = view;
        _players = players;
        _gameWon = false;
        _isNewRound = true;
    }

    public void StartCombat()
    {
        SetInitialTurns();
        PlayCombatRounds();
    }

    private void SetInitialTurns()
    {
        foreach (var player in _players.Values)
            player.SetTurns();
    }

    private void PlayCombatRounds()
    {
        Player currentPlayer = _players["Player 1"];
        
        view.WriteLine(GameConstants.Separator);
        while (!_gameWon)
        {
            HandleRound(ref currentPlayer);

            if (ShouldSwitchPlayer(currentPlayer))
            {
                currentPlayer = GetOpponent(currentPlayer);
                _isNewRound = true;
            }
        }
    }

    private void HandleRound(ref Player currentPlayer)
    {
        int playerNumber = GetPlayerNumber(currentPlayer);

        if (_isNewRound)
        {
            TurnManager.PrepareNewRound(currentPlayer, view, playerNumber);
            _isNewRound = false;
        }

        CombatUI.DisplayBoardState(_players, view);
        CombatUI.DisplayTurnInfo(currentPlayer, view);
        CombatUI.DisplaySortedUnits(currentPlayer, view);

        Unit? activeUnit = TurnManager.GetCurrentUnit(currentPlayer);
        CombatContext ctx = new(currentPlayer, GetOpponent(currentPlayer), view);

        UnitActionManager.ExecuteAction(activeUnit, ctx);

        CheckAndHandleVictory(currentPlayer);
    }
    
    private int GetPlayerNumber(Player player)
    {
        return player.GetName() == "Player 1" ? 1 : 2;
    }

    private bool ShouldSwitchPlayer(Player currentPlayer)
    {
        return currentPlayer.IsPlayerOutOfTurns();
    }

    public Player GetOpponent(Player currentPlayer)
    {
        return currentPlayer.GetName() == "Player 1" ? _players["Player 2"] : _players["Player 1"];
    }
    
    private void CheckAndHandleVictory(Player currentPlayer)
    {
        Player opponent = GetOpponent(currentPlayer);

        currentPlayer.CheckIfTeamIsAbleToContinue();
        opponent.CheckIfTeamIsAbleToContinue();

        if (!currentPlayer.IsTeamAbleToContinue())
        {
            CombatUI.DisplayWinner(opponent, view);
            _gameWon = true;
            return;
        }

        if (!opponent.IsTeamAbleToContinue())
        {
            CombatUI.DisplayWinner(currentPlayer, view);
            _gameWon = true;
            return;
        }
    }
}