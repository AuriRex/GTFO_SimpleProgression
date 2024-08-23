using Clonesoft.Json;
using SimpleProgression.Interfaces;
using SimpleProgression.Progression;
using System;
using System.IO;

namespace SimpleProgression
{
    public class LocalProgressionManager
    {
        public static LocalProgressionManager Instance { get; private set; }

        public static string SavePath { get; private set; }

        private readonly ILogger _logger;

        public LocalProgressionManager(ILogger logger)
        {
            if (Instance != null)
                throw new InvalidOperationException("One instance already exists!");

            SavePath = Path.Combine(BepInEx.Paths.BepInExRootPath, "LocalProgression/");
            Directory.CreateDirectory(SavePath);
            _logger = logger;
            Instance = this;
        }

        public ExpeditionSession CurrentActiveSession { get; private set; }

        public static event Action<ExpeditionCompletionData> OnExpeditionCompleted;

        public LocalRundownProgression CurrentLoadedLocalProgressionData { get; private set; } = null;

        public bool HasLocalRundownProgressionLoaded => CurrentLoadedLocalProgressionData != null;

        private string _loadedRundownKey;

        public LocalRundownProgression GetOrCreateLocalProgression(string rundownKeyToLoad)
        {
            if (string.IsNullOrEmpty(rundownKeyToLoad))
                throw new ArgumentException(nameof(rundownKeyToLoad));

            if (!HasLocalRundownProgressionLoaded)
            {
                CurrentLoadedLocalProgressionData = LoadFromProgressionFile(rundownKeyToLoad);
                return CurrentLoadedLocalProgressionData;
            }

            if (rundownKeyToLoad == _loadedRundownKey)
                return CurrentLoadedLocalProgressionData;

            _logger.Debug($"{nameof(GetOrCreateLocalProgression)}() {nameof(rundownKeyToLoad)} changed. ({_loadedRundownKey} -> {rundownKeyToLoad})");

            SaveToProgressionFile(CurrentLoadedLocalProgressionData);

            CurrentLoadedLocalProgressionData = LoadFromProgressionFile(rundownKeyToLoad);

            return CurrentLoadedLocalProgressionData;
        }

        public void StartNewExpeditionSession(string rundownId, string expeditionId, string sessionId)
        {
            CurrentActiveSession = ExpeditionSession.InitNewSession(rundownId, expeditionId, sessionId, _logger);
        }

        public void OnLevelEntered()
        {
            CurrentActiveSession?.OnLevelEntered();
        }

        public void IncreaseLayerProgression(string strLayer, string strState)
        {
            if (!Enum.TryParse<Layers>(strLayer, out var layer)
                | !Enum.TryParse<LayerState>(strState, out var state))
            {
                _logger.Error($"Either {nameof(Layers)} and/or {nameof(LayerState)} could not be parsed! ({strLayer}, {strState})");
                return;
            }

            CurrentActiveSession?.SetLayer(layer, state);
        }

        public void SaveAtCheckpoint()
        {
            CurrentActiveSession?.OnCheckpointSave();
        }

        public void ReloadFromCheckpoint()
        {
            CurrentActiveSession?.OnCheckpointReset();
        }

        public void ArtifactCountUpdated(int count)
        {
            if (CurrentActiveSession == null) return;
            CurrentActiveSession.ArtifactsCollected = count;
            _logger.Info($"current Artifact count: {count}");
        }

        public void EndCurrentExpeditionSession(bool success)
        {
            CurrentActiveSession?.OnExpeditionCompleted(success);

            GetOrCreateLocalProgression(CurrentActiveSession.RundownId);

            var hasCompletionData = CurrentLoadedLocalProgressionData.AddSessionResults(CurrentActiveSession, out var completionData);

            CurrentActiveSession = null;

            SaveToProgressionFile(CurrentLoadedLocalProgressionData);

            if (hasCompletionData)
            {
                _logger.Notice($"Expedition time: {completionData.RawSessionData.EndTime - completionData.RawSessionData.StartTime}");

                OnExpeditionCompleted?.Invoke(completionData);
            }
        }

        public void SaveToProgressionFile(LocalRundownProgression data)
        {
            SaveToProgressionFile(data, _loadedRundownKey, out var path);
            Instance._logger.Msg(ConsoleColor.DarkRed, $"Saved progression file to disk at: {path}");
        }

        public static void SaveToProgressionFile(LocalRundownProgression data, string rundownKeyToSave, out string path)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (string.IsNullOrEmpty(rundownKeyToSave))
                throw new InvalidOperationException(nameof(rundownKeyToSave));

            path = GetLocalProgressionFilePath(rundownKeyToSave);

            var json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(path, json);
        }

        public static string GetLocalProgressionFilePath(string rundownKey)
        {
            foreach(var c in Path.GetInvalidFileNameChars())
            {
                rundownKey = rundownKey.Replace(c, '_');
            }

            return Path.Combine(SavePath, $"{rundownKey}.json");
        }

        public LocalRundownProgression LoadFromProgressionFile(string rundownKey)
        {
            var loadedLocalProgressionData = LoadFromProgressionFile(rundownKey, out var path, out var isNew);
            _loadedRundownKey = rundownKey;

            if (isNew)
            {
                Instance._logger.Msg(ConsoleColor.Green, $"Created progression file at: {path}");
                SaveToProgressionFile(loadedLocalProgressionData, _loadedRundownKey, out var initialSavePath);
                Instance._logger.Msg(ConsoleColor.DarkRed, $"Saved fresh progression file to disk at: {initialSavePath}");
            }
            else
            {
                Instance._logger.Msg(ConsoleColor.Green, $"Loaded progression file from disk at: {path}");
            }

            return loadedLocalProgressionData;
        }

        public static LocalRundownProgression LoadFromProgressionFile(string rundownKey, out string path, out bool isNew)
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
}
