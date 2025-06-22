using Shin_Megami_Tensei;
using Shin_Megami_Tensei.Enums;
using Shin_Megami_Tensei.Gadgets;

namespace Shin_Megami_Tensei_View.Implementation.Interfaces;

public interface ICombatDisplayer
{
    void DisplayActionSelection(string unitName);
    void DisplaySamuraiOptions();
    void DisplayDemonOptions();
    void DisplayAttack(string attackerName, string targetName, AttackType attackType);
    void DisplaySkillUsage(Unit caster, Skill skill, Unit target);
}
