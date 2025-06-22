namespace Shin_Megami_Tensei.Managers.Managers.Interfaces;

public interface ITurnManager
{
    void ProcessPlayerTurn(Player currentPlayer);
    bool ShouldSwitchPlayer(Player currentPlayer);
    Player GetOpponent(Player currentPlayer);
}