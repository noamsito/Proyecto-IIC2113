namespace Shin_Megami_Tensei.Exceptions.Data;

public class SkillNotFoundException : Exception
{
    public string SkillName { get; }
    
    public SkillNotFoundException(string message) : base(message) { }
    
    public SkillNotFoundException(string skillName, string message) 
        : base(message)
    {
        SkillName = skillName;
    }
}
