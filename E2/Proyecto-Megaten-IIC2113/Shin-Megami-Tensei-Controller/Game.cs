using System.Net.Sockets;
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
    private bool _surrender; 
    private const string Const_Cancel = "Cancelar";
    
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
        _surrender = false;
    }

    public void Play()
    {
        PromptUserToSelectFile();
        LoadTeamsFromFile();
        ValidateTeams();
        SetActiveUnitsForBothPlayers();
        
        if (AreTeamsValid())
        {
            _view.WriteLine(Separator);
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
            string fileNumber = (i + 1).ToString("D3");
            _view.WriteLine($"{i}: {fileNumber}.txt");
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
        if (fileNumber < 9)
            return $"{_teamsFolder}/00{fileNumber + 1}.txt";
        else if (fileNumber < 99)
            return $"{_teamsFolder}/0{fileNumber + 1}.txt";
        else
            return $"{_teamsFolder}/{fileNumber + 1}.txt";
    }

    private void SeparateTeams()
    {
        List<string> teamContentLines = GetNonEmptyLines(_teamsContent);
        InitializePlayers();
        
        ParseTeamContentLines(teamContentLines);
    }
    
    private void ParseTeamContentLines(List<string> contentLines)
    {
        Player currentTeamOwner = null;
        List<string> currentUnitDefinitions = new List<string>();
        
        foreach (string line in contentLines)
        {
            ProcessContentLine(line, ref currentTeamOwner, currentUnitDefinitions);
        }
        
        SaveFinalTeamIfPlayerExists(currentTeamOwner, currentUnitDefinitions);
    }
    
    private void ProcessContentLine(string line, ref Player currentTeamOwner, List<string> unitDefinitions)
    {
        string playerIdentifier = GetPlayerIdentifierFromHeaderLine(line);
        
        if (playerIdentifier != null)
        {
            SaveTeamForCurrentPlayerAndSwitchToNewPlayer(ref currentTeamOwner, unitDefinitions, playerIdentifier);
        }
        else
        {
            AddUnitToCurrentTeam(line, unitDefinitions);
        }
    }
    
    private string GetPlayerIdentifierFromHeaderLine(string line)
    {
        if (IsTeamHeaderForPlayer(line, "Player 1 Team"))
        {
            return "Player 1";
        }
        
        if (IsTeamHeaderForPlayer(line, "Player 2 Team"))
        {
            return "Player 2";
        }
        
        return null;
    }
    
    private void AddUnitToCurrentTeam(string unitDefinition, List<string> unitDefinitions)
    {
        unitDefinitions.Add(unitDefinition);
    }
    
    private void SaveTeamForCurrentPlayerAndSwitchToNewPlayer(ref Player currentPlayer, List<string> teamUnits, string playerIdentifier)
    {
        if (currentPlayer != null)
        {
            currentPlayer.SetTeam(ConvertStringToTeam(teamUnits));
            teamUnits.Clear();
        }
        
        currentPlayer = _players[playerIdentifier];
    }
    
    private void SaveFinalTeamIfPlayerExists(Player player, List<string> teamUnits)
    {
        if (player != null)
        {
            player.SetTeam(ConvertStringToTeam(teamUnits));
        }
    }
    
    private bool IsTeamHeaderForPlayer(string line, string playerTeamHeader)
    {
        return line.StartsWith(playerTeamHeader);
    }

    private Team ConvertStringToTeam(List<string> teamUnitDescriptions)
    {
        Team newTeam = new Team();
        foreach (var unitDescription in teamUnitDescriptions)
        {
            if (IsSamuraiUnit(unitDescription))
            {
                TryAddSamuraiToTeam(newTeam, unitDescription);
            }
            else
            {
                AddDemonToTeam(newTeam, unitDescription);
            }
        }
    
        return newTeam;
    }
    
    private bool IsSamuraiUnit(string unitDescription)
    {
        return unitDescription.StartsWith("[Samurai]");
    }
    
    private void TryAddSamuraiToTeam(Team team, string samuraiDescription)
    {
        if (team.HasSamurai())
        {
            team.SetSamuraiRepeated();
            return;
        }
        
        var (samuraiName, samuraiSkills) = ExtractSamuraiDetails(samuraiDescription);
        Samurai newSamurai = CreateSamuraiWithSkills(samuraiName, samuraiSkills);
        team.AddSamurai(newSamurai);
    }
    
    private Samurai CreateSamuraiWithSkills(string name, List<string> skills)
    {
        Samurai samurai = new Samurai(name);
        SetUpSamurai(ref samurai, skills);
        return samurai;
    }

    private void AddDemonToTeam(Team newTeam, string unit)
    {
        string demonName = unit.Trim();
        Demon demon = new Demon(demonName);
        demon.SetStatsFromJSON();
        demon.SetDemonSkillsFromJSON();
        newTeam.AddDemon(demon);
    }

    private (string SamuraiName, List<string> Skills) ExtractSamuraiDetails(string unitDescription)
    {
        const string SamuraiPrefix = "[Samurai]";
        List<string> samuraiSkills = new List<string>();
        int nameStartPosition = SamuraiPrefix.Length;
        int openParenthesisPosition = unitDescription.IndexOf('(');
        string extractedName;
        
        if (HasSkillsList(openParenthesisPosition))
        {
            extractedName = ExtractNameWithSkills(unitDescription, nameStartPosition, openParenthesisPosition);
            samuraiSkills = ExtractSkillsList(unitDescription, openParenthesisPosition);
        }
        else
        {
            extractedName = ExtractNameWithoutSkills(unitDescription, nameStartPosition);
        }
    
        return (extractedName, samuraiSkills);
    }
    
    private bool HasSkillsList(int parenthesisPosition)
    {
        return parenthesisPosition != -1;
    }
    
    private string ExtractNameWithSkills(string text, int startPosition, int endPosition)
    {
        return text.Substring(startPosition, endPosition - startPosition).Trim();
    }
    
    private string ExtractNameWithoutSkills(string text, int startPosition)
    {
        return text.Substring(startPosition).Trim();
    }
    
    private List<string> ExtractSkillsList(string text, int openParenthesisPosition)
    {
        int closeParenthesisPosition = text.IndexOf(')', openParenthesisPosition);
        if (closeParenthesisPosition == -1)
        {
            return new List<string>();
        }
        
        string skillsText = text.Substring(openParenthesisPosition + 1, 
                                          closeParenthesisPosition - openParenthesisPosition - 1);
        
        return skillsText.Split(',')
                        .Select(skill => skill.Trim())
                        .ToList();
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
        InitializeCombat();
        PlayCombatRounds();
    }
    
    private void InitializeCombat()
    {
        SetPlayerTurns();
        Player currentPlayer = _players["Player 1"];
    }
    
    private void PlayCombatRounds()
    {
        Player currentPlayer = _players["Player 1"];
        
        while (!_gameWon)
        {
            HandlePlayerRound(ref currentPlayer);
            
            if (ShouldSwitchPlayer(currentPlayer))
            {
                currentPlayer = GetOpponent(currentPlayer);
                _isNewRound = true;
            }
        }
    }
    
    private void HandlePlayerRound(ref Player currentPlayer)
    {
        int playerNumber = GetPlayerNumber(currentPlayer);
    
        InitializeRoundIfNeeded(currentPlayer, playerNumber);
        DisplayCombatInfo(currentPlayer);
        ExecuteUnitAction(currentPlayer);
        CheckGameStatus();
    }
    
    private int GetPlayerNumber(Player player)
    {
        return player.GetName() == "Player 1" ? 1 : 2;
    }
    
    private void InitializeRoundIfNeeded(Player currentPlayer, int playerNumber)
    {
        if (_isNewRound)
        { 
            currentPlayer.SetTurns();
            DisplayRoundHeader(currentPlayer, playerNumber);
            currentPlayer.SetOrderOfAttackOfActiveUnits();
            _isNewRound = false;
            _view.WriteLine(Separator);     
        }
    }
    
    private void DisplayCombatInfo(Player currentPlayer)
    {
        ShowBoardStatus();
        _view.WriteLine(Separator);
        ShowTurns(currentPlayer);
        _view.WriteLine(Separator);
        ShowSortedUnits(currentPlayer);
        _view.WriteLine(Separator);
    }
    
    private void ExecuteUnitAction(Player currentPlayer)
    {
        Unit unitToPlay = GetUnitToPlay(currentPlayer);
        ExecuteActionForActiveUnit(currentPlayer, unitToPlay);
    }
    
    private bool ShouldSwitchPlayer(Player currentPlayer)
    {
        return currentPlayer.GetFullTurns() <= 0 && currentPlayer.GetBlinkingTurns() <= 0;
    }

    private void ExecuteActionForActiveUnit(Player currentPlayer, Unit activeUnit)
    {
        bool isActionSuccessful = false;
        
        while (!isActionSuccessful)
        {
            string selectedAction = SelectActionBasedOnUnitType(activeUnit);
            _view.WriteLine(Separator);
            
            isActionSuccessful = ProcessUnitAction(currentPlayer, activeUnit, selectedAction);
            
            if (!_surrender) 
            {
                _view.WriteLine(Separator);
            }
        }
    }
    
    private string SelectActionBasedOnUnitType(Unit unit)
    {
        if (unit is Samurai)
        {
            return GetSamuraiAction(unit.GetName());
        }
        
        return GetDemonsAction(unit.GetName());
    }
    
    private bool ProcessUnitAction(Player currentPlayer, Unit unit, string selectedAction)
    {
        if (unit is Samurai)
        {
            string result = ManageSamuraiAction(selectedAction, ref currentPlayer);
            return (result != Const_Cancel);
        }
        
        string demonResult = ManageDemonsAction(selectedAction, ref currentPlayer);
        return (demonResult != Const_Cancel);
    }

    private Unit GetUnitToPlay(Player currentPlayer)
    {
        var sortedUnits = currentPlayer.GetSortedActiveUnitsByOrderOfAttack();
        Unit unitToPlay = sortedUnits[0];

        return unitToPlay;
    }
    
    private void CheckGameStatus()
    {
        foreach (var player in this._players.Values)
        {
            player.CheckIfTeamIsAbleToContinue();
            if (!player.IsTeamAbleToContinue())
            {
                var winner = GetOpponent(player);
                DisplayWinner(winner);
                break;
            }
        }
    }

    private void DisplayWinner(Player winner)
    {
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
            if (IsValidUnit(unit))
            {
                DisplayUnitStatus(unit, unitLabelNumber);
                unitLabelNumber++;
            }
        }
    
        _view.WriteLine($"{unitLabelNumber}-Cancelar");
    }
    
    private bool IsValidUnit(Unit unit)
    {
        return unit != null && unit.GetCurrentStats().GetStatByName("HP") > 0;
    }
    
    private void DisplayUnitStatus(Unit unit, int unitLabelNumber)
    {
        _view.WriteLine($"{unitLabelNumber}-{unit.GetName()} " +
                        $"HP:{unit.GetCurrentStats().GetStatByName("HP")}/{unit.GetBaseStats().GetStatByName("HP")} " +
                        $"MP:{unit.GetCurrentStats().GetStatByName("MP")}/{unit.GetBaseStats().GetStatByName("MP")}");
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

    private string ManageSamuraiAction(string action, ref Player currentPlayer)
    {
        Samurai currentSamurai = currentPlayer.GetTeam().GetSamurai();
        Player opponentPlayer = GetOpponent(currentPlayer);
        int initialFullTurns = currentPlayer.GetFullTurns();
        int initialBlinkingTurns = currentPlayer.GetBlinkingTurns();
    
        switch (action)
        {
            case "1":
                return HandlePhysicalAttack(ref currentPlayer, currentSamurai, opponentPlayer, initialFullTurns, initialBlinkingTurns);
            
            case "2":
                return HandleGunAttack(ref currentPlayer, currentSamurai, opponentPlayer, initialFullTurns, initialBlinkingTurns);
            
            case "3":
                return HandleUseSkill(ref currentPlayer, currentSamurai, opponentPlayer, initialFullTurns, initialBlinkingTurns);
            
            case "4":
                break;
            
            case "5":
                break;
                
            case "6":
                HandleSurrender(currentPlayer);
                return "";
        }
    
        return "";
    }
    
    private string HandlePhysicalAttack(ref Player currentPlayer, Samurai currentSamurai, Player opponentPlayer, int initialFullTurns, int initialBlinkingTurns)
    {
        string targetInput = SelectTarget(currentSamurai, opponentPlayer);
        
        if (IsTargetSelectionCancelled(targetInput, opponentPlayer))
        {
            return Const_Cancel;
        }
        
        _view.WriteLine(Separator);
        Unit target = FindTarget(Convert.ToInt32(targetInput), ref opponentPlayer);
        
        SamuraiAttack(ref currentSamurai, target);
        _view.WriteLine(Separator);
        
        currentPlayer.UpdateTurnsBasedOnAffinity("Phys", target.GetName());
        
        UpdateTurnsAndApplyEffects(ref currentPlayer, opponentPlayer, initialFullTurns, initialBlinkingTurns, targetInput);
        
        return "";
    }
    
    private string HandleGunAttack(ref Player currentPlayer, Samurai currentSamurai, Player opponentPlayer, int initialFullTurns, int initialBlinkingTurns)
    {
        string targetInput = SelectTarget(currentSamurai, opponentPlayer);
        
        if (IsTargetSelectionCancelled(targetInput, opponentPlayer))
        {
            return Const_Cancel;
        }
        
        _view.WriteLine(Separator);
        Unit target = FindTarget(Convert.ToInt32(targetInput), ref opponentPlayer);
        
        SamuraiShoot(ref currentSamurai, target);
        _view.WriteLine(Separator);
        
        currentPlayer.UpdateTurnsBasedOnAffinity("Gun", target.GetName());
        
        int fullTurnsConsumed = initialFullTurns - currentPlayer.GetFullTurns();
        int blinkingTurnsConsumed = 0;
        int blinkingTurnsGained = 0;
        
        if (initialBlinkingTurns > currentPlayer.GetBlinkingTurns())
        {
            blinkingTurnsConsumed = initialBlinkingTurns - currentPlayer.GetBlinkingTurns();
        }
        else
        {
            blinkingTurnsGained = currentPlayer.GetBlinkingTurns() - initialBlinkingTurns;
        }
        
        DisplayUpdatesOfTurns(fullTurnsConsumed, blinkingTurnsConsumed);
        DisplayBlinkingTurnsGained(blinkingTurnsGained);
        
        opponentPlayer.RemoveFromActiveUnitsIfDead();
        currentPlayer.SortUnitsWhenAnAttackHasBeenMade();
        
        return "";
    }
    
    private string HandleUseSkill(ref Player currentPlayer, Samurai currentSamurai, Player opponentPlayer, int initialFullTurns, int initialBlinkingTurns)
    {
        Skill selectedSkill = SelectSamuraiSkill(currentSamurai);
        if (selectedSkill == null)
        {
            return Const_Cancel;
        }
        
        Unit target = SelectSkillTarget(currentSamurai, opponentPlayer);
        if (target == null)
        {
            return Const_Cancel;
        }
        
        ProcessTurnUpdates(ref currentPlayer, ref opponentPlayer, initialFullTurns, initialBlinkingTurns);
        
        return "";
    }
    
    private Skill SelectSamuraiSkill(Samurai samurai)
    {
        _view.WriteLine($"Seleccione una habilidad para que {samurai.GetName()} use");
        ShowSkillsSamurai(samurai);
        string skillInput = SelectSkillToUse();
        
        int skillIndex = Convert.ToInt32(skillInput) - 1;
        if (skillIndex >= samurai.GetSkills().Count || 
            samurai.GetSkills()[skillIndex].GetCost() > samurai.GetCurrentStats().GetStatByName("MP"))
        {
            return null;
        }
        
        return samurai.GetSkills()[skillIndex];
    }
    
    private Unit SelectSkillTarget(Samurai attacker, Player targetPlayer)
    {
        string targetInput = SelectTarget(attacker, targetPlayer);
        
        if (Convert.ToInt32(targetInput) > targetPlayer.GetValidUnits().Count)
        {
            return null;
        }
        
        _view.WriteLine(Separator);
        return FindTarget(Convert.ToInt32(targetInput), ref targetPlayer);
    }
    
    private void ProcessTurnUpdates(ref Player currentPlayer, ref Player opponentPlayer, int initialFullTurns, int initialBlinkingTurns)
    {
        int currentBlinkingTurns = currentPlayer.GetBlinkingTurns();
        int fullTurnsConsumed = initialFullTurns - currentPlayer.GetFullTurns();
        int blinkingTurnsConsumed = initialBlinkingTurns - currentBlinkingTurns;
        int blinkingTurnsGained = 0;
        
        if (currentBlinkingTurns > initialBlinkingTurns)
        {
            blinkingTurnsGained = currentBlinkingTurns - initialBlinkingTurns;
            blinkingTurnsConsumed = 0;
        }
        
        DisplayTurnChanges(fullTurnsConsumed, blinkingTurnsConsumed, blinkingTurnsGained);
        UpdateTeamsAfterAction(ref currentPlayer, ref opponentPlayer);
    }
    
    private void UpdateTeamsAfterAction(ref Player attacker, ref Player defender)
    {
        defender.RemoveFromActiveUnitsIfDead();
        attacker.SortUnitsWhenAnAttackHasBeenMade();
    }
    
    private void HandleSurrender(Player currentPlayer)
    {
        _gameWon = true;
        currentPlayer.Surrender();
        Player winner = GetOpponent(currentPlayer);
        int playerNumber = currentPlayer.GetName() == "Player 1" ? 1 : 2;
        _view.WriteLine($"{currentPlayer.GetTeam().GetSamurai().GetName()} (J{playerNumber}) se rinde");
        _view.WriteLine(Separator);
        _surrender = true;
        
        DisplayWinner(winner);
    }
    
    private bool IsTargetSelectionCancelled(string targetInput, Player opponentPlayer)
    {
        return Convert.ToInt32(targetInput) > opponentPlayer.GetValidUnits().Count;
    }
    
    private void UpdateTurnsAndApplyEffects(ref Player currentPlayer, Player opponentPlayer, int initialFullTurns, int initialBlinkingTurns, string targetInput)
    {
        int fullTurnsConsumed = initialFullTurns - currentPlayer.GetFullTurns();
        int blinkingTurnsConsumed = 0;
        int blinkingTurnsGained = 0;
        
        if (Convert.ToInt32(targetInput) > opponentPlayer.GetActiveUnits().Where(unit => unit != null).Count())
        {
            blinkingTurnsConsumed = initialBlinkingTurns - currentPlayer.GetBlinkingTurns();
        }
        else
        {
            blinkingTurnsGained = currentPlayer.GetBlinkingTurns() - initialBlinkingTurns;
        }
        
        DisplayUpdatesOfTurns(fullTurnsConsumed, blinkingTurnsConsumed);
        DisplayBlinkingTurnsGained(blinkingTurnsGained);
        
        opponentPlayer.RemoveFromActiveUnitsIfDead();
        currentPlayer.SortUnitsWhenAnAttackHasBeenMade();
    }
    
    private string SelectTarget(Unit attacker, Player targetPlayer)
    {
        _view.WriteLine($"Seleccione un objetivo para {attacker.GetName()}");
        ShowTargetTeam(targetPlayer);

        string input = _view.ReadLine();

        return input;
    }

    private Unit FindTarget(int targetOption, ref Player targetPlayer)
    {
        List<Unit> activeUnits = targetPlayer.GetActiveUnits();
        int validUnitCount = 0;
        
        foreach (Unit unit in activeUnits)
        {
            if (IsValidTargetUnit(unit))
            {
                validUnitCount++;
                if (validUnitCount == targetOption)
                {
                    return unit;
                }
            }
        }
        
        return null;
    }
    
    private bool IsValidTargetUnit(Unit unit)
    {
        return unit != null && HasRemainingHealth(unit);
    }
    
    private bool HasRemainingHealth(Unit unit)
    {
        return unit.GetCurrentStats().GetStatByName("HP") > 0;
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

    private string ManageDemonsAction(string action, ref Player currentPlayer)
    {
        Unit currentDemon = currentPlayer.GetSortedActiveUnitsByOrderOfAttack()[0];
        Player opponentPlayer = GetOpponent(currentPlayer);
        int initialFullTurns = currentPlayer.GetFullTurns();
        int initialBlinkingTurns = currentPlayer.GetBlinkingTurns();
    
        switch (action)
        {
            case "1":
                return HandleDemonPhysicalAttack(ref currentPlayer, ref currentDemon, opponentPlayer, initialFullTurns, initialBlinkingTurns);
            case "2":
                return HandleDemonUseSkill(ref currentPlayer, currentDemon, opponentPlayer, initialFullTurns, initialBlinkingTurns);
            case "3":
                break;
            case "4":
                break;
        }
    
        return "";
    }
    
    private string HandleDemonPhysicalAttack(ref Player currentPlayer, ref Unit currentDemon, Player opponentPlayer, int initialFullTurns, int initialBlinkingTurns)
    {
        string targetInput = SelectTarget(currentDemon, opponentPlayer);
        
        if (IsTargetSelectionCancelled(targetInput, opponentPlayer))
        {
            return Const_Cancel;
        }
        
        _view.WriteLine(Separator);
        Unit target = FindTarget(Convert.ToInt32(targetInput), ref opponentPlayer);
    
        DemonAttack(ref currentDemon, target);
        _view.WriteLine(Separator);
        
        currentPlayer.UpdateTurnsBasedOnAffinity("Phys", target.GetName());
        
        UpdateTurnsAfterAction(ref currentPlayer, ref opponentPlayer, initialFullTurns, initialBlinkingTurns);
        
        return "";
    }
    
    private string HandleDemonUseSkill(ref Player currentPlayer, Unit currentDemon, Player opponentPlayer, int initialFullTurns, int initialBlinkingTurns)
    {
        _view.WriteLine($"Seleccione una habilidad para que {currentDemon.GetName()} use");
        string skillInput = SelectSkillToUse();
    
        if (IsDemonSkillSelectionInvalid(skillInput, currentDemon))
        {
            return Const_Cancel;
        }
        
        return "";
    }
    
    private bool IsDemonSkillSelectionInvalid(string skillInput, Unit currentDemon)
    {
        int skillIndex = Convert.ToInt32(skillInput) - 1;
        return skillIndex >= currentDemon.GetSkills().Count || 
               currentDemon.GetSkills()[skillIndex].GetCost() > currentDemon.GetCurrentStats().GetStatByName("MP");
    }

    private void UpdateTurnsAfterAction(ref Player currentPlayer, ref Player opponentPlayer, int initialFullTurns, int initialBlinkingTurns)
    {
        int fullTurnsConsumed = initialFullTurns - currentPlayer.GetFullTurns();
        int blinkingTurnsConsumed = 0;
        int blinkingTurnsGained = 0;
        
        CalculateTurnChanges(currentPlayer, initialBlinkingTurns, ref blinkingTurnsConsumed, ref blinkingTurnsGained);
        
        DisplayTurnChanges(fullTurnsConsumed, blinkingTurnsConsumed, blinkingTurnsGained);
        UpdateTeamsState(ref currentPlayer, ref opponentPlayer);
    }
    
    private void CalculateTurnChanges(Player currentPlayer, int initialBlinkingTurns, ref int blinkingTurnsConsumed, ref int blinkingTurnsGained)
    {
        int currentBlinkingTurns = currentPlayer.GetBlinkingTurns();
        
        if (initialBlinkingTurns > currentBlinkingTurns)
        {
            blinkingTurnsConsumed = initialBlinkingTurns - currentBlinkingTurns;
        }
        else
        {
            blinkingTurnsGained = currentBlinkingTurns - initialBlinkingTurns;
        }
    }
    
    private void DisplayTurnChanges(int fullTurnsConsumed, int blinkingTurnsConsumed, int blinkingTurnsGained)
    {
        DisplayUpdatesOfTurns(fullTurnsConsumed, blinkingTurnsConsumed);
        DisplayBlinkingTurnsGained(blinkingTurnsGained);
    }
    
    private void UpdateTeamsState(ref Player attacker, ref Player defender)
    {
        defender.RemoveFromActiveUnitsIfDead();
        attacker.SortUnitsWhenAnAttackHasBeenMade();
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

    private void ShowSkillsSamurai(Samurai currentSamurai)
    {
        int iterador = 1;
        foreach (var skill in currentSamurai.GetSkills())
        {
            if (skill.GetCost() <= currentSamurai.GetCurrentStats().GetStatByName("MP"))
            {
                _view.WriteLine($"{iterador}-{skill.GetName()} MP:{skill.GetCost()}");
                iterador++;
            }
        }
        
        _view.WriteLine($"{iterador}-Cancelar");
    }

    private string SelectSkillToUse()
    {
        string optionSelected = _view.ReadLine();
        return optionSelected;
    }
}
