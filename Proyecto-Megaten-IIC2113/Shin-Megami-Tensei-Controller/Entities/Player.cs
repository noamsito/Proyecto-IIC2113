namespace Shin_Megami_Tensei;

public class Player
{
    public string Name;
    public Team Team;

    public Player(string name)
    {
        this.Name = name;
        // this.team = team;
    }

    public void SetTeam(Team team)
    {
        this.Team = team;
    }

    public bool IsTeamValid()
    {
        if (this.Team.HasSamurai() && this.Team.UnitNotRepeated() && this.Team.HasMinimumUnits())
        {
            this.Team.SetTeamAsValid();
        }
        else
        {
            this.Team.SetTeamAsInvalid();
        }
    
        return !this.Team.GetValidation();
    }
}