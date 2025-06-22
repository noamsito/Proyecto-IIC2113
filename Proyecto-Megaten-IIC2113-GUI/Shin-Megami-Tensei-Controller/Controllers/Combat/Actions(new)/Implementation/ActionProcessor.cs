using Shin_Megami_Tensei.Controllers.Exceptions;
using Shin_Megami_Tensei.Enums;

namespace Shin_Megami_Tensei.Managers.New_Actions.Implementation;

public class ActionProcessor : IActionProcessor
{
    private readonly ISamuraiActionExecutor _samuraiActionExecutor;
    private readonly IDemonActionExecutor _demonActionExecutor;

    public ActionProcessor(
        ISamuraiActionExecutor samuraiActionExecutor,
        IDemonActionExecutor demonActionExecutor)
    {
        _samuraiActionExecutor = samuraiActionExecutor ?? throw new ArgumentNullException(nameof(samuraiActionExecutor));
        _demonActionExecutor = demonActionExecutor ?? throw new ArgumentNullException(nameof(demonActionExecutor));
    }

    public bool ProcessSamuraiAction(Samurai samurai, SamuraiAction action, Player currentPlayer)
    {
        try
        {
            return _samuraiActionExecutor.Execute(samurai, action, currentPlayer);
        }
        catch (GameException)
        {
            // Log exception or handle appropriately
            return false;
        }
    }

    public bool ProcessDemonAction(Demon demon, DemonAction action, Player currentPlayer)
    {
        try
        {
            return _demonActionExecutor.Execute(demon, action, currentPlayer);
        }
        catch (GameException)
        {
            // Log exception or handle appropriately
            return false;
        }
    }
}
