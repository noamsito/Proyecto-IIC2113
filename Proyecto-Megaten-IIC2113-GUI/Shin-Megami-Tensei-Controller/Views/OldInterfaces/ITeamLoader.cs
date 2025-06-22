using Shin_Megami_Tensei_GUI;
using Shin_Megami_Tensei;

public interface ITeamLoader
{
    (PlayerTeamInfo Team1, PlayerTeamInfo Team2) LoadTeamInformation(SMTGUI gui);
    Dictionary<string, Player> CreatePlayersFromTeamInfo((PlayerTeamInfo Team1, PlayerTeamInfo Team2) teamInfos);
}
