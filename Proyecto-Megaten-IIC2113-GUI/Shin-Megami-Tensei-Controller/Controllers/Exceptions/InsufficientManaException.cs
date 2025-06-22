namespace Shin_Megami_Tensei.Controllers.Exceptions;

public class InsufficientManaException : GameException
{
    public InsufficientManaException(string skillName, int required, int available) 
        : base($"Insufficient mana for skill '{skillName}'. Required: {required}, Available: {available}") { }
}