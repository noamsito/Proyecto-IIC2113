using Shin_Megami_Tensei.Managers.Managers.Interfaces;

namespace Shin_Megami_Tensei.Managers.Managers.Implementation;

public class CombatManager : ICombatManager
    {
        private readonly ITurnManager _turnManager;
        private readonly IRoundManager _roundManager;
        private readonly IVictoryChecker _victoryChecker;
        private readonly Dictionary<string, Player> _players;
        
        private bool _gameWon;
        private Player _winner;

        public CombatManager(
            ITurnManager turnManager,
            IRoundManager roundManager,
            IVictoryChecker victoryChecker,
            Dictionary<string, Player> players)
        {
            _turnManager = turnManager ?? throw new ArgumentNullException(nameof(turnManager));
            _roundManager = roundManager ?? throw new ArgumentNullException(nameof(roundManager));
            _victoryChecker = victoryChecker ?? throw new ArgumentNullException(nameof(victoryChecker));
            _players = players ?? throw new ArgumentNullException(nameof(players));
        }

        public bool IsGameWon => _gameWon;
        public Player GetWinner() => _winner;

        public void StartCombat()
        {
            InitializeCombat();
            ExecuteCombatLoop();
        }

        private void InitializeCombat()
        {
            _roundManager.InitializePlayersForCombat(_players);
            _gameWon = false;
            _winner = null;
        }

        private void ExecuteCombatLoop()
        {
            Player currentPlayer = _players[PlayerConstants.PLAYER_ONE_NAME];

            while (!_gameWon)
            {
                _turnManager.ProcessPlayerTurn(currentPlayer);

                if (_victoryChecker.CheckForVictory(currentPlayer, _turnManager.GetOpponent(currentPlayer)))
                {
                    _gameWon = true;
                    break;
                }

                if (_turnManager.ShouldSwitchPlayer(currentPlayer))
                {
                    currentPlayer = _turnManager.GetOpponent(currentPlayer);
                }
            }
        }
    }
