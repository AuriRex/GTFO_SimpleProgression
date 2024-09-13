# Simple Progression

A spinoff version of the LocalProgression Feature from [TheArchive](https://github.com/AuriRex/GTFO_TheArchive) but adapted specifically for modded rundowns.

### Saves progression locally and is able to drop both Vanity items and Boosters (also local)!
| | |
|---|---|
|Level Completions|✅|
|Vanity Items|✅|
|Boosters|✅|
|API for plugin devs|WIP|

### Compatible with the base game DataBlocks:
* Vanity
    * `VanityItemsGroupDataBlock`: Drop random item from group (ex. on level completion)
    * `VanityItemsLayerDropsDataBlock`: Drop items after completing x number of objectives
* Boosters
    * `BoosterImplantTemplateDataBlock`: Templates used to define boosters
    * `BoosterImplantEffectDataBlock`: Booster Effects and their min/max values
    * `BoosterImplantConditionDataBlock`: Conditions needed for the effects to trigger

*Or drop items manually via code*

