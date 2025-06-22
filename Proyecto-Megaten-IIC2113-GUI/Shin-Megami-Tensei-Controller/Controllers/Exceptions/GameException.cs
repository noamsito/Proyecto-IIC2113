namespace Shin_Megami_Tensei.Controllers.Exceptions;

public abstract class GameException : Exception
{
    protected GameException(string message) : base(message) { }
    protected GameException(string message, Exception innerException) : base(message, innerException) { }
}
