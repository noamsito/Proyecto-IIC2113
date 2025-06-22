using Shin_Megami_Tensei.Combat;
using Shin_Megami_Tensei.Gadgets;

namespace Shin_Megami_Tensei.Managers.Base;

public class SkillExecutionContext
{
    public Unit Caster { get; }
    public Skill Skill { get; }
    public Player CasterPlayer { get; }
    public Player OpponentPlayer { get; }
    public TurnContext TurnContext { get; }

    public SkillExecutionContext(Unit caster, Skill skill, Player casterPlayer, Player opponentPlayer, TurnContext turnContext)
    {
        Caster = caster ?? throw new ArgumentNullException(nameof(caster));
        Skill = skill ?? throw new ArgumentNullException(nameof(skill));
        CasterPlayer = casterPlayer ?? throw new ArgumentNullException(nameof(casterPlayer));
        OpponentPlayer = opponentPlayer ?? throw new ArgumentNullException(nameof(opponentPlayer));
        TurnContext = turnContext ?? throw new ArgumentNullException(nameof(turnContext));
    }
}