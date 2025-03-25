using Shin_Megami_Tensei_View;
using Shin_Megami_Tensei.Gadgets;

namespace Shin_Megami_Tensei;

public class Game
{
    private readonly View _view;
    private readonly string _teamsFolder;
    private readonly int _totalTeams;
    private const int SeparatorLinesCount = 40;
    private readonly string Separator = new string('-', SeparatorLinesCount);
    private Dictionary<string, Player> _players;
    private string _teamsContent;

    public Game(View view, string teamsFolder)
    {
        _view = view;
        _teamsFolder = teamsFolder;
        _totalTeams = Directory.GetFiles(_teamsFolder, "*.txt").Length;
    }

    public void Play()
    {
        PromptUserToSelectFile();
        LoadTeamsFromFile();
        ValidateTeams();
        if (AreTeamsValid())
        {
            _view.WriteLine("Equipos validos");
            // StartCombat();
        }
        else
        {
            _view.WriteLine("Archivo de equipos inválido");
        }
    }

    private void PromptUserToSelectFile()
    {
        _view.WriteLine("Elige un archivo para cargar los equipos");
        for (int i = 0; i < _totalTeams; i++)
        {
            _view.WriteLine(i < 9 ? $"{i}: 00{i + 1}.txt" : $"{i}: 0{i + 1}.txt");
        }
    }

    private void LoadTeamsFromFile()
    {
        string fileNumber = _view.ReadLine();
        string fileName = GetFileName(int.Parse(fileNumber));
        _teamsContent = File.ReadAllText(fileName);
        SeparateTeams();
    }

    private string GetFileName(int fileNumber)
    {
        return fileNumber < 9 ? $"{_teamsFolder}/00{fileNumber + 1}.txt" : $"{_teamsFolder}/0{fileNumber + 1}.txt";
    }

    private void SeparateTeams()
    {
        List<string> lines = GetNonEmptyLines(_teamsContent);
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

        if (currentPlayer != null)
        {
            currentPlayer.SetTeam(ConvertStringToTeam(teamUnits));
        }
    }

    private void AssignTeamToPlayer(ref Player currentPlayer, List<string> teamUnits, string playerName)
    {
        if (currentPlayer != null)
        {
            currentPlayer.SetTeam(ConvertStringToTeam(teamUnits));
            teamUnits.Clear();
        }

        currentPlayer = _players[playerName];
    }

    private Team ConvertStringToTeam(List<string> teamUnits)
    {
        Team newTeam = new Team();
        foreach (var unit in teamUnits)
        {
            if (unit.StartsWith("[Samurai]"))
            {
                AddSamuraiToTeam(newTeam, unit);
            }
            else
            {
                AddDemonToTeam(newTeam, unit);
            }
        }

        return newTeam;
    }

    private void AddSamuraiToTeam(Team newTeam, string unit)
    {
        if (!newTeam.HasSamurai())
        {
            var (samuraiName, skills) = ExtractSamuraiDetails(unit);

            Samurai samurai = new Samurai(samuraiName);
            
            SetUpSamurai(ref samurai, skills);
            newTeam.AddSamurai(samurai);
        }
        else
        {
            newTeam.SetSamuraiRepeated();
        }
    }

    private void AddDemonToTeam(Team newTeam, string unit)
    {
        string demonName = unit.Trim();
        Demon demon = new Demon(demonName);
        demon.SetStatsFromJSON();
        demon.SetDemonSkillsFromJSON();
        newTeam.AddDemon(demon);
    }

    private (string, List<string>) ExtractSamuraiDetails(string unit)
    {
        List<string> skills = new List<string>();
        int nameStart = "[Samurai]".Length;
        int parenthesisStart = unit.IndexOf('(');
        string samuraiName = "";
        string skillsString = "";

        if (parenthesisStart != -1)
        {
            int parenthesisEnd = unit.IndexOf(')', parenthesisStart);
            if (parenthesisEnd != -1)
            {
                skillsString = unit.Substring(parenthesisStart + 1, parenthesisEnd - parenthesisStart - 1);
            }

            skills = skillsString.Split(',')
                .Select(word => word.Trim())
                .ToList();
            samuraiName = unit.Substring(nameStart, parenthesisStart - nameStart).Trim();
        }
        else
        {
            samuraiName = unit.Substring(nameStart).Trim();
        }

        return (samuraiName, skills);
    }

