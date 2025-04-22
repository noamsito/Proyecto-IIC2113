using System.Text.Json;
using Shin_Megami_Tensei.Units;

namespace Shin_Megami_Tensei;

public class Player
{
    private string _name;
    private Team _team;
    
    private int _fullTurns;
    private int _blinkingTurns;
    private bool _ableToContinue;
    
    private bool _hasSurrendered = false;
        
    private List<Unit> _activeUnits;
    private List<Unit> _reservedUnits;
    private List<Unit> _sortedActiveUnitsByOrderOfAttack;

    public Player(string name)
    {
        this._name = name;
        this._activeUnits = new List<Unit> { null, null, null, null };
        this._reservedUnits = new List<Unit>();
    }

    public void SetTeam(Team team)
    {
        this._team = team;
        SetActiveUnits();
        SetReserveUnits();
    }

   public void SetTeamValidation()
   {
       if (!this._team.HasSamurai() || this._team.SamuraiRepeated || !this._team.HasLessThanMaximumUnits()
            || this._team.IsAnyDemonRepeated() || this._team.HasSamuraiExceededMaxSkills()
            || this._team.HasSamuraiRepeatedSkills())
       {
           this._team.SetTeamAsInvalid();
       }
       else
       {
           this._team.SetTeamAsValid();
       }
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
        return this._fullTurns;
   }

   public int GetBlinkingTurns()
   {
        return this._blinkingTurns;
   }
   
   public List<Unit> GetActiveUnits()
   {
       return this._activeUnits;
   }
   
    public List<Unit> GetReservedUnits()
    {
         return this._reservedUnits;
    }

   public void SetTurns()
   {
       this._fullTurns = this._activeUnits.Count(unit => 
           unit != null && 
           unit.GetCurrentStats().GetStatByName("HP") > 0);
       
       this._blinkingTurns = 0;
   }
   
   public void SetActiveUnits()
   {
       Samurai samurai = this._team.Samurai;
       List<Demon> listDemons = this._team.Demons;
       
       this._activeUnits[0] = samurai;
       for (int i = 0; i < listDemons.Count && i < 3; i++)
       {
           this._activeUnits[i + 1] = listDemons[i];
       }
   }

   public void SetReserveUnits()
   {
       List<Demon> listDemons = this._team.Demons;

       for (int i = 0; i < listDemons.Count; i++)
       {
           if (listDemons[i] != null && !_activeUnits.Contains(listDemons[i]))
           {
               this._reservedUnits.Add(listDemons[i]);
           }
       }
   }

   public void UpdateTurnsBasedOnAffinity(string typeAttack, string nameTarget)
   {
       string targetAffinity = this.FindTargetInFileForStats(typeAttack, nameTarget);
       this.ConsumeTurnsBasedOnAffinity(targetAffinity);
   }

   public string FindTargetInFileForStats(string typeAttack, string nameTarget)
   {
       string resultOfSamuraiJson = this.SearchInJsonSamurai(typeAttack, nameTarget);
       string resultOfDemonsJson = this.SearchInJsonDemons(typeAttack, nameTarget);
       
       return (resultOfSamuraiJson != null) ? resultOfSamuraiJson : resultOfDemonsJson;
   }

   public string SearchInJsonSamurai(string typeAttack, string nameTarget)
   {
       string jsonString = File.ReadAllText(GameConstants.JSON_FILE_SAMURAI);
       JsonDocument document = JsonDocument.Parse(jsonString);
       JsonElement root = document.RootElement;
       
       foreach (JsonElement skillJSON in root.EnumerateArray())
       { 
           if (skillJSON.GetProperty("name").GetString() == nameTarget) 
           {
               return skillJSON.GetProperty("affinity").GetProperty($"{typeAttack}").GetString(); 
           } 
       }

       return null;
   }
   public string SearchInJsonDemons(string typeAttack, string nameTarget)
   {
       string jsonString = File.ReadAllText(GameConstants.JSON_FILE_MONSTERS);
       JsonDocument document = JsonDocument.Parse(jsonString);
       JsonElement root = document.RootElement;

       foreach (JsonElement demonJSON in root.EnumerateArray())
       {
           if (demonJSON.GetProperty("name").GetString() == nameTarget)
           {
               return demonJSON.GetProperty("affinity").GetProperty($"{typeAttack}").GetString();
           }
       }

       return null;
   }

