using Shin_Megami_Tensei.Enums;
using Shin_Megami_Tensei.Managers.Managers.Interfaces;

namespace Shin_Megami_Tensei.Managers.Managers.Implementation;

public class TurnManager : ITurnManager
    {
        private readonly IActionExecutor _actionExecutor;
        private readonly IRoundManager _roundManager;
        private readonly Dictionary<string, Player> _players;
        private bool _isNewRound = true;

        public TurnManager(
            IActionExecutor actionExecutor,
            IRoundManager roundManager,
            Dictionary<string, Player> players)
        {
            _actionExecutor = actionExecutor ?? throw new ArgumentNullException(nameof(actionExecutor));
            _roundManager = roundManager ?? throw new ArgumentNullException(nameof(roundManager));
            _players = players ?? throw new ArgumentNullException(nameof(players));
        }

        public void ProcessPlayerTurn(Player currentPlayer)
        {
            int playerNumber = DeterminePlayerNumber(currentPlayer);

            HandleNewRoundIfNeeded(currentPlayer, playerNumber);
            
            Unit activeUnit = GetCurrentUnit(currentPlayer);
            if (!IsValidActiveUnit(activeUnit))
            {
                ConsumeCurrentTurn(currentPlayer);
                return;
            }

            bool actionWasExecuted = _actionExecutor.TryExecuteUnitAction(activeUnit, currentPlayer);
            
            if (!actionWasExecuted)
            {
                ConsumeCurrentTurn(currentPlayer);
            }
        }

        public bool ShouldSwitchPlayer(Player currentPlayer)
        {
            return currentPlayer.TurnManager.IsPlayerOutOfTurns();
        }

        public Player GetOpponent(Player currentPlayer)
        {
            return currentPlayer.GetName() == PlayerNameConstants.PlayerOneName 
                ? _players[PlayerNameConstants.PlayerTwoName] 
                : _players[PlayerNameConstants.PlayerOneName];
        }

        private void HandleNewRoundIfNeeded(Player currentPlayer, int playerNumber)
        {
            if (_isNewRound)
            {
                _roundManager.StartNewRound(currentPlayer, playerNumber);
                _isNewRound = false;
            }
        }

        private Unit GetCurrentUnit(Player player)
        {
            var sortedUnits = player.UnitManager.GetSortedActiveUnitsByOrderOfAttack();
            return sortedUnits.FirstOrDefault();
        }

        private bool IsValidActiveUnit(Unit unit)
        {
            return unit != null && IsUnitAlive(unit);
        }

        private bool IsUnitAlive(Unit unit)
        {
            return unit.GetCurrentStats().GetStatByName(StatType.HP.ToGameString()) > 0;
        }

        private void ConsumeCurrentTurn(Player currentPlayer)
        {
            var turnManager = currentPlayer.TurnManager;
            
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

        private int DeterminePlayerNumber(Player player)
        {
            return player.GetName() == PlayerNameConstants.PlayerOneName ? 1 : 2;
        }
    }
