using Shin_Megami_Tensei_GUI;
using Shin_Megami_Tensei;

public class GameStateAdapter : IState
{
    public IPlayer Player1 { get; }
    public IPlayer Player2 { get; }
    public IEnumerable<string> Options { get; }
    public int Turns { get; }
    public int BlinkingTurns { get; }
    public IEnumerable<string> Order { get; }

    public GameStateAdapter(Dictionary<string, Player> players, Player currentPlayer, List<string> options)
    {
        if (players == null) throw new ArgumentNullException(nameof(players));
        if (currentPlayer == null) throw new ArgumentNullException(nameof(currentPlayer));
        if (options == null) throw new ArgumentNullException(nameof(options));

        Player1 = new PlayerAdapter(players["Player 1"]);
        Player2 = new PlayerAdapter(players["Player 2"]);
        Options = options;
        Turns = currentPlayer.TurnManager.GetFullTurns();
        BlinkingTurns = currentPlayer.TurnManager.GetBlinkingTurns();
        Order = ExtractUnitOrder(currentPlayer);
    }

    private IEnumerable<string> ExtractUnitOrder(Player currentPlayer)
    {
        return currentPlayer.UnitManager.GetSortedActiveUnitsByOrderOfAttack()
            .Where(unit => unit != null)
            .Select(unit => unit.GetName());
    }
}