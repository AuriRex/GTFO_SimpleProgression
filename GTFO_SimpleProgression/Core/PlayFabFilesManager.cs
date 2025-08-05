using System;
using System.Collections.Generic;
using System.IO;

namespace SimpleProgression.Core;

public static class PlayFabFilesManager
{
    internal const bool SKIP_OG = false;
    
    internal static void OnLoggedIn()
    {
        ReadAllFilesFromDisk();
    }

    private static readonly Dictionary<string, string> _localPlayerTitleData = new();

    private static void ReadAllFilesFromDisk()
    {
        _localPlayerTitleData.Clear();
        foreach (var file in Directory.EnumerateFiles(Paths.LocalTitleDataFolderPath, "*.json"))
        {
            Plugin.L.Msg(ConsoleColor.Green, $"Reading LocalTitleDataFile file: {Path.GetFileName(file)}");
            var contents = File.ReadAllText(file);
            var fileName = Path.GetFileName(file).Replace(".json", string.Empty);
        
            _localPlayerTitleData.Add(fileName, contents);
            PlayFabManager.Current.m_localPlayerData[fileName] = contents;
        }
    }

    private static void SaveAllLocalPlayerTitleDataToDisk()
    {
        foreach (var (fileName, contents) in _localPlayerTitleData)
        {
            var path = Path.Combine(Paths.LocalTitleDataFolderPath, $"{fileName}.json");
            File.WriteAllText(path, contents);
        }
    }
    
    internal static bool SkipOriginalAndInvoke(Il2CppSystem.Action action)
    {
        action?.Invoke();
        return SKIP_OG;
    }

    public static void UpdateLocalPlayerTitleData(string key, string value)
    {
        _localPlayerTitleData[key] = value;
        PlayFabManager.LocalPlayerTitleData[key] = value;

        SaveAllLocalPlayerTitleDataToDisk();
    }
}