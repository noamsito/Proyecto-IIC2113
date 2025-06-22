namespace Shin_Megami_Tensei_View.Implementation.Interfaces;

public interface ISelectionDisplayer
{
    void DisplaySelectTarget(string attackerName);
    void DisplaySkillSelectionPrompt(string unitName);
    void DisplaySummonPrompt();
    void DisplaySlotSelectionPrompt();
    void DisplayCancelOption(int optionsCount);
}
