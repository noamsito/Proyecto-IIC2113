using Shin_Megami_Tensei.Managers;
using Shin_Megami_Tensei_View;

namespace Shin_Megami_Tensei;

public class Game
{
    public readonly View view;
    private readonly string _teamsFolder;
    private Dictionary<string, Player> _players;

    public Game(View view, string teamsFolder)
    {
        this.view = view;
        _teamsFolder = teamsFolder;
    }

    public void Play()
    {
        _players = GameLoader.LoadTeamsFromFile(view, _teamsFolder);
        ValidateTeams();

        if (AreTeamsValid())
        {
            var combatManager = new CombatManager(view, _players);
            combatManager.StartCombat();
        }
        else
        {
            view.WriteLine("Archivo de equipos inválido");
        }
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