namespace Shin_Megami_Tensei.Controllers.Exceptions;

public class DataValidationException : GameException
{
    public DataValidationException(string fieldName, string value) 
        : base($"Invalid data for field '{fieldName}': {value}") { }
}
