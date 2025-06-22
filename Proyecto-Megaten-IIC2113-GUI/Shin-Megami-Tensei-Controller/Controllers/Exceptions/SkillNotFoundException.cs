namespace Shin_Megami_Tensei.Controllers.Exceptions;

public class SkillNotFoundException : GameException
{
    public SkillNotFoundException(string skillName) : base($"Skill '{skillName}' not found") { }
}