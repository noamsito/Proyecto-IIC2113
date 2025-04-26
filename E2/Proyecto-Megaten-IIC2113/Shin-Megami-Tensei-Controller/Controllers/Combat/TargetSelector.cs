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
        CombatUI.DisplayTargetOptions(enemies);
        CombatUI.DisplayCancelOption(enemies.Count);

        string input = CombatUI.GetUserInput();
        CombatUI.DisplaySeparator();

        return input;
    }

    public static Unit ResolveTarget(Player opponent, string input)
    {
        int index = ConvertInputToIndex(input);
        var validTargets = GetValidEnemies(opponent);
        return validTargets[index];
    }

    public static Unit? SelectSkillTarget(SkillTargetContext skillCtx, Unit unitAttacking)
    {
        DisplayTargetSelectionPrompt(skillCtx.View, unitAttacking.GetName());

        List<Unit> possibleTargets = GetPossibleTargets(skillCtx);
        CombatUI.DisplayTargetOptions(possibleTargets);
        CombatUI.DisplayCancelOption(possibleTargets.Count);

        string input = GetUserInput(skillCtx.View);

        if (IsCancelOption(input, possibleTargets.Count))
            return null;
        
        skillCtx.View.WriteLine(GameConstants.Separator);

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

    public static string FormatUnitStatus(Unit unit)
    {
        int currentHP = unit.GetCurrentStats().GetStatByName("HP");
        int maxHP = unit.GetBaseStats().GetStatByName("HP");
        int currentMP = unit.GetCurrentStats().GetStatByName("MP");
        int maxMP = unit.GetBaseStats().GetStatByName("MP");

        return $"HP:{currentHP}/{maxHP} MP:{currentMP}/{maxMP}";
    }
    
    private static string GetUserInput(View view)
    {
        return view.ReadLine();
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