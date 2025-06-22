using Shin_Megami_Tensei_GUI;
using Shin_Megami_Tensei.Views.Interfaces;

public class UserInputHandler : IUserInputHandler
{
    private readonly SMTGUI _gui;
    private const int MAX_INPUT_ATTEMPTS = 1000;

    public UserInputHandler(SMTGUI gui)
    {
        _gui = gui ?? throw new ArgumentNullException(nameof(gui));
    }

    public string WaitForValidChoice()
    {
        var attempts = 0;

        while (attempts < MAX_INPUT_ATTEMPTS)
        {
            var clickedElement = _gui.GetClickedElement();
            attempts++;

            if (IsValidActionButton(clickedElement))
            {
                return MapButtonTextToChoice(clickedElement.Text);
            }
        }

        return "5";  
    }

    private bool IsValidActionButton(IClickedElement element)
    {
        return element.Type == ClickedElementType.Button &&
               !IsSystemButton(element.Text);
    }

    private bool IsSystemButton(string buttonText)
    {
        return buttonText.StartsWith("Turno de") ||
               buttonText.StartsWith("---") ||
               buttonText.StartsWith("Selecciona");
    }

    private string MapButtonTextToChoice(string buttonText)
    {
        return buttonText switch
        {
            "Atacar" => "1",
            "Disparar" => "2",
            "Usar Habilidad" => "3",
            "Invocar" => "4",
            "Pasar Turno" => "5",
            "Rendirse" => "6",
            _ => "1"
        };
    }
}