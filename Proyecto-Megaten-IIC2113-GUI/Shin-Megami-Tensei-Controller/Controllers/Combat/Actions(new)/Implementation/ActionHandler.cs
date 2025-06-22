using Shin_Megami_Tensei.Enums;

namespace Shin_Megami_Tensei.Managers.New_Actions.Implementation;

public class ActionHandler : IActionHandler
{
    private readonly IShinMegamiTenseiView _view;
    private readonly IUserInputValidator _inputValidator;
    private readonly IActionProcessor _actionProcessor;

    public ActionHandler(
        IShinMegamiTenseiView view,
        IUserInputValidator inputValidator,
        IActionProcessor actionProcessor)
    {
        _view = view ?? throw new ArgumentNullException(nameof(view));
        _inputValidator = inputValidator ?? throw new ArgumentNullException(nameof(inputValidator));
        _actionProcessor = actionProcessor ?? throw new ArgumentNullException(nameof(actionProcessor));
    }

    public bool HandleAction(Unit activeUnit, Player currentPlayer)
    {
        string playerChoice = _view.GetPlayerChoice();
            
        if (!_inputValidator.IsValidInput(playerChoice, activeUnit))
            return false;

        return activeUnit switch
        {
            Samurai samurai => HandleSamuraiAction(samurai, playerChoice, currentPlayer),
            Demon demon => HandleDemonAction(demon, playerChoice, currentPlayer),
            _ => false
        };
    }

    private bool HandleSamuraiAction(Samurai samurai, string choice, Player currentPlayer)
    {
        if (!Enum.TryParse<SamuraiAction>(choice, out var action))
            return false;

        return _actionProcessor.ProcessSamuraiAction(samurai, action, currentPlayer);
    }

    private bool HandleDemonAction(Demon demon, string choice, Player currentPlayer)
    {
        if (!Enum.TryParse<DemonAction>(choice, out var action))
            return false;

        return _actionProcessor.ProcessDemonAction(demon, action, currentPlayer);
    }
}
