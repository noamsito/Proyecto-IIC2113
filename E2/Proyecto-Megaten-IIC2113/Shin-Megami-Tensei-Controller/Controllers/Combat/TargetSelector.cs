using Shin_Megami_Tensei_View;
using Shin_Megami_Tensei.Combat;
using Shin_Megami_Tensei.Gadgets;

namespace Shin_Megami_Tensei.Managers;

public static class TargetSelector
{
    public static string SelectEnemy(AttackTargetContext ctx)
    {
        var enemies = ctx.Opponent.GetValidActiveUnits();

        ctx.View.WriteLine($"Seleccione un objetivo para {ctx.Attacker.GetName()}");

        for (int i = 0; i < enemies.Count; i++)
        {
            Unit unit = enemies[i];
            string hp = $"HP:{unit.GetCurrentStats().GetStatByName("HP")}/{unit.GetBaseStats().GetStatByName("HP")} MP:{unit.GetCurrentStats().GetStatByName("MP")}/{unit.GetBaseStats().GetStatByName("MP")}";            ctx.View.WriteLine($"{i + 1}-{unit.GetName()} {hp}");
        }

        ctx.View.WriteLine($"{enemies.Count + 1}-Cancelar");
        string input = ctx.View.ReadLine();
        ctx.View.WriteLine(GameConstants.Separator);

        return input;
    }


    public static Unit ResolveTarget(Player opponent, string input)
    {
        int index = Convert.ToInt32(input) - 1;
        return opponent.GetValidActiveUnits()[index];
    }

    public static Unit? SelectSkillTarget(SkillTargetContext ctx, Unit unitAttacking)
    {
        ctx.View.WriteLine($"Seleccione un objetivo para {unitAttacking.GetName()}");
        
        bool isTargetAlly = ctx.Skill.Target == "Ally";
        List<Unit> possibleTargets = isTargetAlly
            ? ctx.CurrentPlayer.GetActiveUnits()
            : ctx.Opponent.GetValidActiveUnits();

        for (int i = 0; i < possibleTargets.Count; i++)
        {
            var unit = possibleTargets[i];
            string hpStatus = $"HP:{unit.GetCurrentStats().GetStatByName("HP")}/{unit.GetBaseStats().GetStatByName("HP")}";
            ctx.View.WriteLine($"{i + 1}-{unit.GetName()} {hpStatus}");
        }

        ctx.View.WriteLine($"{possibleTargets.Count + 1}-Cancelar");
        string input = ctx.View.ReadLine();

        if (input == $"{possibleTargets.Count + 1}")
            return null;

        int index = Convert.ToInt32(input) - 1;
        return possibleTargets[index];
    }

}