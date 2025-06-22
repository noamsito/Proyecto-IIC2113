using Shin_Megami_Tensei.Enums;

namespace Shin_Megami_Tensei.Managers.New_Actions;

public interface IDemonActionExecutor
{
    bool Execute(Demon demon, DemonAction action, Player currentPlayer);
}