using Shin_Megami_Tensei.Managers;

namespace Shin_Megami_Tensei;

public class Player
{
    private const string Player1Name = "Player 1";
    private const string Player2Name = "Player 2";
    
    private readonly string _name;
    private Team _team;
    private bool _hasSurrendered = false;
    
    
    public PlayerTeamManager TeamManager { get; private set; }
    public PlayerTurnManager TurnManager { get; private set; }
    public PlayerUnitManager UnitManager { get; private set; }
    public PlayerCombatState CombatState { get; private set; }

    public Player(string name)
    {
        _name = name;
        TeamManager = new PlayerTeamManager(this);
        TurnManager = new PlayerTurnManager(this);
        UnitManager = new PlayerUnitManager(this);
        CombatState = new PlayerCombatState(this);
    }

    public void SetTeam(Team team)
    {
        _team = team;
        TeamManager.OrganizeTeamUnits();
    }

    public void SetTeamValidation()
    {
        if (TeamManager.HasInvalidTeamConfiguration())
        {
            _team.SetTeamAsInvalid();
        }
        else
        {
            _team.SetTeamAsValid();
        }
    }

    public string GetName() => _name;
    public Team GetTeam() => _team;

    public void Surrender()
    {
        _hasSurrendered = true;
        CombatState.SetTeamUnableToContinue();
    }

    public bool HasSurrendered() => _hasSurrendered;

    public bool IsPlayer1() => _name == Player1Name;
    public bool IsPlayer2() => _name == Player2Name;
}