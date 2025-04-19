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

}