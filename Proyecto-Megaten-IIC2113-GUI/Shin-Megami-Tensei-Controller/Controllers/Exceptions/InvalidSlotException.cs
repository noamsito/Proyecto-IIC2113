namespace Shin_Megami_Tensei.Controllers.Exceptions;

public class InvalidSlotException : GameException
{
    public InvalidSlotException(int slot) : base($"Invalid slot index: {slot}") { }
}
