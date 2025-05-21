using Shin_Megami_Tensei_View;
using Shin_Megami_Tensei.Gadgets;

namespace Shin_Megami_Tensei.Combat;

public class SkillUseContext
{
    public Unit Caster { get; }
    public Unit Target { get; set;  }
    public Skill Skill { get; }
    public Player Attacker { get; }
    public Player Defender { get; }

    public SkillUseContext(Unit caster, Unit target, Skill skill, Player attacker, Player defender)
    {
        Caster = caster;
        Target = target;
        Skill = skill;
        Attacker = attacker;
        Defender = defender;
    }
}