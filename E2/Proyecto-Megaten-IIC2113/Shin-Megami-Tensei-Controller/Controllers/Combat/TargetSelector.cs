using Shin_Megami_Tensei_View;
using Shin_Megami_Tensei.Combat;
using Shin_Megami_Tensei.Gadgets;

namespace Shin_Megami_Tensei.Managers;

public static class TargetSelector
{
    public static string SelectEnemy(AttackTargetContext ctx)
    {
        var enemies = GetValidEnemies(ctx.Opponent);
        DisplayTargetSelectionPrompt(ctx.View, ctx.Attacker.GetName());
        DisplayTargetOptions(ctx.View, enemies);
        DisplayCancelOption(ctx.View, enemies.Count);

        string input = GetUserInput(ctx.View);
        DisplaySeparator(ctx.View);

        return input;
    }

    public static Unit ResolveTarget(Player opponent, string input)
    {
        int index = ConvertInputToIndex(input);
        var validTargets = GetValidEnemies(opponent);
        return validTargets[index];
    }

    public static Unit? SelectSkillTarget(SkillTargetContext ctx, Unit unitAttacking)
    {
        DisplayTargetSelectionPrompt(ctx.View, unitAttacking.GetName());

        List<Unit> possibleTargets = GetPossibleTargets(ctx);
        DisplayTargetOptions(ctx.View, possibleTargets);
        DisplayCancelOption(ctx.View, possibleTargets.Count);

        string input = GetUserInput(ctx.View);

        if (IsCancelOption(input, possibleTargets.Count))
            return null;

        return GetSelectedTarget(possibleTargets, input);
    }

    private static List<Unit> GetValidEnemies(Player opponent)
    {
        return opponent.GetValidActiveUnits();
    }

    private static void DisplayTargetSelectionPrompt(View view, string attackerName)
    {
        view.WriteLine($"Seleccione un objetivo para {attackerName}");
    }

    private static void DisplayTargetOptions(View view, List<Unit> targets)
    {
        for (int i = 0; i < targets.Count; i++)
        {
            Unit unit = targets[i];
            string statusInfo = FormatUnitStatus(unit);
            view.WriteLine($"{i + 1}-{unit.GetName()} {statusInfo}");
        }
    }

    private static string FormatUnitStatus(Unit unit)
    {
        int currentHP = unit.GetCurrentStats().GetStatByName("HP");
        int maxHP = unit.GetBaseStats().GetStatByName("HP");
        int currentMP = unit.GetCurrentStats().GetStatByName("MP");
        int maxMP = unit.GetBaseStats().GetStatByName("MP");

        return $"HP:{currentHP}/{maxHP} MP:{currentMP}/{maxMP}";
    }

    private static string FormatHpStatus(Unit unit)
    {
        int currentHP = unit.GetCurrentStats().GetStatByName("HP");
        int maxHP = unit.GetBaseStats().GetStatByName("HP");

        return $"HP:{currentHP}/{maxHP}";
    }

    private static void DisplayCancelOption(View view, int optionsCount)
    {
        view.WriteLine($"{optionsCount + 1}-Cancelar");
    }

    private static string GetUserInput(View view)
    {
        return view.ReadLine();
    }

    private static void DisplaySeparator(View view)
    {
        view.WriteLine(GameConstants.Separator);
    }

    private static int ConvertInputToIndex(string input)
    {
        return Convert.ToInt32(input) - 1;
    }

    private static List<Unit> GetPossibleTargets(SkillTargetContext ctx)
    {
        bool isTargetAlly = ctx.Skill.Target == "Ally";
        return isTargetAlly
            ? ctx.CurrentPlayer.GetActiveUnits()
            : ctx.Opponent.GetValidActiveUnits();
    }

    private static bool IsCancelOption(string input, int optionsCount)
    {
        return input == $"{optionsCount + 1}";
    }

    private static Unit GetSelectedTarget(List<Unit> targets, string input)
    {
        int index = ConvertInputToIndex(input);
        return targets[index];
    }
}