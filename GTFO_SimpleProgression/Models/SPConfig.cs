namespace SimpleProgression.Models;

public class SPConfig
{
    public bool UnlockAllLevels { get; set; }
    public uint MaxBoosterCountPerCategory { get; set; } = 20;
    public float ArtifactRewardMultiplier { get; set; } = 1f;
    public int NewBoosterDropCost { get; set; } = 1_000;

    /// <summary>
    /// Forces artifact heat to be 100% for each level.<br/>
    /// (Real values are still being calculated every time artifacts are collected / a level is completed)
    /// </summary>
    public bool ForceOneHundredArtifactHeat { get; set; }
    public RandomizationOptions DropRandomization { get; set; } = new();
    
    public class RandomizationOptions
    {
        public int MutedUsesMin { get; set; } = 1;
        public int MutedUsesMax { get; set; } = 1;
        
        public int BoldUsesMin { get; set; } = 1;
        public int BoldUsesMax { get; set; } = 2;
        
        public int AggressiveUsesMin { get; set; } = 2;
        public int AggressiveUsesMax { get; set; } = 3;
    }
}