namespace Shin_Megami_Tensei.Combat;
public class SummonExecutionContext
{
    public Player CurrentPlayer { get; }
    public Unit CurrentUnit { get; }
    public TurnContext TurnContext { get; }
    public SummonType SummonType { get; }

    public SummonExecutionContext(Player currentPlayer, Unit currentUnit, TurnContext turnContext, SummonType summonType)
    {
        CurrentPlayer = currentPlayer;
        CurrentUnit = currentUnit;
        TurnContext = turnContext;
        SummonType = summonType;
    }
}

public enum SummonType
{
    Samurai,
    Demon
}