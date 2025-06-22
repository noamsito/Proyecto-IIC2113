namespace Shin_Megami_Tensei_View.Implementation.Interfaces;

public interface ITurnDisplayer
{
    void DisplayTurnChanges(int fullConsumed, int blinkingConsumed, int blinkingGained);
    void DisplaySeparator();
}