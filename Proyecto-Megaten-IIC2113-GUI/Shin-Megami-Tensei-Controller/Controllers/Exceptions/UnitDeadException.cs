namespace Shin_Megami_Tensei.Controllers.Exceptions;

public class UnitDeadException : GameException
{
    public UnitDeadException(string unitName) : base($"Unit '{unitName}' is dead and cannot perform actions") { }
}
