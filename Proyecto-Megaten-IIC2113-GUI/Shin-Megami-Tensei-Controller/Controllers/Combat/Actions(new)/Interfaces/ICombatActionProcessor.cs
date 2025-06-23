using Shin_Megami_Tensei;
using Shin_Megami_Tensei.Enums;

public interface ICombatActionProcessor
{
    bool ProcessSamuraiAction(Samurai samurai, SamuraiAction action, Player currentPlayer);
    bool ProcessDemonAction(Demon demon, DemonAction action, Player currentPlayer);
}