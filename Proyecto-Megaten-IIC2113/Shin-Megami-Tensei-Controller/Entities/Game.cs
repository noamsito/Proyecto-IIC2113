using System.Reflection.Metadata.Ecma335;
using Shin_Megami_Tensei_View;

namespace Shin_Megami_Tensei;

public class Game
{
    View _view;
    
    readonly string _teamsFolder; 
    readonly int NUMBER_TOTAL_TEAMS;

    private Dictionary<string, Player> players;
    
    string content_teams_folder;
    
    public Game(View view, string teamsFolder)
    {
        this._teamsFolder = teamsFolder;
        _view = view;

        NUMBER_TOTAL_TEAMS = Directory.GetFiles(_teamsFolder, "*.txt").Length;
        
    }

    public void AsignFileNameOfContents(int numberOfFile)
    {
        string fullNameOfFile;
        if (numberOfFile < 9)
        {
            fullNameOfFile = $"{_teamsFolder}/00{numberOfFile + 1}.txt";
        }
        else
        {
            fullNameOfFile = $"{_teamsFolder}/0{numberOfFile + 1}.txt";
        }
        
        this.content_teams_folder = File.ReadAllText(fullNameOfFile); // remember to fix this
    }

    public Team ConvertStringIntoTeam(List<string> teamUnits)
    {
        Team newTeam = new Team();
        for (var i = 0; i < teamUnits.Count(); i++)
        {
            if (!teamUnits[i].Contains("Samurai") && !teamUnits[i].Contains("Player"))
            {
                newTeam.AddDemon(new Demon(teamUnits[i]));
            }
            else if (!newTeam.HasSamurai())
            {
                newTeam.AddSamurai(new Samurai(name: teamUnits[i]));
            }
            else if (newTeam.HasSamurai())
            {
                newTeam.SetTeamAsInvalid();
            }
        }
        return newTeam;
    }

    private void SeparateTeamsOfPlayers()
    {
        List<string> lines = content_teams_folder.Split('\n')
            .Select(line => line.Trim())
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToList();

        this.players = new Dictionary<string, Player>
        {
            { "Player 1", new Player("Player 1") },
            { "Player 2", new Player("Player 2") }
        };

        Player currentPlayer = null;
        List<string> auxiliarTeam = new List<string>();

        foreach (string line in lines)
        {
            if (line.StartsWith("Player 1 Team"))
            {
                if (currentPlayer != null)
                {
                    currentPlayer.SetTeam(this.ConvertStringIntoTeam(auxiliarTeam));
                    auxiliarTeam.Clear();
                }
                currentPlayer = players["Player 1"];
            }
            else if (line.StartsWith("Player 2 Team"))
            {
                if (currentPlayer != null)
                {
                    currentPlayer.SetTeam(this.ConvertStringIntoTeam(auxiliarTeam));
                    auxiliarTeam.Clear();
                }
                currentPlayer = players["Player 2"];
            }
            else
            {
                auxiliarTeam.Add(line);
            }
        }

        if (currentPlayer != null)
        {
            currentPlayer.SetTeam(this.ConvertStringIntoTeam(auxiliarTeam));
        }
    }

    public void PrintTeams(Dictionary<string, Player> players)
    {
        foreach (var player in players)
        {
            _view.WriteLine($"{player.Key}'s Team:");
            Team team = player.Value.Team;

            if (team.HasSamurai())
            {
                _view.WriteLine($"Samurai: {team.GetSamurai().Name}");
            }

            foreach (var demon in team.GetDemons())
            {
                _view.WriteLine($"Demon: {demon.Name}");
            }
            _view.WriteLine("\n");
        }
    }
    
    public void Play()
    {
        _view.WriteLine("Elige un archivo para cargar los equipos");
        for (int i = 0; i < NUMBER_TOTAL_TEAMS; i++)
        {
            _view.WriteLine(i < 9 ? $"{i}: 00{i + 1}.txt" : $"{i}: 0{i + 1}.txt");
        }
        
        string numberOfFile = _view.ReadLine();
        this.AsignFileNameOfContents(int.Parse(numberOfFile));
        SeparateTeamsOfPlayers();

        if (this.players["Player 1"].Team.HasSamurai())
        {
            _view.WriteLine("skljfsdk\n");
        }

        if (this.players["Player 2"].Team.HasSamurai())
        {
            _view.WriteLine("skljfsdk 2 ");
        }
        
        // this.PrintTeams(this.players);
    }
}
