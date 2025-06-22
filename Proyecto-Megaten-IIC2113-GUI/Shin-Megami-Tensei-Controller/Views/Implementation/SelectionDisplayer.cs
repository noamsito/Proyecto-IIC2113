using Shin_Megami_Tensei_View.Implementation.Interfaces;

namespace Shin_Megami_Tensei_View.Implementation.Implementation;

public class SelectionDisplayer : ISelectionDisplayer
{
    private readonly IDisplayService _displayService;

    public SelectionDisplayer(IDisplayService displayService)
    {
        _displayService = displayService ?? throw new ArgumentNullException(nameof(displayService));
    }

    public void DisplaySelectTarget(string attackerName)
    {
        _displayService.WriteLine($"Seleccione un objetivo para {attackerName}");
    }

    public void DisplaySkillSelectionPrompt(string unitName)
    {
        _displayService.WriteLine($"Seleccione una habilidad para que {unitName} use");
    }

    public void DisplaySummonPrompt()
    {
        _displayService.WriteLine("Seleccione un monstruo para invocar");
    }

    public void DisplaySlotSelectionPrompt()
    {
        _displayService.WriteLine("Seleccione una posición para invocar");
    }

    public void DisplayCancelOption(int optionsCount)
    {
        _displayService.WriteLine($"{optionsCount + 1}-{GameConstants.CANCEL_OPTION}");
    }
}
