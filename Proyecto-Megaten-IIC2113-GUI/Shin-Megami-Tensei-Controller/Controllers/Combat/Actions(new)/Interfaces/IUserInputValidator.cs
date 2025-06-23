namespace Shin_Megami_Tensei.Managers.New_Actions;

public interface IUserInputValidator
{
    bool IsValidInput(string input, Unit unit);
    public T ParseInput<T>(string input) where T : struct, Enum;
}
