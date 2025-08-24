using Clonesoft.Json;
using SimpleProgression.Interfaces;
using SimpleProgression.Models.Progression;
using System;
using System.Collections.Generic;
using System.IO;

namespace SimpleProgression.Core;

public class LocalProgressionManager
{
    public static event Action<ExpeditionSession> OnExpeditionEntered;
    public static event Action<ExpeditionCompletionData> OnExpeditionCompleted;


    private static LocalProgressionManager _instance;
    public static LocalProgressionManager Instance => _instance ??= new LocalProgressionManager(Plugin.L);

    private readonly ILogger _logger;

    private LocalProgressionManager(ILogger logger)
    {
        _logger = logger;
    }

    public ExpeditionSession CurrentActiveSession { get; private set; }

    /// <summary>
    /// Loaded Rundown Progression Data<br/>
    /// Key: <c>Local_{RundownDBPersistentID}</c> // e.g. <c>Local_41</c>
    /// </summary>
    public Dictionary<string, LocalRundownProgression> LoadedProgressionData { get; } = new();

    /// <summary>
    /// Try to get loaded progression data.
    /// </summary>
    /// <param name="rundownKey">The rundown key of the progression data.</param>
    /// <param name="progression">The loaded progression data or null if false.</param>
    /// <returns><c>True</c> if the progression file is loaded.</returns>
    public bool TryGetLocalProgression(string rundownKey, out LocalRundownProgression progression)
    {
        return LoadedProgressionData.TryGetValue(rundownKey, out progression);
    }

    public LocalRundownProgression GetOrCreateLocalProgression(uint rundownDataBlockPersistentId)
    {
        return GetOrCreateLocalProgression($"Local_{rundownDataBlockPersistentId}");
    }
    
    public LocalRundownProgression GetOrCreateLocalProgression(string rundownKeyToLoad)
    {
        if (string.IsNullOrWhiteSpace(rundownKeyToLoad))
            throw new ArgumentException(null, nameof(rundownKeyToLoad));
        
        if (TryGetLocalProgression(rundownKeyToLoad, out var progression))
        {
            return progression;
        }
        
        var loadedProgression = LoadFromProgressionFile(rundownKeyToLoad);

        LoadedProgressionData.Add(rundownKeyToLoad, loadedProgression);
        
        return loadedProgression;
    }

    public void StartNewExpeditionSession(string rundownKey, string expeditionId, string sessionId)
    {
        CurrentActiveSession = ExpeditionSession.InitNewSession(rundownKey, expeditionId, sessionId, _logger);
    }

    public void OnLevelEntered()
    {
        CurrentActiveSession?.OnLevelEntered();
        OnExpeditionEntered?.Invoke(CurrentActiveSession);
    }

    internal void IncreaseLayerProgression(string strLayer, string strState)
    {
        if (!Enum.TryParse<Layers>(strLayer, out var layer)
            | !Enum.TryParse<LayerState>(strState, out var state))
        {
            _logger.Error($"Either {nameof(Layers)} and/or {nameof(LayerState)} could not be parsed! ({strLayer}, {strState})");
            return;
        }

        CurrentActiveSession?.SetLayer(layer, state);
    }

    internal void SaveAtCheckpoint()
    {
        CurrentActiveSession?.OnCheckpointSave();
    }

    internal void ReloadFromCheckpoint()
    {
        CurrentActiveSession?.OnCheckpointReset();
    }

    internal void ArtifactCountUpdated(int mutedCount, int boldCount, int aggressiveCount)
    {
        if (CurrentActiveSession == null)
            return;

        CurrentActiveSession.MutedArtifactsCollected = mutedCount;
        CurrentActiveSession.BoldArtifactsCollected = boldCount;
        CurrentActiveSession.AggressiveArtifactsCollected = aggressiveCount;
        _logger.Info($"current Artifact count: Muted:{mutedCount}, Bold:{boldCount}, Aggressive:{aggressiveCount}");
    }

    internal void EndCurrentExpeditionSession(bool success)
    {
        CurrentActiveSession?.OnExpeditionCompleted(success);

        var rundownKey = CurrentActiveSession?.RundownKey;
        
        var progressionFile = GetOrCreateLocalProgression(rundownKey);

        var hasCompletionData = progressionFile.AddSessionResults(CurrentActiveSession, out var completionData);

        SaveToProgressionFile(progressionFile, rundownKey);
        
        CurrentActiveSession = null;

        if (!hasCompletionData)
            return;
        
        _logger.Notice($"Expedition time: {completionData.RawSessionData.EndTime - completionData.RawSessionData.StartTime}");

        OnExpeditionCompleted?.Invoke(completionData);
    }

    private void SaveToProgressionFile(LocalRundownProgression data, string rundownKey)
    {
        SaveToProgressionFile(data, rundownKey, out var path);
        Instance._logger.Msg(ConsoleColor.DarkRed, $"Saved progression file to disk at: {path}");
    }

    private static void SaveToProgressionFile(LocalRundownProgression data, string rundownKeyToSave, out string path)
    {
        if (data == null)
            throw new ArgumentNullException(nameof(data));
        if (string.IsNullOrEmpty(rundownKeyToSave))
            throw new InvalidOperationException(nameof(rundownKeyToSave));

        path = GetLocalProgressionFilePath(rundownKeyToSave);

        var json = JsonConvert.SerializeObject(data, Formatting.Indented);
        File.WriteAllText(path, json);
    }

    private static string GetLocalProgressionFilePath(string rundownKey)
    {
        foreach(var c in Path.GetInvalidFileNameChars())
        {
            rundownKey = rundownKey.Replace(c, '_');
        }

        return Path.Combine(Paths.SaveFolderPath, $"{rundownKey}.json");
    }

    private LocalRundownProgression LoadFromProgressionFile(string rundownKey)
    {
        var loadedLocalProgressionData = LoadFromProgressionFile(rundownKey, out var path, out var isNew);

        if (isNew)
        {
            Instance._logger.Msg(ConsoleColor.Green, $"Created progression file at: {path}");
            SaveToProgressionFile(loadedLocalProgressionData, rundownKey, out var initialSavePath);
            Instance._logger.Msg(ConsoleColor.DarkRed, $"Saved fresh progression file to disk at: {initialSavePath}");
        }
        else
        {
            Instance._logger.Msg(ConsoleColor.Green, $"Loaded progression file from disk at: {path}");
        }

        return loadedLocalProgressionData;
    }

    private static LocalRundownProgression LoadFromProgressionFile(string rundownKey, out string path, out bool isNew)
    {
        path = GetLocalProgressionFilePath(rundownKey);

        if (!File.Exists(path))
        {
            isNew = true;
            return new LocalRundownProgression();
        }

        isNew = false;

        var json = File.ReadAllText(path);

        return JsonConvert.DeserializeObject<LocalRundownProgression>(json);
    }
}