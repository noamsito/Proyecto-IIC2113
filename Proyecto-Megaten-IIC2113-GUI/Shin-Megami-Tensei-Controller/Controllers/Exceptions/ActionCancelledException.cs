namespace Shin_Megami_Tensei.Controllers.Exceptions;

public class ActionCancelledException : GameException
{
    public ActionCancelledException() : base("Action was cancelled by user") { }
}
