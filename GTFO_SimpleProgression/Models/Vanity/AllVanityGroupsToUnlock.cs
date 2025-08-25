using System.Collections.Generic;
using Clonesoft.Json;

namespace SimpleProgression.Models.Vanity;

public class AllVanityGroupsToUnlock
{
    public string Comment { get; set; } = "This are all the VanityItemsGroupDataBlocks (name) that will be unlocked by Simple Progression if AllVanity is installed.";
    public List<string> GroupsToUnlock { get; set; } = new(GAME_DEFAULT_GROUPS);
    
    [JsonIgnore]
    public static readonly IReadOnlyList<string> GAME_DEFAULT_GROUPS = new List<string>()
    {
        "Initial Helmet DONOTDELETE",
        "Initial Torso DONOTDELETE",
        "Initial Legs DONOTDELETE",
        "Initial Backpack DONOTDELETE",
        "Initial Palette DONOTDELETE",
        "MainDrops",
        "MainStoryDrops",
        "SecondaryDrops",
        "OverloadDrops",
        "MainCompletedDrops",
        "StoryCompletedDrops",
        "SecondaryCompletedDrops",
        "OverloadCompletedDrops",
        "R7MainStoryDrops",
        "R7SecondaryDrops",
        "R7OverloadDrops",
        "R7MainCompletedDrops",
        "R7StoryCompletedDrops",
        "R7SecondaryCompletedDrops",
        "R7OverloadCompletedDrops",
        "R1Completion",
        "R2Completion",
        "R3Completion",
        "R4Completion",
        "R5Completion",
        "R6Completion",
        "R8Completion",
        "EarlyAccessDrops DONOTDELETE",
        "ChineseNewYear2022 DONOTDELETE",
        "ExtraExpeditions6.5 DONOTDELETE",
        "Gamescom2022 DONOTDELETE",
        "Halloween2022 DONOTDELETE",
        "BlackFriday2022 DONOTDELETE",
        "ChineseNewYear2023 DONOTDELETE",
        "ALT R4 Timed Drop 2023 DONOTDELETE",
        "Halloween2023 DONOTDELETE"
    };
}