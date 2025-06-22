using Shin_Megami_Tensei.Controllers.Exceptions;

public class PlayerNotInitializedException : GameException
{
    public PlayerNotInitializedException() : base("Players must be initialized before performing this operation") { }
}