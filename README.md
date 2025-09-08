# Simple Progression

An improved version of the LocalProgression Feature from [TheArchive](https://github.com/AuriRex/GTFO_TheArchive) with some extra additions for custom rundown developers.

Completely removes the dependency on PlayFab servers,

### Saves progression locally and is able to drop both Vanity items and Boosters (also local)!

## Functionality
| Feature           | ❔ |
|-------------------|---|
| Level Completions | ✅ |
| Vanity Items      | ✅ |
| Boosters          | ✅ |
| Artifact Heat     | ✅ |
| Lore Logs         | ✅ |

### Compatible with the base game DataBlocks:
* Vanity
  * `VanityItemsGroupDataBlock`: Drop random item from group (ex. on level completion)
  * `VanityItemsLayerDropsDataBlock`: Drop items after completing x number of objectives
* Boosters
  * `BoosterImplantTemplateDataBlock`: Templates used to define boosters
  * `BoosterImplantEffectDataBlock`: Booster Effects and their min/max values
  * `BoosterImplantConditionDataBlock`: Conditions needed for the effects to trigger

*Or drop items manually via code*

### Special All-Vanity interop

If All-Vanity is installed, SimpleProgression is responsible for unlocking all items instead.  
That way it's easier to progression lock custom items, but still unlock all vanilla ones.  
The file `BepInEx/config/SimpleProgression_VanityGroupUnlockData.json` specifies which ItemGroups to drop.

### For rundown devs:

SimpleProgression fully supports `UnlockedByExpedition` and `CustomProgressionLock` in your RundownDatablocks ExpeditionInTier data!  
This can be circumvented, for both devs or players that want to have access to all levels no matter their current completions, by setting `"UnlockAllLevels": true` in the `BepInEx/config/SimpleProgression_Config.json` config file.  

The plugin also drops items from the groups specified in `VanityItemsDropData` (in ExpeditionInTier) on first time completion of a level.  
By specifying a proper value in the `VanityItemLayerDropDataBlock` field (in RundownDataBlock), you're able to drop vanity items based on X completed level layers per rundown.

There are a few other things for rundown devs to modify in this config file:

```json
{
  "UnlockAllLevels": false,
  "ShowScrambledTimer": false,
  "MaxBoosterCountPerCategory": 20,
  "ArtifactRewardMultiplier": 1.0,
  "NewBoosterDropCost": 1000,
  "ForceOneHundredArtifactHeat": false,
  "DropRandomization": {
    "MutedUsesMin": 1,
    "MutedUsesMax": 1,
    "BoldUsesMin": 1,
    "BoldUsesMax": 2,
    "AggressiveUsesMin": 2,
    "AggressiveUsesMax": 3
  }
}
```

* `UnlockAllLevels`: If progression lock should be ignored.
* `ShowScrambledTimer`: Shows a glitching element below the rundown header.
* `MaxBoosterCountPerCategory`: The amount of unique boosters per category should be allowed.
* `ArtifactRewardMultiplier`: This value directly multiplies the gained booster currency awarded by collected artifacts.  
  (Note: Using this will desync the visual booster value on the warden intel and on the end screen.)
* `NewBoosterDropCost`: How much currency is required for a single booster to drop.  
  (Note: With 100% Artifact Heat, one artifact is worth ~150 currency)
* `ForceOneHundredArtifactHeat`: Force artifact heat to stay pinned at 100%  
  (Note: Proper values are still being calculated even if this is on.)
* `DropRandomization`: How many uses the different booster categories should be generated with.  
  (Note: Minimum (Inclusive) - Maximum (Inclusive))
