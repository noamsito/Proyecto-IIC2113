using System.Reflection.Metadata.Ecma335;
using Shin_Megami_Tensei_View;

namespace Shin_Megami_Tensei;

public class Game
{
    View _view;
    
    readonly string _teamsFolder;
    readonly string content_teams_folder;
    readonly int NUMBER_TOTAL_TEAMS;
    
    public Game(View view, string teamsFolder)
    {
        this._teamsFolder = teamsFolder;
        _view = view;

        NUMBER_TOTAL_TEAMS = Directory.GetFiles(_teamsFolder, "*.txt").Length;
        
        content_teams_folder = File.ReadAllText($"{_teamsFolder}/011.txt"); // remember to fix this
    }

    public Team ConvertStringIntoTeam(List<string> teamUnits)
    {
        Team newTeam = new Team();
        for (int i = 0; i < teamUnits.Count(); i++)
        {
            if (string.IsNullOrWhiteSpace(teamUnits[i])) continue;
            
            if (!teamUnits[i].Contains("Samurai") && !teamUnits[i].Contains("Player"))
            {
                newTeam.AddDemon(new Demon(teamUnits[i]));
            }
            else
            {
                // new_team.AddSamurai(new Samurai(name: teamUnits[i]));
            }
        }
        return newTeam;
    }

    public void SeparateTeamsOfPlayers(int numberOfFile) // change to "content_teams_folder"
    {
        List<string> lines = content_teams_folder.Split('\n')
            .Select(line => line.Trim())  
            .Where(line => !string.IsNullOrWhiteSpace(line)) 
            .ToList();

        Dictionary<string, Player> players = new Dictionary<string, Player>
        {
            { "Player 1", new Player("Player 1") },
            { "Player 2", new Player("Player 2") }
        };

        Player currentPlayer = null;
        List<string> auxiliarTeam = new List<string>();
        
        foreach (string line in lines)
        {
            // _view.WriteLine($"[{line}]");
            auxiliarTeam.Add(line);
            if (line.StartsWith("Player 1 Team"))
            {
                currentPlayer = players["Player 1"];
            }
            else if (line.StartsWith("Player 2 Team"))
            {
                // _view.WriteLine($"{}");
                currentPlayer.SetTeam(this.ConvertStringIntoTeam(auxiliarTeam));
                currentPlayer = players["Player 2"];
            }
        }
        
        // currentPlayer.SetTeam(this.ConvertStringIntoTeam(auxiliarTeam));

        // Opcional: Verificar la salida en consola
        // foreach (var player in players.Values)
        // {
        //     _view.WriteLine(player.Team.GetSamurai().Name);
        //     foreach (var unit in player.Team.GetDemons())
        //     {
        //         _view.WriteLine($"  - {unit.Name}");
        //     }
        // }
    }

    
    public void Play()
    {
        _view.WriteLine("Elige un archivo para cargar los equipos");
        for (int i = 0; i < NUMBER_TOTAL_TEAMS; i++)
        {
            _view.WriteLine(i < 9 ? $"{i}: 00{i + 1}.txt" : $"{i}: 0{i + 1}.txt");
        }
        
        string numberOfFile = _view.ReadLine();
        SeparateTeamsOfPlayers(int.Parse(numberOfFile));
    }
}
