namespace Shin_Megami_Tensei.Controllers.Exceptions;

public class NoValidTargetsException : GameException
{
    public NoValidTargetsException() : base("No valid targets available for this action") { }
}
