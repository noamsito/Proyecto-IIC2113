namespace Shin_Megami_Tensei;

public class Player
{
    private string _name;
    private Team _team;
    private int fullTurns;
    private int blinkingTurns;

    public Player(string name)
    {
        this._name = name;
    }

    public void SetTeam(Team team)
    {
        this._team = team;
    }

   public bool IsTeamValid()
   {
       if (!this._team.HasSamurai() || this._team.GetIfSamuraiIsRepeated() || !this._team.HasLessThanMaximumUnits()
            || this._team.UnitRepeated() || this._team.SamuraiWithMoreThanMaxSkills()
            || this._team.SamuraiWithRepeatedHabilities())
       {
           this._team.SetTeamAsInvalid();
       }
       else
       {
           this._team.SetTeamAsValid();
       }
   
       return this._team.GetValidation();
   }

   public string GetName()
   {
       return this._name;
   }

   public Team GetTeam()
   {
       return this._team;   
   }

   public int GetFullTurns()
   {
        return this.fullTurns;
   }

   public int GetBlinkingTurns()
   {
        return this.blinkingTurns;
   }

   public void SetTurns()
   {
       if (this._team.HasSamurai())
       {
           this.fullTurns = this._team.GetDemons().Count + 1;
       }

       this.blinkingTurns = 0;
   } 
}