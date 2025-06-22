namespace Shin_Megami_Tensei.Controllers.Exceptions;

public class InvalidTeamException : GameException
{
    public InvalidTeamException(string reason) : base($"Invalid team configuration: {reason}") { }
}