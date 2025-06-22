namespace Shin_Megami_Tensei.Managers.New_Actions;

public interface IActionHandler
{
    bool HandleAction(Unit activeUnit, Player currentPlayer);
}
