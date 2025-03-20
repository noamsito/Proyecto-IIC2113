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
        
        this.content_teams_folder = File.ReadAllText(fullNameOfFile); 
        _view.WriteLine(fullNameOfFile);
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
        List<string> lines = GetNonEmptyLines(content_teams_folder);
        InitializePlayers();
    
        Player currentPlayer = null;
        List<string> teamUnits = new List<string>();
    
        foreach (string line in lines)
        {
            if (IsPlayerTeamLine(line, "Player 1 Team"))
            {
                currentPlayer = players["Player 1"];
                AssignTeamToCurrentPlayer(ref currentPlayer, teamUnits);
            }
            else if (IsPlayerTeamLine(line, "Player 2 Team"))
            {
                currentPlayer = players["Player 2"];
                AssignTeamToCurrentPlayer(ref currentPlayer, teamUnits);
            }
            else
            {
                teamUnits.Add(line);
            }
        }
    
        AssignTeamToCurrentPlayer(ref currentPlayer, teamUnits);
    }
    
    private List<string> GetNonEmptyLines(string content)
    {
        return content.Split('\n')
            .Select(line => line.Trim())
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToList();
    }
    
    private void InitializePlayers()
    {
        this.players = new Dictionary<string, Player>
        {
            { "Player 1", new Player("Player 1") },
            { "Player 2", new Player("Player 2") }
        };
    }
    
    private bool IsPlayerTeamLine(string line, string playerTeam)
    {
        return line.StartsWith(playerTeam);
    }
    
    private void AssignTeamToCurrentPlayer(ref Player currentPlayer, List<string> teamUnits)
    {
        if (currentPlayer != null)
        {
            currentPlayer.SetTeam(this.ConvertStringIntoTeam(teamUnits));
            teamUnits.Clear();
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
            else
            {
                _view.WriteLine("Doesn't have a samurai");
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
        
        string numberOfFileString = _view.ReadLine();
        
        this.AsignFileNameOfContents(int.Parse(numberOfFileString));
        SeparateTeamsOfPlayers();
        
        this.PrintTeams(players);
    }
}
