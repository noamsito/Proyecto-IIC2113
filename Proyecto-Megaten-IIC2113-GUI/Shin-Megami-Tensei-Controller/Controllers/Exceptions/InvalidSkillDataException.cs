namespace Shin_Megami_Tensei.Exceptions.Data;

public class InvalidSkillDataException : Exception
{
    public string SkillName { get; }
    public string InvalidValue { get; }

    public InvalidSkillDataException(string message) : base(message) { }
    
    public InvalidSkillDataException(string skillName, string invalidValue, string message) 
        : base(message)
    {
        SkillName = skillName;
        InvalidValue = invalidValue;
    }
}
