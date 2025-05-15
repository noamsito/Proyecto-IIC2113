using System.Diagnostics;
using System.Text.Json;
using Shin_Megami_Tensei_View;
using Shin_Megami_Tensei.Combat;
using Shin_Megami_Tensei.Managers;
using Shin_Megami_Tensei.String_Handlers;
using Shin_Megami_Tensei.Units;

namespace Shin_Megami_Tensei;

public class Player
{
    private string _name;
    private Team _team;
    
    private int _fullTurns;
    private int _blinkingTurns;
    private bool _ableToContinue;
    private int _constOfHits = 0;
    
    private bool _hasSurrendered = false;
        
    private List<Unit> _activeUnits;
    private List<Unit> _reservedUnits;
    private List<Unit> _sortedActiveUnitsByOrderOfAttack;
    

    public Player(string name)
    {
        // PlayerData data = new PlayerData(string name);
        
        this._name = name;
        this._activeUnits = new List<Unit> { null, null, null, null };
        this._sortedActiveUnitsByOrderOfAttack = new List<Unit> { null, null, null, null };
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
                   _reservedUnits.Add(_activeUnits[i]);
                   _activeUnits[i] = null;
               }
           }
       }
       
       CheckIfTeamIsAbleToContinue();
   }
   
   public List<int> GetValidSlotsFromActiveUnitsAndDisplayIt()
   {
       var activeUnits = GetActiveUnits();
       List<int> validSlots = new();
        
       for (int iterator = 1; iterator < activeUnits.Count; iterator++)
       {
           if (activeUnits[iterator] == null || activeUnits[iterator].GetCurrentStats().GetStatByName("HP") <= 0)
           {
               CombatUI.DisplayEmptySlot(validSlots, iterator);
           }
           else
           {
               CombatUI.DisplayDemonInSlot(validSlots, iterator, activeUnits[iterator]);
           }
           validSlots.Add(iterator);
       }
       
       CombatUI.DisplayCancelOption(validSlots.Count);
       return validSlots;
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
               _sortedActiveUnitsByOrderOfAttack[i] = null;
               break;
           }
       }
   }

   public void ReplaceFromSortedListWhenInvoked(Unit oldDemon, Unit newDemon)
   {
       bool wasFoundPreviousDemon = false;
       for (int i = 0; i < _sortedActiveUnitsByOrderOfAttack.Count; i++)
       {
           AddTheDemonInTheAvailableSlot(i, (Demon)oldDemon, (Demon)newDemon);
       }
   }

   public void AddTheDemonInTheAvailableSlot(int iteratorSlots, Demon oldDemon, Demon newDemon)
   {
       if (_sortedActiveUnitsByOrderOfAttack[iteratorSlots].GetName() == oldDemon.GetName())
       {
           _sortedActiveUnitsByOrderOfAttack[iteratorSlots] = newDemon;
       }
   }

   public void AddDemonInTheLastSlot(Demon newDemon)
   {
       _sortedActiveUnitsByOrderOfAttack.Add(newDemon);
   }
   
   public void ReplaceFromReserveUnitsList(string newName, Demon replacedDemon)
   {
       for (int i = 0; i < _reservedUnits.Count; i++)
       {
           if (_reservedUnits[i] != null && _reservedUnits[i].GetName() == newName)
           {
               _reservedUnits[i] = replacedDemon;
               return;
           }
       }
   }

   public void ReorderReserveBasedOnJsonOrder()
   {
       var orderedDemons = _team.Demons
           .Where(demon => _reservedUnits.Contains(demon))
           .ToList();

       _reservedUnits = orderedDemons.Cast<Unit>().ToList();
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
   
   public int GetConstantKPlayer( )
   {
       return _constOfHits;
   }
   
   public void IncreaseConstantKPlayer()
   { 
       _constOfHits++;
   }
}