using Shin_Megami_Tensei;

public class GameDisplayState
{
    public Dictionary<string, Player> Players { get; }
    public Player CurrentPlayer { get; }
    public List<string> AvailableOptions { get; }

    public GameDisplayState(Dictionary<string, Player> players, Player currentPlayer, List<string> options)
    {
        Players = players ?? throw new ArgumentNullException(nameof(players));
        CurrentPlayer = currentPlayer ?? throw new ArgumentNullException(nameof(currentPlayer));
        AvailableOptions = options ?? throw new ArgumentNullException(nameof(options));
    }
}