using Shin_Megami_Tensei_View;
using Shin_Megami_Tensei.Gadgets;

namespace Shin_Megami_Tensei.Combat;

public class SkillTargetContext
{
    public Skill Skill { get; }
    public Player CurrentPlayer { get; }
    public Player Opponent { get; }
    public View View { get; }

    public SkillTargetContext(Skill skill, Player currentPlayer, Player opponent, View view)
    {
        Skill = skill;
        CurrentPlayer = currentPlayer;
        Opponent = opponent;
        View = view;
    }
}