   public void ConsumeTurnsBasedOnAffinity(string targetAffinity)
   {
       switch (targetAffinity)
       {
           case "Rp": 
           case "Dr":
               this._fullTurns = 0;
               this._blinkingTurns = 0;
               break;
           case "Nu":
               this._fullTurns = (this._fullTurns <= 2) ? 0 : (this._fullTurns - 2);
               break;
           // Miss
           case "Wk":
               
               break;
           case "-":
               if (this._blinkingTurns == 0)
               {
                   this._fullTurns--; 
               }
               else
               {
                   this._blinkingTurns--;
               }
               break;
           case "Rs":
               break;
           case "":
               break;
           
       }
   }
   
   public void CheckIfTeamIsAbleToContinue()
   {
       if (_hasSurrendered)
       {
           _ableToContinue = false;
           return;
       }

       foreach (Unit unit in _activeUnits)
       {
           if (unit != null && unit.GetCurrentStats().GetStatByName("HP") > 0)
           {
               _ableToContinue = true;
               return;
           }
       }

       _ableToContinue = false;
   }

    public bool IsTeamAbleToContinue()
    {
        return this._ableToContinue;
    }

    public void Surrender()
    {
        _hasSurrendered = true;
        _ableToContinue = false;
    }
    
    public bool IsPlayerOutOfTurns()
    {
        return this._fullTurns == 0 && this._blinkingTurns == 0;
    }

    public void SetOrderOfAttackOfActiveUnits()
    {
        _sortedActiveUnitsByOrderOfAttack = this._activeUnits
            .Where(unit => unit != null &&
                   !(unit is Samurai && unit.GetCurrentStats().GetStatByName("HP") <= 0) &&
                   unit.GetCurrentStats().GetStatByName("HP") > 0)
            .OrderByDescending(unit => unit.GetCurrentStats().GetStatByName("Spd"))
            .ToList();
    }
    
    public void ReorderUnitsWhenAttacked()
    {
        if (_sortedActiveUnitsByOrderOfAttack.Count > 0)
        {
            Unit firstUnit = _sortedActiveUnitsByOrderOfAttack[0];
            
            _sortedActiveUnitsByOrderOfAttack.RemoveAt(0);
            _sortedActiveUnitsByOrderOfAttack.Add(firstUnit);
        }
    }
    
    public List<Unit> GetSortedActiveUnitsByOrderOfAttack()
    {
        return _sortedActiveUnitsByOrderOfAttack;
    }

   public void RemoveFromActiveUnitsIfDead()
   {
       for (int i = 0; i < _activeUnits.Count; i++)
       {
           if (_activeUnits[i] != null &&
               _activeUnits[i].GetCurrentStats().GetStatByName("HP") <= 0)
           {
               RemoveFromSortedUnits(_activeUnits[i].GetName());
   
               if (!(_activeUnits[i] is Samurai))
               {
                   _activeUnits[i] = null;
               }
           }
       }
       
       CheckIfTeamIsAbleToContinue();
   }
   
   public void RemoveFromSortedUnits(string nameUnit)
   {
       if (_sortedActiveUnitsByOrderOfAttack == null)
           return;
           
       for (int i = 0; i < _sortedActiveUnitsByOrderOfAttack.Count; i++)
       {
           if (_sortedActiveUnitsByOrderOfAttack[i] != null &&
               _sortedActiveUnitsByOrderOfAttack[i].GetName() == nameUnit)
           {
               _sortedActiveUnitsByOrderOfAttack.RemoveAt(i);
               break;
           }
       }
   }

   public void ReplaceFromSortedListWhenInvoked(string nameUnit, Demon newDemon)
   {
       for (int i = 0; i < _sortedActiveUnitsByOrderOfAttack.Count; i++)
       {
           if (_sortedActiveUnitsByOrderOfAttack[i] != null &&
               _sortedActiveUnitsByOrderOfAttack[i].GetName() == nameUnit)
           {
               _sortedActiveUnitsByOrderOfAttack[i] = newDemon;
               break;
           }
       }
   }
   
   public void ReplaceFromReserveUnitsList(string nameDemonBeingRemoved, Demon demonBeginAddedToReserve)
   {
       for (int i = 0; i < _reservedUnits.Count; i++)
       {
           if (_reservedUnits[i] != null &&
               _reservedUnits[i].GetName() == nameDemonBeingRemoved)
           {
               _reservedUnits[i] = demonBeginAddedToReserve;
               break;
           }
       }
   }

   public List<Unit> GetValidActiveUnits()
   {
       return _activeUnits.Where(unit => unit != null && unit.GetCurrentStats().GetStatByName("HP") > 0).ToList();
   }
   
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
}