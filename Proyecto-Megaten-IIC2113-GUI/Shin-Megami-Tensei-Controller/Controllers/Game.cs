using Shin_Megami_Tensei.Managers;
using Shin_Megami_Tensei_View;

namespace Shin_Megami_Tensei
{
    public class Game
    {
        private readonly View _view;
        private readonly string _teamsFolder;
        private readonly IShinMegamiTenseiView _guiView;
        private Dictionary<string, Player> _players;
        private readonly bool _useGUI;

        public Game(View view, string teamsFolder)
        {
            _view = view;
            _teamsFolder = teamsFolder;
            _useGUI = false;
            CombatUI.Initialize(view);
        }

        public Game(IShinMegamiTenseiView guiView)
        {
            _guiView = guiView;
            _teamsFolder = "";
            _useGUI = true;
        }

        public void Play()
        {
            if (_useGUI)
            {
                PlayGUIVersion();
            }
            else
            {
                PlayConsoleVersion();
            }
        }

        private void PlayConsoleVersion()
        {
            _players = GameLoader.LoadTeamsFromFile(_view, _teamsFolder);

            if (!ValidateAndCheckTeams())
            {
                _view.WriteLine("Archivo de equipos inválido");
                return;
            }

            StartCombat();
        }

        private void PlayGUIVersion()
        {
            _players = _guiView.LoadTeamsFromInput();

            if (!ValidateAndCheckTeams())
            {
                _guiView.ShowInvalidTeamMessage();
                return;
            }

            StartGUICombat();
        }

        private bool ValidateAndCheckTeams()
        {
            ValidateTeams();
            return AreTeamsValid();
        }

        private void StartCombat()
        {
            var combatManager = new CombatManager(_view, _players);
            combatManager.StartCombat();
        }

        private void StartGUICombat()
        {
            var guiCombatManager = new GUICombatManager(_guiView, _players);
            guiCombatManager.StartCombat();
        }

        private void ValidateTeams()
        {
            foreach (var player in _players.Values)
            {
                player.SetTeamValidation();
            }
        }

        private bool AreTeamsValid()
        {
            return _players["Player 1"].GetTeam().IsValid &&
                   _players["Player 2"].GetTeam().IsValid;
        }
    }
}
