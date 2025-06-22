namespace Shin_Megami_Tensei.Managers.Managers.Interfaces;

public interface IActionExecutor
{
    bool ExecuteUnitAction(Unit activeUnit, Player currentPlayer);
}