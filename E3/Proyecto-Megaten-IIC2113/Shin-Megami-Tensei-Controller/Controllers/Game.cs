using Shin_Megami_Tensei.Managers;
using Shin_Megami_Tensei_View;

namespace Shin_Megami_Tensei;

public class Game
{
    private readonly View _view;
    private readonly string _teamsFolder;
    private Dictionary<string, Player> _players;

    public Game(View view, string teamsFolder)
    {
        _view = view;
        _teamsFolder = teamsFolder;

        CombatUI.Initialize(view);
    }

    public void Play()
    {
        _players = GameLoader.LoadTeamsFromFile(_view, _teamsFolder);

        if (!ValidateAndCheckTeams())
        {
            _view.WriteLine("Archivo de equipos inválido");
            return;
        }

        StartCombat();
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