using Shin_Megami_Tensei_View;

namespace Shin_Megami_Tensei.Controllers;

public static class ActionMenuProvider
{
    private static readonly Dictionary<UnitType, ActionMenu> _menus = new()
    {
        {
            UnitType.Samurai,
            new ActionMenu(
                new[] { "1: Atacar", "2: Disparar", "3: Usar Habilidad", "4: Invocar", "5: Pasar Turno", "6: Rendirse" },
                new[] { "1", "2", "3", "4", "5", "6" }
            )
        },
        {
            UnitType.Demon,
            new ActionMenu(
                new[] { "1: Atacar", "2: Usar Habilidad", "3: Invocar", "4: Pasar Turno" },
                new[] { "1", "2", "3", "4" }
            )
        }
    };

    public static void DisplayMenu(UnitType unitType, string unitName, View view)
    {
        view.WriteLine($"Seleccione una acción para {unitName}");
        
        if (!_menus.ContainsKey(unitType))
        {
            throw new ArgumentException($"Tipo de unidad no soportado: {unitType}");
        }
        
        var menu = _menus[unitType];
        foreach (string option in menu.Options)
        {
            view.WriteLine(option);
        }
    }

    public static bool IsValidInput(UnitType unitType, string input)
    {
        if (!_menus.ContainsKey(unitType))
        {
            return false;
        }
        
        return _menus[unitType].ValidInputs.Contains(input);
    }

    private record ActionMenu(string[] Options, string[] ValidInputs);
    
    public enum UnitType
    {
        Samurai,
        Demon
    }
}