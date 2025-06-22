namespace Shin_Megami_Tensei.Managers;

public class PlayerTurnManager
{
    private readonly Player _player;
    private int _fullTurns;
    private int _blinkingTurns;
    private int _KConstOfHits = 0;

    public PlayerTurnManager(Player player)
    {
        _player = player;
        _fullTurns = 0;
        _blinkingTurns = 0;
    }

    public int GetFullTurns() => _fullTurns;
    public int GetBlinkingTurns() => _blinkingTurns;

    public void SetTurns()
    {
        _fullTurns = _player.UnitManager.CountHealthyActiveUnits();
        _blinkingTurns = 0;
    }

    public bool IsPlayerOutOfTurns() => _fullTurns == 0 && _blinkingTurns == 0;

    public void ConsumeFullTurn(int amount)
    {
        _fullTurns = Math.Max(0, _fullTurns - amount);
    }

    public void ConsumeBlinkingTurn(int amount)
    {
        _blinkingTurns = Math.Max(0, _blinkingTurns - amount);
    }

    public void GainBlinkingTurn(int amount)
    {
        _blinkingTurns += amount;
    }
    
    public int GetConstantKPlayer() => _KConstOfHits;
    
    public void IncreaseConstantKPlayer()
    {
        _KConstOfHits++;
    }
}