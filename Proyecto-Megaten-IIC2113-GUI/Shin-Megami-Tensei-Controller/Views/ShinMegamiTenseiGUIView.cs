using Shin_Megami_Tensei_GUI;
using Shin_Megami_Tensei;
using Shin_Megami_Tensei.Gadgets;
using Shin_Megami_Tensei.Views;
using Shin_Megami_Tensei.Views.Interfaces;

namespace Shin_Megami_Tensei_View.Implementation
{
    public class ShinMegamiTenseiGUIView : IShinMegamiTenseiView
    {
        private readonly SMTGUI _gui;
        private readonly ITeamLoader _teamLoader;
        private readonly IGameStatePresenter _gameStatePresenter;
        private readonly IUserInputHandler _inputHandler;
        private readonly TargetSelectionService _targetSelector;

        private Dictionary<string, Player> _currentPlayers;

        public ShinMegamiTenseiGUIView(SMTGUI gui)
        {
            _gui = gui ?? throw new ArgumentNullException(nameof(gui));
            _teamLoader = new GUITeamLoader();
            _gameStatePresenter = new GameStatePresenter(gui);
            _inputHandler = new UserInputHandler(gui);
            _targetSelector = new TargetSelectionService(gui);
        }

        public Dictionary<string, Player> LoadTeamsFromInput()
        {
            var teamInfos = _teamLoader.LoadTeamInformation(_gui);
            _currentPlayers = _teamLoader.CreatePlayersFromTeamInfo(teamInfos);
            
            _targetSelector.SetPlayers(_currentPlayers);
            
            ValidateTeams();
            return _currentPlayers;
        }

        public void ShowInvalidTeamMessage()
        {
            _gui.ShowEndGameMessage("Al menos un equipo es inválido");
        }

        public void ShowWinner(Player winner)
        {
            var winnerMessage = CreateWinnerMessage(winner);
            _gui.ShowEndGameMessage(winnerMessage);
        }

        public void DisplayGameState(Dictionary<string, Player> players, Player currentPlayer, List<string> options)
        {
            var displayState = new GameDisplayState(players, currentPlayer, options);
            _gameStatePresenter.UpdateDisplay(displayState);
        }

        public string GetPlayerChoice()
        {
            return _inputHandler.WaitForValidChoice();
        }

        public Unit SelectTarget(Player targetPlayer)
        {
            EnsurePlayersInitialized();
            var context = new TargetSelectionContext(
                GetCurrentPlayer(), 
                targetPlayer, 
                "Selecciona un objetivo"
            );
            return _targetSelector.SelectTargetUnit(context);
        }

        public Skill SelectSkill(Unit unit)
        {
            EnsurePlayersInitialized();
            return _targetSelector.SelectSkillFromUnit(unit);
        }

        public Unit SelectSummonTarget(Player player)
        {
            EnsurePlayersInitialized();
            return _targetSelector.SelectSummonableUnit(player);
        }

        public int SelectSlot(Player player)
        {
            EnsurePlayersInitialized();
            return _targetSelector.SelectBoardSlot(player);
        }

        private void EnsurePlayersInitialized()
        {
            if (_currentPlayers == null)
            {
                throw new InvalidOperationException("Players must be loaded before performing selections");
            }
            _targetSelector.SetPlayers(_currentPlayers);
        }

        private void ValidateTeams()
        {
            foreach (var player in _currentPlayers.Values)
            {
                player.SetTeamValidation();
            }
        }

        private string CreateWinnerMessage(Player winner)
        {
            var playerNumber = DeterminePlayerNumber(winner);
            var samuraiName = winner.GetTeam().Samurai.GetName();
            return $"Ganador: {samuraiName} (J{playerNumber})";
        }

        private int DeterminePlayerNumber(Player winner)
        {
            return winner.GetName() == "Player 1" ? 1 : 2;
        }

        private Player GetCurrentPlayer()
        {
            return _currentPlayers?.Values.FirstOrDefault() 
                   ?? throw new InvalidOperationException("No current player available");
        }
    }
}
