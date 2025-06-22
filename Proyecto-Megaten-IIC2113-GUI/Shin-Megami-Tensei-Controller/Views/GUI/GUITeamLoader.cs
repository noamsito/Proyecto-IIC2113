using Shin_Megami_Tensei_GUI;
using Shin_Megami_Tensei;

public class GUITeamLoader : ITeamLoader
    {
        private const int MAX_SKILLS_PER_SAMURAI = 8;

        public (PlayerTeamInfo Team1, PlayerTeamInfo Team2) LoadTeamInformation(SMTGUI gui)
        {
            var team1Info = gui.GetTeamInfo(1);
            var team2Info = gui.GetTeamInfo(2);

            return (
                ConvertToPlayerTeamInfo(team1Info),
                ConvertToPlayerTeamInfo(team2Info)
            );
        }

        public Dictionary<string, Player> CreatePlayersFromTeamInfo((PlayerTeamInfo Team1, PlayerTeamInfo Team2) teamInfos)
        {
            try
            {
                var player1 = CreatePlayerFromTeamInfo(teamInfos.Team1, "Player 1");
                var player2 = CreatePlayerFromTeamInfo(teamInfos.Team2, "Player 2");

                return new Dictionary<string, Player>
                {
                    ["Player 1"] = player1,
                    ["Player 2"] = player2
                };
            }
            catch (Exception)
            {
                return CreateInvalidTeams();
            }
        }

        private PlayerTeamInfo ConvertToPlayerTeamInfo(ITeamInfo teamInfo)
        {
            return new PlayerTeamInfo(
                teamInfo.SamuraiName,
                teamInfo.SkillNames.ToList(),
                teamInfo.DemonNames.ToList()
            );
        }

        private Player CreatePlayerFromTeamInfo(PlayerTeamInfo teamInfo, string playerName)
        {
            ValidateTeamInfo(teamInfo);

            var samurai = new Samurai(teamInfo.SamuraiName, teamInfo.SkillNames);
            var demons = CreateDemons(teamInfo.DemonNames);
            var team = CreateTeam(samurai, demons);

            var player = new Player(playerName);
            player.SetTeam(team);
            
            return player;
        }

        private void ValidateTeamInfo(PlayerTeamInfo teamInfo)
        {
            if (teamInfo.SkillNames.Count > MAX_SKILLS_PER_SAMURAI)
            {
                throw new InvalidOperationException($"Too many skills: {teamInfo.SkillNames.Count}");
            }

            var uniqueSkills = new HashSet<string>(teamInfo.SkillNames.Select(s => s.ToLower().Trim()));
            if (uniqueSkills.Count != teamInfo.SkillNames.Count)
            {
                throw new InvalidOperationException("Duplicate skills found");
            }
        }

        private List<Demon> CreateDemons(List<string> demonNames)
        {
            return demonNames.Select(name => new Demon(name)).ToList();
        }

        private Team CreateTeam(Samurai samurai, List<Demon> demons)
        {
            var team = new Team();
            team.AddSamurai(samurai);
            
            foreach (var demon in demons)
            {
                team.AddDemon(demon);
            }
            
            return team;
        }

        private Dictionary<string, Player> CreateInvalidTeams()
        {
            var invalidTeam = new Team();
            invalidTeam.SetTeamAsInvalid();

            return new Dictionary<string, Player>
            {
                ["Player 1"] = CreatePlayerWithTeam("Player 1", invalidTeam),
                ["Player 2"] = CreatePlayerWithTeam("Player 2", invalidTeam)
            };
        }

        private Player CreatePlayerWithTeam(string playerName, Team team)
        {
            var player = new Player(playerName);
            player.SetTeam(team);
            return player;
        }
    }