    private void SetUpSamurai(ref Samurai samurai, List<string> skills)
    {
        samurai.SetStatsFromJSON();
        samurai.SetSamuraiSkillsFromJSON(skills);
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
        _players = new Dictionary<string, Player>
        {
            { "Player 1", new Player("Player 1") },
            { "Player 2", new Player("Player 2") }
        };
    }

    private bool IsPlayerTeamLine(string line, string playerTeam)
    {
        return line.StartsWith(playerTeam);
    }

    private void ValidateTeams()
    {
        foreach (var player in _players.Values)
        {
            player.IsTeamValid();
        }
    }

    private bool AreTeamsValid()
    {
        return _players["Player 1"].IsTeamValid() && _players["Player 2"].IsTeamValid();
    }

    private void StartCombat()
    {
        SetPlayerTurns();
        Player currentPlayer = _players["Player 1"];
        bool gameWon = false;

        while (!gameWon)
        {
            int playerNumber = currentPlayer.GetName() == "Player 1" ? 1 : 2;

            _view.WriteLine(Separator);
            DisplayRoundHeader(currentPlayer, playerNumber);
            _view.WriteLine(Separator);

            
            ShowBoardStatus();
            _view.WriteLine(Separator);
            
            ShowTurns(currentPlayer);
            _view.WriteLine(Separator);
            
            ShowSortedUnits(currentPlayer);
            _view.WriteLine(Separator);
            
            string samuraiAction = GetSamuraiAction(currentPlayer.GetTeam().GetSamurai());
            _view.WriteLine(Separator);
            ManageSamuraiAction(samuraiAction, ref currentPlayer);
        }
    }

    private void SetPlayerTurns()
    {
        foreach (Player player in _players.Values)
        {
            player.SetTurns();
        }
    }

    private void DisplayRoundHeader(Player currentPlayer, int playerNumber)
    {
        _view.WriteLine($"Ronda de {currentPlayer.GetTeam().GetSamurai().GetName()} (J{playerNumber})");
    }

    private void ShowBoardStatus()
    {
        int playerNumber = 1;
        foreach (Player player in _players.Values)
        {
            ShowPlayerStatus(player, playerNumber);
            playerNumber++;
        }
    }

    private void ShowPlayerStatus(Player player, int playerNumber)
    {
        _view.WriteLine($"Equipo de {player.GetTeam().GetSamurai().GetName()} (J{playerNumber})");
        ShowTeamWithLetter(player);
    }

    private void ShowTeamWithLetter(Player player)
    {
        Team team = player.GetTeam();
        Samurai samurai = team.GetSamurai();
        char unitLabel = 'A';

        DisplayUnitStatus(samurai, unitLabel);
        var demons = team.GetDemons().Take(3).ToList();

        for (int i = 0; i < 3; i++)
        {
            unitLabel++;
            if (i < demons.Count)
            {
                DisplayUnitStatus(demons[i], unitLabel);
            }
            else
            {
                _view.WriteLine($"{unitLabel}-");
            }
        }
    }

