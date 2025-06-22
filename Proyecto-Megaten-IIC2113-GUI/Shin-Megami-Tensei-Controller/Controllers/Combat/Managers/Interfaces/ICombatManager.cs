namespace Shin_Megami_Tensei.Managers.Managers.Interfaces;

public interface ICombatManager
{
    void StartCombat();
    bool IsGameWon { get; }
    Player GetWinner();
}