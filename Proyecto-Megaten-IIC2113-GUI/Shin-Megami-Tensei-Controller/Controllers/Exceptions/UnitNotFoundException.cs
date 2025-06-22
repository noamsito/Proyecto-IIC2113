namespace Shin_Megami_Tensei.Controllers.Exceptions;

public class UnitNotFoundException : GameException
{
    public UnitNotFoundException(string unitName) : base($"Unit '{unitName}' not found") { }
}
