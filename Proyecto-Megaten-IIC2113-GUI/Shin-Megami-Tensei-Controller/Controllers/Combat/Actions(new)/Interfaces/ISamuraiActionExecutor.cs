using Shin_Megami_Tensei;
using Shin_Megami_Tensei.Enums;

public interface ISamuraiActionExecutor
{
    bool Execute(Samurai samurai, SamuraiAction action, Player currentPlayer);
}