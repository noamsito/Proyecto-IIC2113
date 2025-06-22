using Shin_Megami_Tensei;

namespace Shin_Megami_Tensei_View.Implementation.Interfaces;

public interface IGameStateDisplayer
{
    void DisplayBoardState(Dictionary<string, Player> players);
    void DisplayTurnInfo(Player player);
    void DisplaySortedUnits(Player player);
    void DisplayRoundStart(Samurai samurai, int playerNumber);
}