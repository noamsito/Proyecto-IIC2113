using Shin_Megami_Tensei_View;
using Shin_Megami_Tensei;
using Shin_Megami_Tensei.Gadgets;

public static class CombatUI
{
    public static void DisplayBoardState(Dictionary<string, Player> players, View view)
    {
        int playerNumber = 1;
        foreach (var player in players.Values)
        {
            view.WriteLine($"Equipo de {player.GetTeam().Samurai.GetName()} (J{playerNumber})");
            DisplayActiveUnits(player, view);
            playerNumber++;
        }
        view.WriteLine(GameConstants.Separator);
    }

    public static void DisplayActiveUnits(Player player, View view)
    {
        var units = player.GetActiveUnits();
        char label = 'A';

        foreach (var unit in units)
        {
            if (unit == null)
            {
                view.WriteLine($"{label}-");
            }
            else
            {
                view.WriteLine($"{label}-{unit.GetName()} HP:{unit.GetCurrentStats().GetStatByName("HP")}/{unit.GetBaseStats().GetStatByName("HP")} MP:{unit.GetCurrentStats().GetStatByName("MP")}/{unit.GetBaseStats().GetStatByName("MP")}");
            }
            label++;
        }
    }


    public static void DisplayTurnInfo(Player player, View view)
    {
        view.WriteLine($"Full Turns: {player.GetFullTurns()}");
        view.WriteLine($"Blinking Turns: {player.GetBlinkingTurns()}");
        view.WriteLine(GameConstants.Separator);
    }

    public static void DisplaySortedUnits(Player player, View view)
    {
        view.WriteLine("Orden:");
        var units = player.GetSortedActiveUnitsByOrderOfAttack();
        for (int i = 0; i < units.Count; i++)
            view.WriteLine($"{i + 1}-{units[i].GetName()}");
        view.WriteLine(GameConstants.Separator);
    }

    public static void DisplayWinner(Player winner, View view)
    {
        int playerNumber = winner.GetName() == "Player 1" ? 1 : 2;
        view.WriteLine($"Ganador: {winner.GetTeam().Samurai.GetName()} (J{playerNumber})");
    }
    
    public static void DisplayAttack(string attackerName, string targetName, string attackType, View view)
    {
        string action = attackType switch
        {
            "Phys" => "ataca a",
            "Gun" => "dispara a",
            "Fire" => "lanza fuego a",
            "Ice" => "lanza hielo a",
            "Elec" => "lanza electricidad a",
            "Force" => "lanza viento a",
            _ => "ataca a"
        };

        view.WriteLine($"{attackerName} {action} {targetName}");
    }

    public static void DisplayDamageResult(Unit target, int damage, View view)
    {
        view.WriteLine($"{target.GetName()} recibe {damage} de daño");
        view.WriteLine($"{target.GetName()} termina con HP:{target.GetCurrentStats().GetStatByName("HP")}/{target.GetBaseStats().GetStatByName("HP")}");
        view.WriteLine(GameConstants.Separator);
    }

    public static void DisplayHealing(Unit target, int amount, View view)
    {
        view.WriteLine($"{target.GetName()} recibe {amount} de HP");
        view.WriteLine($"{target.GetName()} termina con HP:{target.GetCurrentStats().GetStatByName("HP")}/{target.GetBaseStats().GetStatByName("HP")}");
        view.WriteLine(GameConstants.Separator);
    }

    public static void DisplayAffinityMessage(string affinityType, string attackerName, string targetName, View view)
    {
        string msg = affinityType switch
        {
            "Wk" => $"{targetName} es débil contra el ataque de {attackerName}",
            "Rs" => $"{targetName} es resistente al ataque de {attackerName}",
            "Nu" => $"{targetName} bloquea el ataque de {attackerName}",
            "Rp" => $"{targetName} devuelve el ataque de {attackerName}",
            "Dr" => $"{targetName} absorbe el daño de {attackerName}"
        };

        if (!string.IsNullOrEmpty(msg))
            view.WriteLine(msg);
    }

    public static void DisplayTurnChanges(int fullTurns, int blinkingConsumed, int blinkingGained, View view)
    {
        view.WriteLine($"Se han consumido {fullTurns} Full Turn(s) y {blinkingConsumed} Blinking Turn(s)");
        view.WriteLine($"Se han obtenido {blinkingGained} Blinking Turn(s)");
    }

    public static void DisplaySkillUsage(Unit caster, Skill skill, Unit target, View view)
    {
        string action = skill.Type switch
        {
            "Fire" => "lanza fuego a",
            "Ice" => "lanza hielo a",
            "Elec" => "lanza electricidad a",
            "Force" => "lanza viento a",
            _ => "usa " + skill.Name + " en"
        };

        view.WriteLine($"{caster.GetName()} {action} {target.GetName()}");
    }

    public static void DisplayRevive(Unit caster, Unit revived, int healAmount, View view)
    {
        view.WriteLine($"{caster.GetName()} revive a {revived.GetName()}");
        view.WriteLine($"{revived.GetName()} recibe {healAmount} de HP");
        view.WriteLine($"{revived.GetName()} termina con HP:{revived.GetCurrentStats().GetStatByName("HP")}/{revived.GetBaseStats().GetStatByName("HP")}");
        view.WriteLine(GameConstants.Separator);
    }

    public static void DisplaySummon(Unit summoned, View view)
    {
        view.WriteLine($"{summoned.GetName()} ha sido invocado");
        view.WriteLine(GameConstants.Separator);
    }
}