    private void ShowTargetTeam(Player player)
    {
        Team targetTeam = player.GetTeam();
        Samurai targetSamurai = targetTeam.GetSamurai();
        List<Demon> targetDemons = targetTeam.GetDemons();
        
        int i = 0 ;
        
        _view.WriteLine($"{i + 1}-{targetSamurai.GetName()} " +
                        $"HP:{targetSamurai.GetCurrentStats().GetStatByName("HP")}/{targetSamurai.GetBaseStats().GetStatByName("HP")} " +
                        $"MP:{targetSamurai.GetCurrentStats().GetStatByName("MP")}/{targetSamurai.GetBaseStats().GetStatByName("MP")}");

        for (i = i; i < 3 && i < targetDemons.Count; i++)
        {
            Demon currentDemon = targetDemons[i];
            _view.WriteLine($"{i + 1}-{currentDemon.GetName()} " +
                            $"HP:{currentDemon.GetCurrentStats().GetStatByName("HP")}/{currentDemon.GetBaseStats().GetStatByName("HP")} " +
                            $"MP:{currentDemon.GetCurrentStats().GetStatByName("MP")}/{currentDemon.GetBaseStats().GetStatByName("MP")}");;
        }

        i = (targetDemons.Count == 0) ? ++i : i;
        
        _view.WriteLine($"{i + 1}-Cancelar");
    }

    private void DisplayUnitStatus(Unit unit, char label)
    {
        Stat baseStats = unit.GetBaseStats();
        Stat currentStats = unit.GetCurrentStats();
        _view.WriteLine($"{label}-{unit.GetName()} HP:{currentStats.GetStatByName("HP")}/{baseStats.GetStatByName("HP")} MP:{currentStats.GetStatByName("MP")}/{baseStats.GetStatByName("MP")}");
    }

    private string GetSamuraiAction(Samurai samurai)
    {
        _view.WriteLine($"Seleccione una acción para {samurai.GetName()}");
        string options = "1: Atacar\n2: Disparar\n3: Usar Habilidad\n4: Invocar\n5: Pasar Turno\n6: Rendirse";
        _view.WriteLine(options);
        return _view.ReadLine();
    }

    private void ShowTurns(Player player)
    {
        _view.WriteLine($"Full Turns: {player.GetFullTurns()}");
        _view.WriteLine($"Blinking Turns: {player.GetBlinkingTurns()}");
    }

    private void ShowSortedUnits(Player currentPlayer)
    {
        _view.WriteLine("Orden:");
        Team team = currentPlayer.GetTeam();

        if (team.HasSamurai())
        {
            _view.WriteLine($"1-{team.GetSamurai().GetName()}");
        }

        if (team.GetDemons().Count > 0)
        {
            int demonNumber = 2;
            List<Demon> sortedDemons = team.GetSortedDemonsBySpeed();
            foreach (Demon demon in sortedDemons)
            {
                _view.WriteLine($"{demonNumber}-{demon.GetName()}");
                demonNumber++;
            }
        }
    }
    
    private Player GetOpponent(Player currentPlayer)
    {
        return currentPlayer.GetName() == "Player 1" ? _players["Player 2"] : _players["Player 1"];
    }

    private void ManageSamuraiAction(string action, ref Player currentPlayer)
    {
        Samurai samurai = currentPlayer.GetTeam().GetSamurai();
        Player targetPlayer = GetOpponent(currentPlayer);

        switch (action)
        {
            case "1":
                string targetName = SelectTarget(samurai, targetPlayer);
                _view.WriteLine(Separator);
                Unit target = FindTarget(targetName, ref currentPlayer);
                SamuraiAttack(ref samurai, target);
                break;
            case "2":
                // Implement Shoot action
                break;
            case "3":
                // Implement Use Skill action
                break;
            case "4":
                // Implement Summon action
                break;
            case "5":
                // Implement Pass Turn action
                break;
            case "6":
                // Implement Surrender action
                break;
        }
    }

    private string SelectTarget(Unit attacker, Player targetPlayer)
    {
        _view.WriteLine($"Seleccione un objetivo para {attacker.GetName()}");
        ShowTargetTeam(targetPlayer);
        return _view.ReadLine();
    }

    private Unit FindTarget(string targetName, ref Player targetPlayer)
    {
        Team team = targetPlayer.GetTeam();
        Unit target = team.GetSamurai().GetName() == targetName ? 
            team.GetSamurai() : team.GetDemons().FirstOrDefault(demon => demon.GetName() == targetName);
        
        _view.WriteLine($"{target.GetName()}");
        return target;
    }

    private void SamuraiAttack(ref Samurai samurai, Unit target)
    {
        // Implement Samurai attack logic
    }
}