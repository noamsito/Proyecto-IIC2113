using System.Reflection.Metadata.Ecma335;
using Shin_Megami_Tensei_View;
using Shin_Megami_Tensei.Gadgets;

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
    
    private Samurai SetUpSamurai(Samurai samurai, string unit)
    {;
        samurai.SetStatsFromJSON();
        // samurai.SetAbilities();
        return samurai;
    }

    private Team ConvertStringIntoTeam(List<string> teamUnits)
    {
      Team newTeam = new Team();
      foreach (var unit in teamUnits)
      {
          if (unit.StartsWith("[Samurai]"))
          {
              if (!newTeam.HasSamurai())
              {
                  string samuraiName = unit.Replace("[Samurai]", "").Trim();
                  Samurai NewSamurai = new Samurai(samuraiName);
                  this.SetUpSamurai(NewSamurai, unit);
                  newTeam.AddSamurai(NewSamurai);
              }
              else
              {
                  newTeam.SetTeamAsInvalid();
              }
          }
          else
          {
              string demonName = unit.Trim();
              newTeam.AddDemon(new Demon(demonName));
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
                AssignTeamToPlayer(ref currentPlayer, teamUnits, "Player 1");
                
            }
            else if (IsPlayerTeamLine(line, "Player 2 Team"))
            {
                AssignTeamToPlayer(ref currentPlayer, teamUnits, "Player 2");
            }
            else
            {
                teamUnits.Add(line);
            }
        }
    
        AssignTeamToCurrentPlayer(ref currentPlayer, teamUnits);
    }
    
    private void AssignTeamToPlayer(ref Player currentPlayer, List<string> teamUnits, string playerName)
    {
        if (currentPlayer != null)
        {
            AssignTeamToCurrentPlayer(ref currentPlayer, teamUnits);
        }
        currentPlayer = players[playerName];
    }
    
    private void AssignTeamToCurrentPlayer(ref Player currentPlayer, List<string> teamUnits)
    {
        if (currentPlayer != null)
        {
            currentPlayer.SetTeam(ConvertStringIntoTeam(teamUnits));
            teamUnits.Clear();
        }
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
    
    public void PrintTeams(Dictionary<string, Player> players)
    {
        foreach (var player in players)
        {
            _view.WriteLine($"{player.Key}'s Team:");
            Team team = player.Value.Team;

            if (team.HasSamurai())
            {
                Samurai samurai = team.GetSamurai();
                _view.WriteLine($"Samurai: {samurai.GetName()}");
                
                // samurai.PrintStats();
            }
            else
            {
                _view.WriteLine("Doesn't have a samurai");
            }

            foreach (var demon in team.GetDemons())
            {
                _view.WriteLine($"Demon: {demon.GetName()}");
            }
            _view.WriteLine("\n");
        }
    }

    public void CombatLogic()
    {
        
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

        if (this.players["Player 1"].IsTeamValid() || this.players["Player 2"].IsTeamValid())
        {
            _view.WriteLine("Archivo de equipos inválido");
        }
        else
        {
            // continue the flow
        }
        
        this.PrintTeams(players);
    }
}
