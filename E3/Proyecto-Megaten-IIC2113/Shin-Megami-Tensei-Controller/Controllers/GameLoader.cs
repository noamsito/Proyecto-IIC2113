using Shin_Megami_Tensei_View;
using Shin_Megami_Tensei.String_Handlers;

namespace Shin_Megami_Tensei.Managers;

public static class GameLoader
{
    public static Dictionary<string, Player> LoadTeamsFromFile(View view, string folderPath)
    {
        int totalTeams = Directory.GetFiles(folderPath, "*.txt").Length;
        view.WriteLine("Elige un archivo para cargar los equipos");
        for (int i = 0; i < totalTeams; i++)
        {
            string fileNumber = (i + 1).ToString("D3");
            view.WriteLine($"{i}: {fileNumber}.txt");
        }

        string selected = view.ReadLine();
        string filePath = FileHelper.GetFileName(int.Parse(selected), folderPath);
        List<string> lines = StringHelper.GetNonEmptyLines(File.ReadAllText(filePath));

        var players = new Dictionary<string, Player>
        {
            { "Player 1", new("Player 1") },
            { "Player 2", new("Player 2") }
        };

        ParseTeamLines(lines, players);
        
        return players;
    }

    private static void ParseTeamLines(List<string> lines, Dictionary<string, Player> players)
    {
        Player current = null;
        List<string> teamBuffer = new();

        foreach (string line in lines)
        {
            var idLinePlayer = StringHelper.GetPlayerIdentifierFromHeaderLine(line);
            if (idLinePlayer != null)
            {
                if (current != null)
                    current.SetTeam(TeamBuilder.BuildTeamFromStringList(teamBuffer));
                teamBuffer.Clear();
                current = players[idLinePlayer];
            }
            else
            {
                teamBuffer.Add(line);
            }
        }

        if (current != null)
            current.SetTeam(TeamBuilder.BuildTeamFromStringList(teamBuffer));
    }
}