namespace Shin_Megami_Tensei;

public class CombatState
{
    public Player CurrentPlayer { get; set; }
    public Player OpponentPlayer { get; set; }
    public Unit CurrentDemon { get; set; }
    public TurnInfo TurnInfo { get; set; }
}