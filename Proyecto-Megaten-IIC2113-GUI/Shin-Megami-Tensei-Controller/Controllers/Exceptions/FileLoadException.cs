namespace Shin_Megami_Tensei.Controllers.Exceptions;

public class FileLoadException : GameException
{
    public FileLoadException(string fileName, Exception innerException) 
        : base($"Failed to load file: {fileName}", innerException) { }
}
