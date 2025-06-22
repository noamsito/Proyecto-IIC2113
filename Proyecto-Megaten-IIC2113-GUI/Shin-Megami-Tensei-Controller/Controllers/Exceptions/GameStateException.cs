namespace Shin_Megami_Tensei.Controllers.Exceptions;

public class GameStateException : GameException
{
    public GameStateException(string message) : base($"Invalid game state: {message}") { }
}
