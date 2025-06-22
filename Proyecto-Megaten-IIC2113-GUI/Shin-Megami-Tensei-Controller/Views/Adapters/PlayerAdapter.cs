using Shin_Megami_Tensei_GUI;
using Shin_Megami_Tensei;

public class PlayerAdapter : IPlayer
{
    private const int MAX_BOARD_SLOTS = 4;
        
    public IUnit?[] UnitsInBoard { get; }
    public IEnumerable<IUnit> UnitsInReserve { get; }

    public PlayerAdapter(Player player)
    {
        if (player == null) throw new ArgumentNullException(nameof(player));

        UnitsInBoard = CreateBoardUnitsArray(player);
        UnitsInReserve = CreateReserveUnitsCollection(player);
    }

    private IUnit?[] CreateBoardUnitsArray(Player player)
    {
        var activeUnits = player.UnitManager.GetActiveUnits();
        var boardUnits = new IUnit?[MAX_BOARD_SLOTS];

        for (int i = 0; i < Math.Min(activeUnits.Count, MAX_BOARD_SLOTS); i++)
        {
            boardUnits[i] = activeUnits[i] != null ? new UnitAdapter(activeUnits[i]) : null;
        }

        return boardUnits;
    }

    private IEnumerable<IUnit> CreateReserveUnitsCollection(Player player)
    {
        return player.UnitManager.GetReservedUnits()
            .Where(unit => unit != null)
            .Select(unit => new UnitAdapter(unit));
    }
}