namespace Shin_Megami_Tensei.Views.Interfaces;

public interface ICombatView
{
    void DisplayGameState(Dictionary<string, Player> players, Player currentPlayer, List<string> options);
    string GetPlayerChoice();
}
