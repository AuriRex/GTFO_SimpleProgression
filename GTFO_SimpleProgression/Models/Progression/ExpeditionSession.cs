using SimpleProgression.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleProgression.Models.Progression;

public class ExpeditionSession
{
    private ExpeditionSessionData SavedData { get; set; }
    public ExpeditionSessionData CurrentData { get; private set; }
    public bool HasCheckpointBeenUsed { get; private set; }
    public bool ExpeditionSurvived { get; private set; }
    public DateTimeOffset DropTime { get; private set; }
    public DateTimeOffset StartTime { get; private set; }
    public DateTimeOffset EndTime { get; private set; }

    private readonly ILogger _logger;
    public string RundownKey { get; private set; }
    public string ExpeditionId { get; private set; }
    public string SessionId { get; private set; }

    public int ArtifactsCollected => MutedArtifactsCollected + BoldArtifactsCollected + AggressiveArtifactsCollected;
    public int MutedArtifactsCollected { get; internal set; }
    public int BoldArtifactsCollected { get; internal set; }
    public int AggressiveArtifactsCollected { get; internal set; }

    public bool PrisonerEfficiencyCompleted
    {
        get
        {
            // PrisonerEfficiency => All 3 objectives (Main, Extreme, Overload) have been completed
            return CurrentData.LayerStates.Count() == 3
                   && CurrentData.LayerStates.All(x => x.Value == LayerState.Completed);
        }
    }

    private ExpeditionSession(string rundownKey, string expeditionId, string sessionId, ILogger logger)
    {
        RundownKey = rundownKey;
        ExpeditionId = expeditionId;
        SessionId = sessionId;
        _logger = logger;
        DropTime = DateTimeOffset.UtcNow;

        CurrentData = new ExpeditionSessionData(logger);

        SetLayer(Layers.Main, LayerState.Entered);
    }

    internal static ExpeditionSession InitNewSession(string rundownKey, string expeditionId, string sessionId, ILogger logger)
    {
        var session = new ExpeditionSession(rundownKey, expeditionId, sessionId, logger);

        logger.Info($"[{nameof(ExpeditionSession)}] New expedition session started! (R:{rundownKey}, E:{expeditionId}, S:{sessionId})");
            
        return session;
    }

    internal void OnLevelEntered()
    {
        StartTime = DateTimeOffset.UtcNow;
    }

    internal void OnCheckpointSave()
    {
        _logger.Info($"Saving current {nameof(ExpeditionSessionData)} at checkpoint.");
        SavedData = CurrentData.Clone();
    }

    internal void OnCheckpointReset()
    {
        if (!HasCheckpointBeenUsed)
            _logger.Notice("Checkpoint has been used!");
        HasCheckpointBeenUsed = true;
        if(SavedData != null)
        {
            _logger.Info($"Resetting previous {nameof(ExpeditionSessionData)} from checkpoint.");
            CurrentData = SavedData.Clone();
        }
    }

    internal void OnExpeditionCompleted(bool success)
    {
        EndTime = DateTimeOffset.UtcNow;

        _logger.Info($"[{nameof(ExpeditionSession)}] Expedition session has ended! (R:{RundownKey}, E:{ExpeditionId}, S:{SessionId}){(success ? " Expedition Successful!" : string.Empty)}");

        if (success)
        {
            ExpeditionSurvived = true;
            SetLayer(Layers.Main, LayerState.Completed);
        }

        _logger.Info($"[{nameof(ExpeditionSession)}] Data: {CurrentData}");
    }

    internal void SetLayer(Layers layer, LayerState state)
    {
        CurrentData.SetOnlyIncreaseLayerState(layer, state);
    }

    public bool HasLayerBeenCompleted(Layers layer)
    {
        if (!CurrentData.LayerStates.TryGetValue(layer, out var state)) return false;

        return state == LayerState.Completed;
    }

    public class ExpeditionSessionData
    {
        public Dictionary<Layers, LayerState> LayerStates { get; private set; } = new Dictionary<Layers, LayerState>();

        private readonly ILogger _logger;

        internal ExpeditionSessionData(ILogger logger)
        {
            _logger = logger;
        }

        internal void SetOnlyIncreaseLayerState(Layers layer, LayerState state)
        {
            if (LayerStates.TryGetValue(layer, out var currentState))
            {
                if((int)currentState < (int)state)
                {
                    LayerStates.Remove(layer);
                    _logger.Debug($"[{nameof(ExpeditionSessionData)}] Set layer {layer} from {currentState} to {state}");
                    LayerStates.Add(layer, state);
                }
                return;
            }

            _logger.Debug($"[{nameof(ExpeditionSessionData)}] Set layer {layer} to {state}");
            LayerStates.Add(layer, state);
        }

        internal void SetLayerState(Layers layer, LayerState state)
        {
            if(LayerStates.TryGetValue(layer, out var currentState))
            {
                LayerStates.Remove(layer);
                _logger.Debug($"[{nameof(ExpeditionSessionData)}] Set layer {layer} from {currentState} to {state}");
            }
            else
            {
                _logger.Debug($"[{nameof(ExpeditionSessionData)}] Set layer {layer} to {state}");
            }
            LayerStates.Add(layer, state);
        }

        public override string ToString()
        {
            string ret = string.Empty;

            foreach(var kvp in LayerStates)
            {
                ret += $"{kvp.Key}: {kvp.Value}, ";
            }

            return ret.Substring(0, ret.Length-2);
        }

        public ExpeditionSessionData Clone()
        {
            var newExpSD = new ExpeditionSessionData(_logger);

            foreach(var kvp in LayerStates)
            {
                newExpSD.LayerStates.Add(kvp.Key, kvp.Value);
            }

            return newExpSD;
        }
    }
}