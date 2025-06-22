namespace Shin_Megami_Tensei.Managers;

public class SkillExecutionResult
{
    public bool WasSuccessful { get; }

    public SkillExecutionResult(bool wasSuccessful)
    {
        WasSuccessful = wasSuccessful;
    }

    public static SkillExecutionResult Failed() => new(false);
}