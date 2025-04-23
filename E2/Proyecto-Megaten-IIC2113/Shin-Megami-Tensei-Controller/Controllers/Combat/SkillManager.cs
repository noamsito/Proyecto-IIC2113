using Shin_Megami_Tensei_View;
using Shin_Megami_Tensei.Combat;
using Shin_Megami_Tensei.Gadgets;

namespace Shin_Megami_Tensei.Managers;

public static class SkillManager
{
    public static Skill? SelectSkill(View view, Unit unit)
    {
        var skills = unit.GetSkills();
        int displayIndex = 1;

        foreach (var skill in skills)
        {
            if (skill.Cost <= unit.GetCurrentStats().GetStatByName("MP"))
            {
                view.WriteLine($"{displayIndex}-{skill.Name} MP:{skill.Cost}");
                displayIndex++;
            }
        }

        view.WriteLine($"{displayIndex}-Cancelar");
        string input = view.ReadLine();
        int selected = Convert.ToInt32(input) - 1;
        
        view.WriteLine(GameConstants.Separator);

        if (selected < 0 || selected >= skills.Count || skills[selected].Cost > unit.GetCurrentStats().GetStatByName("MP"))
            return null;

        return skills[selected];
    }

    public static void ApplySkillEffect(SkillUseContext ctx)
    {
        ctx.View.WriteLine($"{ctx.Caster.GetName()} usa {ctx.Skill.Name} en {ctx.Target.GetName()}");

        switch (ctx.Skill.Type)
        {
            case "Phys":
            case "Gun":
            case "Fire":
            case "Ice":
            case "Elec":
            case "Force":
                HandleOffensiveSkill(ctx);
                // TurnManager.ApplyAffinityPenalty(ctx.Attacker, ctx.Target, ctx.Skill.Type);
                break;

            case "Heal":
                
                break;

            case "Special":
                // implementar Invitation o Sabbatma aquí
                break;
        }

        ConsumeMP(ctx.Caster, ctx.Skill.Cost);
    }

    private static void HandleOffensiveSkill(SkillUseContext ctx)
    {
        int damage = (int)Math.Floor(ctx.Skill.Power * GameConstants.ConstantOfDamage);
        UnitActionManager.ApplyDamageTaken(ctx.Target, damage);

        ctx.View.WriteLine($"{ctx.Target.GetName()} recibe {damage} de daño");
        ctx.View.WriteLine($"{ctx.Target.GetName()} termina con HP: {ctx.Target.GetCurrentStats().GetStatByName("HP")}/{ctx.Target.GetBaseStats().GetStatByName("HP")}");
    }
    

    private static void ConsumeMP(Unit caster, int cost)
    {
        int current = caster.GetCurrentStats().GetStatByName("MP");
        caster.GetCurrentStats().SetStatByName("MP", Math.Max(0, current - cost));
    }
}
