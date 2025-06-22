using Shin_Megami_Tensei_GUI;
using Shin_Megami_Tensei.Views.Interfaces;

public class GameStatePresenter : IGameStatePresenter
{
    private readonly SMTGUI _gui;

    public GameStatePresenter(SMTGUI gui)
    {
        _gui = gui ?? throw new ArgumentNullException(nameof(gui));
    }

    public void UpdateDisplay(GameDisplayState displayState)
    {
        var gameState = new GameStateAdapter(
            displayState.Players,
            displayState.CurrentPlayer,
            displayState.AvailableOptions
        );
            
        _gui.Update(gameState);
    }
}