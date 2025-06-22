using Shin_Megami_Tensei;

namespace Shin_Megami_Tensei_View.Contracts
{
    public interface IGameView
    {
        void ShowInvalidTeamMessage();
        void ShowWinner(Player winner);
        Dictionary<string, Player> LoadTeamsFromInput();
    }
}