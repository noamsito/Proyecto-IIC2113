namespace Shin_Megami_Tensei.Managers;

public class SummonExecutionResult
{
    public bool WasSuccessful { get; }

    public SummonExecutionResult(bool wasSuccessful)
    {
        WasSuccessful = wasSuccessful;
    }
}