using Shin_Megami_Tensei_View.Implementation.Implementation;
using Shin_Megami_Tensei.Managers.Managers.Interfaces;

namespace Shin_Megami_Tensei.Managers.Managers.Implementation;

public class VictoryChecker : IVictoryChecker
{
    private readonly CombatUIFacade _ui;

    public VictoryChecker(CombatUIFacade ui)
    {
        _ui = ui ?? throw new ArgumentNullException(nameof(ui));
    }

    public bool CheckForVictory(Player currentPlayer, Player opponent)
    {
        UpdateTeamsStatus(currentPlayer, opponent);

        if (!currentPlayer.CombatState.IsTeamAbleToContinue())
        {
            AnnounceWinner(opponent);
            return true;
        }

        if (!opponent.CombatState.IsTeamAbleToContinue())
        {
            AnnounceWinner(currentPlayer);
            return true;
        }

        return false;
    }

    public void UpdateTeamsStatus(Player currentPlayer, Player opponent)
    {
        currentPlayer.CombatState.CheckIfTeamIsAbleToContinue();
        opponent.CombatState.CheckIfTeamIsAbleToContinue();
    }

    public void AnnounceWinner(Player winner)
    {
        _ui.DisplayWinner(winner);
    }
}
