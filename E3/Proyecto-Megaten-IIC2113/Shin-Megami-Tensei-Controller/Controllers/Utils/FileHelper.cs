using System.Text.Json;

namespace Shin_Megami_Tensei.String_Handlers;

public static class FileHelper
{
    public static string GetFileName(int fileNumber, string _teamsFolder)
    {
        if (fileNumber < 9)
            return $"{_teamsFolder}/00{fileNumber + 1}.txt";
        else if (fileNumber < 99)
            return $"{_teamsFolder}/0{fileNumber + 1}.txt";
        else
            return $"{_teamsFolder}/{fileNumber + 1}.txt";
    }
    
    public static string FindTargetInFileForStats(string typeAttack, string nameTarget)
    {
        string resultOfSamuraiJson = SearchInJsonSamurai(typeAttack, nameTarget);
        string resultOfDemonsJson = SearchInJsonDemons(typeAttack, nameTarget);
       
        return (resultOfSamuraiJson != null) ? resultOfSamuraiJson : resultOfDemonsJson;
    }   
    
    public static string SearchInJsonSamurai(string typeAttack, string nameTarget)
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

    public static string SearchInJsonDemons(string typeAttack, string nameTarget)
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
}