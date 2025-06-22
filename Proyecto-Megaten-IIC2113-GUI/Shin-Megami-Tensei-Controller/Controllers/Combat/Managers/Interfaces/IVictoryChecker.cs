namespace Shin_Megami_Tensei.Managers.Managers.Interfaces;

public interface IVictoryChecker
{
    bool CheckForVictory(Player currentPlayer, Player opponent);
    void UpdateTeamsStatus(Player currentPlayer, Player opponent);
    void AnnounceWinner(Player winner);
}