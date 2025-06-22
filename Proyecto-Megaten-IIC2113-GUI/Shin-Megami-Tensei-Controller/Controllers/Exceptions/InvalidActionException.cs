namespace Shin_Megami_Tensei.Controllers.Exceptions;

public class InvalidActionException : GameException
{
    public InvalidActionException(string action) : base($"Invalid action: {action}") { }
}
