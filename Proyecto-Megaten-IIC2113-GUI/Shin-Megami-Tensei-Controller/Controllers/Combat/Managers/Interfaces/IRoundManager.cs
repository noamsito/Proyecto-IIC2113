namespace Shin_Megami_Tensei.Managers.Managers.Interfaces;

public interface IRoundManager
{
    void StartNewRound(Player player, int playerNumber);
    void InitializePlayersForCombat(Dictionary<string, Player> players);
}