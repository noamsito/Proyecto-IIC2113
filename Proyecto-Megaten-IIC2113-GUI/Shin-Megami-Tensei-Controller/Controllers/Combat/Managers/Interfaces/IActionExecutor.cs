namespace Shin_Megami_Tensei.Managers.Managers.Interfaces;

public interface IActionExecutor
{
    bool TryExecuteUnitAction(Unit activeUnit, Player currentPlayer);
}