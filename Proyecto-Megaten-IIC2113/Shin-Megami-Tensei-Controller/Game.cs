using Shin_Megami_Tensei_View;
using Shin_Megami_Tensei.Gadgets;

namespace Shin_Megami_Tensei;

public class Game
{
    private readonly View _view;
    private readonly string _teamsFolder;
    private readonly int _totalTeams;
    private bool _gameWon;
    private bool _isNewRound; 
    
    private const int SeparatorLinesCount = 40;
    private readonly string Separator = new string('-', SeparatorLinesCount);
    private const double ConstantOfDamage = 0.0114f;
    private const double ModifierPhysDamage = 54;
    private const double ModifierGunDamage = 80;
    
    private Dictionary<string, Player> _players;
    
    private string _teamsContent;
    private List<Unit> player1ActiveUnits;
    private List<Unit> player2ActiveUnits;
    

    public Game(View view, string teamsFolder)
    {
        _view = view;
        _teamsFolder = teamsFolder;
        _totalTeams = Directory.GetFiles(_teamsFolder, "*.txt").Length;
        _gameWon = false;
        _isNewRound = true;
    }

    public void Play()
    {
        PromptUserToSelectFile();
        LoadTeamsFromFile();
        ValidateTeams();
        SetActiveUnitsForBothPlayers();
        if (AreTeamsValid())
        {
            StartCombat();
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

    private void SetActiveUnitsForBothPlayers()
    {
        foreach (Player player in this._players.Values)
        {
            player.SetActiveUnits();
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
        Player winnerPlayer;
        
        while (!_gameWon)
        {
            int playerNumber = currentPlayer.GetName() == "Player 1" ? 1 : 2;
    
            if (_isNewRound)
            { 
                currentPlayer.SetTurns();
                _view.WriteLine(Separator);
                DisplayRoundHeader(currentPlayer, playerNumber);
                currentPlayer.SetOrderOfAttackOfActiveUnits();
                _isNewRound = false;
            }
    
            ShowBoardStatus();
            _view.WriteLine(Separator);
    
            ShowTurns(currentPlayer);
            _view.WriteLine(Separator);
    
            ShowSortedUnits(currentPlayer);
            _view.WriteLine(Separator);
    
            Unit unitToPlay = currentPlayer.GetSortedActiveUnitsByOrderOfAttack()[0];
            if (unitToPlay is Samurai)
            {
                string samuraiAction = GetSamuraiAction(unitToPlay.GetName());
                _view.WriteLine(Separator);
                ManageSamuraiAction(samuraiAction, ref currentPlayer);
            }
            else
            {
                string demonsAction = GetDemonsAction(unitToPlay.GetName());
                _view.WriteLine(Separator);
                ManageDemonsAction(demonsAction, ref currentPlayer);
            }
            
            CheckGameStatus();
    
            if (currentPlayer.GetFullTurns() <= 0 && currentPlayer.GetBlinkingTurns() <= 0)
            {
                currentPlayer = GetOpponent(currentPlayer);
                _isNewRound = true;
            }
        }
    }
    
    private void CheckGameStatus()
    {
        foreach (var player in this._players.Values)
        {
            player.CheckIfTeamIsAbleToContinue();
            if (!player.IsTeamAbleToContinue())
            {
                var winner = GetOpponent(player);
                _view.WriteLine(Separator);
                _view.WriteLine($"Ganador: {winner.GetTeam().GetSamurai().GetName()} (J{(winner.GetName() == "Player 1" ? 1 : 2)})");
                _gameWon = true;
                break;
            }
        }
    }

    private void DisplayWinner(Player player)
    {
        var winner = GetOpponent(player);
        _view.WriteLine(Separator);
        _view.WriteLine($"Ganador: {winner.GetTeam().GetSamurai().GetName()} (J{(winner.GetName() == "Player 1" ? 1 : 2)})");
        _gameWon = true;
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
        _view.WriteLine(Separator);
        foreach (Player player in _players.Values)
        {
            ShowPlayerStatus(player, playerNumber);
            playerNumber++;
        }
    }

    private void ShowPlayerStatus(Player player, int playerNumber)
    {
        _view.WriteLine($"Equipo de {player.GetTeam().GetSamurai().GetName()} (J{playerNumber})");
        ShowActiveUnitsWithLetter(player);
    }

    private void ShowActiveUnitsWithLetter(Player player)
    {
        List<Unit> activeUnits = player.GetActiveUnits();
        char unitLabel = 'A';
    
        for (int i = 0; i < activeUnits.Count; i++)
        {
            if (activeUnits[i] == null)
            {
                _view.WriteLine($"{unitLabel}-");
            }
            else
            {
                DisplayUnitStatus(activeUnits[i], unitLabel);
            }
            unitLabel++;
        }
    }

    private void ShowTargetTeam(Player player)
    {
        List<Unit> activeUnits = player.GetActiveUnits();
        int unitLabelNumber = 1;
    
        foreach (var unit in activeUnits)
        {
            if (unit != null && unit.GetCurrentStats().GetStatByName("HP") > 0)
            {
                _view.WriteLine($"{unitLabelNumber}-{unit.GetName()} " +
                                $"HP:{unit.GetCurrentStats().GetStatByName("HP")}/{unit.GetBaseStats().GetStatByName("HP")} " +
                                $"MP:{unit.GetCurrentStats().GetStatByName("MP")}/{unit.GetBaseStats().GetStatByName("MP")}");
                unitLabelNumber++;
            }
        }
    
        _view.WriteLine($"{unitLabelNumber}-Cancelar");
    }

    private void DisplayUnitStatus(Unit unit, char label)
    {
        Stat baseStats = unit.GetBaseStats();
        Stat currentStats = unit.GetCurrentStats();
        _view.WriteLine($"{label}-{unit.GetName()} HP:{currentStats.GetStatByName("HP")}/{baseStats.GetStatByName("HP")} MP:{currentStats.GetStatByName("MP")}/{baseStats.GetStatByName("MP")}");
    }

    private string GetSamuraiAction(string samuraiName)
    {
        _view.WriteLine($"Seleccione una acción para {samuraiName}");
        string options = "1: Atacar\n2: Disparar\n3: Usar Habilidad\n4: Invocar\n5: Pasar Turno\n6: Rendirse";
        _view.WriteLine(options);
        return _view.ReadLine();
    }
    
    private string GetDemonsAction(string demonName)
    {
        _view.WriteLine($"Seleccione una acción para {demonName}");
        string options = "1: Atacar\n2: Usar Habilidad\n3: Invocar\n4: Pasar Turno";
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
        List<Unit> sortedActiveUnits = currentPlayer.GetSortedActiveUnitsByOrderOfAttack();

        foreach (Unit unit in sortedActiveUnits)
        {
            Console.WriteLine($"{unit}");
        }
        
        if (sortedActiveUnits.Count > 0)
        {
            int unitLabel = 1;
            foreach (Unit unit in sortedActiveUnits)
            {
                _view.WriteLine($"{unitLabel}-{unit.GetName()}");
                unitLabel++;
            }
        }
    }
    
    private Player GetOpponent(Player currentPlayer)
    {
        return currentPlayer.GetName() == "Player 1" ? _players["Player 2"] : _players["Player 1"];
    }

    private void ManageSamuraiAction(string action, ref Player currentPlayer)
    {
        Samurai currentSamurai = currentPlayer.GetTeam().GetSamurai();
        Player targetPlayer = GetOpponent(currentPlayer);
        int initialFullTurns = currentPlayer.GetFullTurns();
        int initialBlinkingTurns = currentPlayer.GetBlinkingTurns();
        string targetInput;
        int fullTurnsConsumed;
        int blinkingTurnsConsumed;
        int blinkingTurnsGained;
        Unit target;

        switch (action)
        {
            case "1":
                targetInput = SelectTarget(currentSamurai, targetPlayer);

                if (targetInput == "cancel")
                {
                    return;
                }
                
                target = FindTarget(Convert.ToInt32(targetInput), ref targetPlayer);
                _view.WriteLine(Separator);
                
                SamuraiAttack(ref currentSamurai, target);
                
                currentPlayer.UpdateTurnsBasedOnAffinity("Phys", target.GetName());
                
                fullTurnsConsumed = initialFullTurns - currentPlayer.GetFullTurns();
                blinkingTurnsConsumed = 0;
                blinkingTurnsGained = 0;
                
                if (initialBlinkingTurns > currentPlayer.GetBlinkingTurns())
                {
                    blinkingTurnsConsumed = initialBlinkingTurns - currentPlayer.GetBlinkingTurns();
                }
                else
                {
                    blinkingTurnsGained = currentPlayer.GetBlinkingTurns() - initialBlinkingTurns;
                }
                
                _view.WriteLine(Separator);
                
                DisplayUpdatesOfTurns(fullTurnsConsumed, blinkingTurnsConsumed);
                DisplayBlinkingTurnsGained(blinkingTurnsGained);
                
                targetPlayer.RemoveFromActiveUnitsIfDead();
                currentPlayer.SortUnitsWhenAnAttackHasBeenMade();
                break;
            
            case "2":
                targetInput = SelectTarget(currentSamurai, targetPlayer);

                if (targetInput == "cancel")
                {
                    return;
                }
                
                target = FindTarget(Convert.ToInt32(targetInput), ref targetPlayer);

                _view.WriteLine(Separator);

                SamuraiShoot(ref currentSamurai, target);
                
                currentPlayer.UpdateTurnsBasedOnAffinity("Gun", target.GetName());
                
                fullTurnsConsumed = initialFullTurns - currentPlayer.GetFullTurns();
                blinkingTurnsConsumed = 0;
                blinkingTurnsGained = 0;
                
                if (initialBlinkingTurns > currentPlayer.GetBlinkingTurns())
                {
                    blinkingTurnsConsumed = initialBlinkingTurns - currentPlayer.GetBlinkingTurns();
                }
                else
                {
                    blinkingTurnsGained = currentPlayer.GetBlinkingTurns() - initialBlinkingTurns;
                }
                
                _view.WriteLine(Separator);
                
                DisplayUpdatesOfTurns(fullTurnsConsumed, blinkingTurnsConsumed);
                DisplayBlinkingTurnsGained(blinkingTurnsGained);
                
                targetPlayer.RemoveFromActiveUnitsIfDead();
                currentPlayer.SortUnitsWhenAnAttackHasBeenMade();
                
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
                _gameWon = true;
                currentPlayer.Surrender();
                int numberOfPlayer = currentPlayer.GetName() == "Player 1" ? 1 : 2;
                _view.WriteLine($"{currentPlayer.GetTeam().GetSamurai().GetName()} (J{numberOfPlayer}) se rinde");
                
                DisplayWinner(currentPlayer);
                break;
        } 
    }

    private string SelectTarget(Unit attacker, Player targetPlayer)
    {
        while (true) 
        {
            _view.WriteLine($"Seleccione un objetivo para {attacker.GetName()}");
            ShowTargetTeam(targetPlayer);

            string input = _view.ReadLine();
            int optionSelected;
        
            if (!int.TryParse(input, out optionSelected))
            {
                continue;
            }

            int cancelOption = targetPlayer.GetActiveUnits().Count + 1;
        
            if (optionSelected == cancelOption)
            {
                return "cancel"; 
            }
            else if (optionSelected >= 1 && optionSelected <= targetPlayer.GetActiveUnits().Count)
            {
                return input; 
            }
        }
    }

    private Unit FindTarget(int selectedOption, ref Player targetPlayer)
    {
        List<Unit> activeUnits = targetPlayer.GetActiveUnits();
        int currentIndex = 0;
        
        for (int i = 0; i < activeUnits.Count; i++)
        {
            Unit unit = activeUnits[i];
            if (unit != null && unit.GetCurrentStats().GetStatByName("HP") > 0)
            {
                currentIndex++;
                if (currentIndex == selectedOption)
                {
                    return unit;
                }
            }
        }
        
        return null; 
    }

    private void SamuraiAttack(ref Samurai samurai, Unit target)
    {
        string targetName = target.GetName();
        string samuraiName = samurai.GetName();
        
        int damageTakenTarget = CalculateDamagePhys(samurai, target);
        
        _view.WriteLine($"{samuraiName} ataca a {targetName}");
        _view.WriteLine($"{targetName} recibe {damageTakenTarget} de daño");
        _view.WriteLine($"{targetName} termina con " +
                        $"HP:{target.GetCurrentStats().GetStatByName("HP")}/{target.GetBaseStats().GetStatByName("HP")}");
    }

    private int CalculateDamagePhys(Unit attacker, Unit target)
    {
        double physDamage = attacker.GetBaseStats().GetStatByName("Str") * ModifierPhysDamage * ConstantOfDamage;
        int damageTaken = Convert.ToInt32(Math.Floor(physDamage));
        
        target.ApplyDamageTaken(damageTaken);
        
        return damageTaken;
    }

    private void SamuraiShoot(ref Samurai samurai, Unit target)
    {
        string targetName = target.GetName();
        string samuraiName = samurai.GetName();
        
        int damageTakenTarget = CalculateDamageGun(samurai, target);
        
        _view.WriteLine($"{samuraiName} dispara a {targetName}");
        _view.WriteLine($"{targetName} recibe {damageTakenTarget} de daño");
        _view.WriteLine($"{targetName} termina con " +
                        $"HP:{target.GetCurrentStats().GetStatByName("HP")}/{target.GetBaseStats().GetStatByName("HP")}");
    }

    private int CalculateDamageGun(Samurai samurai, Unit target)
    {
        double physDamage = samurai.GetBaseStats().GetStatByName("Skl") * ModifierGunDamage * ConstantOfDamage;
        int damageTaken = Convert.ToInt32(Math.Floor(physDamage));
        
        target.ApplyDamageTaken(damageTaken);
        
        return damageTaken;
    }
    
    private void DisplayUpdatesOfTurns(int fullTurnsConsumed, int blinkingTurnsConsumed)
    {
        _view.WriteLine($"Se han consumido {fullTurnsConsumed} Full Turn(s) " +
                        $"y {blinkingTurnsConsumed} Blinking Turn(s)");
    }

    private void DisplayBlinkingTurnsGained(int blinkingTurnsGained)
    {
        _view.WriteLine($"Se han obtenido {blinkingTurnsGained} Blinking Turn(s)");
    }

    private void ManageDemonsAction(string action, ref Player currentPlayer)
    {
        Unit currentDemon = currentPlayer.GetSortedActiveUnitsByOrderOfAttack()[0];
        Player targetPlayer = GetOpponent(currentPlayer);
        int initialFullTurns = currentPlayer.GetFullTurns();
        int initialBlinkingTurns = currentPlayer.GetBlinkingTurns();
        string targetInput;
        int fullTurnsConsumed;
        int blinkingTurnsConsumed;
        int blinkingTurnsGained;
        Unit target;

        switch (action)
        {
            case "1":
                targetInput = SelectTarget(currentDemon, targetPlayer);
                
                if (targetInput == "cancel")
                {
                    return;
                }
                
                target = FindTarget(Convert.ToInt32(targetInput), ref targetPlayer);
                _view.WriteLine(Separator);

                DemonAttack(ref currentDemon, target);
                
                currentPlayer.UpdateTurnsBasedOnAffinity("Phys", target.GetName());
                
                fullTurnsConsumed = initialFullTurns - currentPlayer.GetFullTurns();
                blinkingTurnsConsumed = 0;
                blinkingTurnsGained = 0;
                
                if (initialBlinkingTurns > currentPlayer.GetBlinkingTurns())
                {
                    blinkingTurnsConsumed = initialBlinkingTurns - currentPlayer.GetBlinkingTurns();
                }
                else
                {
                    blinkingTurnsGained = currentPlayer.GetBlinkingTurns() - initialBlinkingTurns;
                }
                
                _view.WriteLine(Separator);
                
                DisplayUpdatesOfTurns(fullTurnsConsumed, blinkingTurnsConsumed);
                DisplayBlinkingTurnsGained(blinkingTurnsGained);
                
                targetPlayer.RemoveFromActiveUnitsIfDead();
                currentPlayer.SortUnitsWhenAnAttackHasBeenMade();
                
                break;
        }
    }

    private void DemonAttack(ref Unit demon, Unit target)
    {
        string targetName = target.GetName();
        string samuraiName = demon.GetName();
        
        int damageTakenTarget = CalculateDamagePhys(demon, target);
        
        _view.WriteLine($"{samuraiName} ataca a {targetName}");
        _view.WriteLine($"{targetName} recibe {damageTakenTarget} de daño");
        _view.WriteLine($"{targetName} termina con " +
                        $"HP:{target.GetCurrentStats().GetStatByName("HP")}/{target.GetBaseStats().GetStatByName("HP")}");
    }
}
