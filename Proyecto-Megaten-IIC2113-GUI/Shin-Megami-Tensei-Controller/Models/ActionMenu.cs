public class ActionMenu
{
    public string[] Options { get; }
    public string[] ValidInputs { get; }

    public ActionMenu(string[] options, string[] validInputs)
    {
        Options = options;
        ValidInputs = validInputs;
    }
}