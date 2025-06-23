using Shin_Megami_Tensei.Controllers.Exceptions;
using Shin_Megami_Tensei.Enums;

namespace Shin_Megami_Tensei.Managers.New_Actions.Implementation;

public class UserInputValidator : IUserInputValidator
{
    public bool IsValidInput(string input, Unit unit)
    {
        if (string.IsNullOrWhiteSpace(input))
            return false;

        return unit switch
        {
            Samurai => IsValidSamuraiInput(input),
            Demon => IsValidDemonInput(input),
            _ => false
        };
    }

    public T ParseInput<T>(string input) where T : struct, Enum
    {
        if (Enum.TryParse(input, true, out T result))
            return result;
        
        throw new InvalidActionException($"Invalid input for {typeof(T).Name}: {input}");
    }

    private bool IsValidSamuraiInput(string input)
    {
        return Enum.TryParse<SamuraiAction>(input, true, out _);
    }

    private bool IsValidDemonInput(string input)
    {
        return Enum.TryParse<DemonAction>(input, true, out _);
    }
}