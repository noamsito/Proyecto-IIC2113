namespace Shin_Megami_Tensei_View.Implementation.Implementation;

public class ConsoleDisplayService : IDisplayService
{
    public void WriteLine(string message)
    {
        Console.WriteLine(message);
    }

    public string ReadLine()
    {
        return Console.ReadLine() ?? string.Empty;
    }
}
