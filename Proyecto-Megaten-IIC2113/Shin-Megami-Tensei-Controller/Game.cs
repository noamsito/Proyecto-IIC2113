using System.Reflection.Metadata.Ecma335;
    using Shin_Megami_Tensei_View;
    using Shin_Megami_Tensei.Gadgets;
    
    namespace Shin_Megami_Tensei;
    
    public class Game
    {
        View _view;
        
        readonly string _teamsFolder; 
        readonly int NUMBER_TOTAL_TEAMS;
        readonly int NUMBER_OF_SEPARATOR_LINES;
        readonly string CONST_OF_SEPARATORS;
    
        private Dictionary<string, Player> _players;
        
        string content_teams_folder;
        
        public Game(View view, string teamsFolder)
        {
            this._teamsFolder = teamsFolder;
            _view = view;

            NUMBER_TOTAL_TEAMS = Directory.GetFiles(_teamsFolder, "*.txt").Length;
            
            NUMBER_OF_SEPARATOR_LINES = 40;
            CONST_OF_SEPARATORS = new string('-', NUMBER_OF_SEPARATOR_LINES);
        }
    
        public void Play()
        {
            _view.WriteLine("Elige un archivo para cargar los equipos");
            for (int i = 0; i < NUMBER_TOTAL_TEAMS; i++)
            {
                _view.WriteLine(i < 9 ? $"{i}: 00{i + 1}.txt" : $"{i}: 0{i + 1}.txt");
            }
            
            string numberOfFileString = _view.ReadLine();
            
            this.AssignFileNameOfContents(int.Parse(numberOfFileString));
            SeparateTeamsOfPlayers();
            
            if (!this._players["Player 1"].IsTeamValid() && !this._players["Player 2"].IsTeamValid())
            {
                _view.WriteLine("Archivo de equipos inválido");
            }
            else
            {
                this.CombatBetweenPlayers();
            }
        }
    
        public void AssignFileNameOfContents(int numberOfFile)
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
        }
    
        private void SeparateTeamsOfPlayers()
        {
            List<string> lines = GetNonEmptyLines(content_teams_folder);
            this.InitializePlayers();
        
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
                currentPlayer.SetTeam(ConvertStringIntoTeam(teamUnits));
            }
        }
    
        private void AssignTeamToPlayer(ref Player currentPlayer, List<string> teamUnits, string playerName)
        {
            if (currentPlayer != null)
            {
                currentPlayer.SetTeam(ConvertStringIntoTeam(teamUnits));
                teamUnits.Clear();
            }
            currentPlayer = _players[playerName];
        }
    
        private Team ConvertStringIntoTeam(List<string> teamUnits)
        {
            Team newTeam = new Team();
            foreach (var unit in teamUnits)
            {
                if (unit.StartsWith("[Samurai]"))
                {
                    this.AddSamuraiToTeam(newTeam, unit);
                }
                else
                {
                    this.AddDemonToTeam(newTeam, unit);
                }
            }
            return newTeam;
        }
    
        private void AddSamuraiToTeam(Team newTeam, string unit)
        {
            if (!newTeam.HasSamurai())
            {
                string samuraiName;
                List<string> ListOfSkillsSamurai;
                      
                (samuraiName, ListOfSkillsSamurai) = this.ExtractSkillsAndSamuraiNames(unit);
                      
                Samurai NewSamurai = new Samurai(samuraiName);
                this.SetUpSamurai(NewSamurai, ListOfSkillsSamurai);
                      
                newTeam.AddSamurai(NewSamurai);
            }
            else
            {
                newTeam.SetTeamAsInvalid();
            }
        }
    
        private void AddDemonToTeam(Team newTeam, string unit)
        {
            string demonName = unit.Trim(); 
            newTeam.AddDemon(new Demon(demonName));
        }
    
        private (string, List<string>) ExtractSkillsAndSamuraiNames(string unit)
        {
            List<string> skillsNames = new List<string>();
            
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
                
                skillsNames = skillsString.Split(',')
                                            .Select(word => word.Trim())
                                            .ToList();
                samuraiName = unit.Substring(nameStart, parenthesisStart - nameStart).Trim();
            }
            else
            {
                samuraiName = unit.Substring(nameStart).Trim();
            }
            
            return (samuraiName, skillsNames);
        }
    
        private Samurai SetUpSamurai(Samurai samurai, List<string> ListOfSkillsSamurai)
        {
            samurai.SetStatsFromJSON();
            samurai.SetSkillsFromJSON(ListOfSkillsSamurai);
            return samurai;
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
            this._players = new Dictionary<string, Player>
            {
                { "Player 1", new Player("Player 1") },
                { "Player 2", new Player("Player 2") }
            };
        }
    
        private bool IsPlayerTeamLine(string line, string playerTeam)
        {
            return line.StartsWith(playerTeam);
        }
    
        private void PrintTeams(Dictionary<string, Player> players)
        {
            foreach (var player in players)
            {
                _view.WriteLine($"{player.Key}'s Team:");
                Team team = player.Value.Team;
    
                if (team.HasSamurai())
                {
                    Samurai samurai = team.GetSamurai();
                    _view.WriteLine($"Samurai: {samurai.GetName()}");
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

        private void CombatBetweenPlayers()
        {
            Samurai samuraiPlayer1 = this._players["Player 1"].Team.GetSamurai();
            _view.WriteLine($"Ronda de {samuraiPlayer1.GetName()}");
            
            _view.WriteLine(CONST_OF_SEPARATORS);

            this.ShowStatusOfBoard();
        }
        
        private void ShowStatusOfBoard()
        {
            this.ShowStatusPlayer1();
            this.ShowStatusPlayer2();
        }
        
        private void ShowStatusPlayer1()
        {
            Player player1 = this._players["Player 1"];
            Samurai samuraiPlayer1 = player1.Team.GetSamurai();
            
            _view.WriteLine($"Ronda de {samuraiPlayer1.GetName()}");

            this.ShowTeamPlayer(ref player1);
        }

        private void ShowStatusPlayer2()
        {
            
        }

        private void ShowTeamPlayer(ref Player player)
        {
               
        }
    }