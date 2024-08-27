namespace SimpleProgression.Models.Progression
{
    public struct ExpeditionCompletionData
    {
        public readonly bool Success => RawSessionData.ExpeditionSurvived;
        public readonly string RundownIdString => RawSessionData.RundownId;
        public uint RundownId { get; internal set; }
        public eRundownTier ExpeditionTier { get; internal set; }
        public int ExpeditionIndex { get; internal set; }
        public readonly string ExpeditionId => RawSessionData.ExpeditionId;
        public readonly string SessionId => RawSessionData.SessionId;
        public readonly int ArtifactsCollected => RawSessionData.ArtifactsCollected;
        public readonly bool WasPrisonerEfficiencyClear => RawSessionData.PrisonerEfficiencyCompleted;
        public bool WasFirstTimeCompletion { get; internal set; }
        public ExpeditionSession RawSessionData { get; internal set; }

        /// <summary>
        /// Artifact Heat before this session
        /// </summary>
        public float PreArtifactHeat { get; internal set; }

        /// <summary>
        /// Artifact Heat after session completion
        /// </summary>
        public float NewArtifactHeat { get; internal set; }
    }
}
