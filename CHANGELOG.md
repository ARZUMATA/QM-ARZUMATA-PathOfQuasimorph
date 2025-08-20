# Changelog
## 1.0 (be3c4c6)
* Initial release

## 1.1 (8d85683)
* Fixed new game not starting issue when mod is enabled.
* Some files were not uploaded to the workshop, making mod inactive.
* Mod manifest updated, so it doesn't appear in mod list for stable version.

## 1.2 (92e9f12)
- Rarity mods are now better applied.
- One stat is boosted little more that defines it's suffix.
- Improved resist boosts to use min and max values for better consistency.
- Armor now uses an average resist value to determine which resist is randomly added if the roll is successful.
- Fixed incorrect boost logic.
- Enhanced logging for easier debugging and tracking of changes.

## 1.3 (850ae94)
 - Unbreakable trait now has weighed chance.
 - Parameters to modify are now selected within percentage range, rather than a fixed percent.
 - Fixed one issue where some items were not processed and were reset to vanilla ones. Loooking into it anyway.
 - Rarity csv file is now available for editing and overrides internal data. See mod config to enable it.
 - Tooltips for weapons to compare PoQ one with vanilla. Hotkey 'left shift'.

## 1.4 (aef44c6)
- Internal rnd calculations are corrected.
- Traits for ranged weapons and melee now have blacklists to remove absurd traits.
- Tooltips for armor resists.
- Average resist if applied is not that strong.
- Fixed error, making armor getting huge weight.

## 1.41 (d7997d6)
- Fix: For some users the modded item stats were too extreme due to regional settings and were applied wrong leading not only to visual bug but breaking item stats.
- Fix: Some items were becoming generic.
- Item prefixes were updated, so now armor/wepon prefix can determine it's type. 
Like "Overcharged" for shock damage weapon, "Cryo" for cold, Toxic for poison based as well as and "Sturdy" for armor. Etc.
Thanks community for feedback.

## 1.42 (1d5fcea)
- Fix: Some items were becoming generic. Yes, there were a bit more.
- Fix: In some cases, items were appearing in projects list like as they were unlocked.
- Add: Parameter hindering, now there is a chance that parameter will be lowered, instead of being increased as usual.
- Fix: Minor tooltip formatting isues.

## 1.43 (80a01dc)
- Fixed: Balance question. Conveyor crafted items that a project ones (yellow M), will no longer receive rarity.

## 1.5 (f9540b0)
- Improved logging with better class-specific logs.
- Enhanced item comparison tooltip with additional information.
- Creature generation now respects rarity and masteries, including an extra stats boost for items based on mob mastery.
- Fixed issue where files were being held in use by other applications.
- Added `MonsterInspectWindow` with perks, tooltips, and monster master tier support.
- Added new class sprite for the monster inspector window.
- Fixed deserialization issues and ensured correct monster stats are applied.
- Fixed incorrect tooltip extra info display for common folk.
- Fixed ID reuse in mob generation.
- Added support for monster ranks and talents.
- Added translation support for new content.
- Enabled an option to toggle mob generation.
- Updated helper functions and logger for better performance.
- Updated Unity assets.

## 1.6-pre (7935d11)
### Enhancements
- Major refactor of item serialization architecture (Magnum Projects removed)
- Added `MobModifier` rarity, items get boosted stats from mob rarity as well
- Added support for **augmentations**, **implants**

### Bug Fixes
- Fixed unbreakable entry display in tooltips

## 1.8 (81864aa)
- Augmentation and implants support
- Fix: magnum project upgrade limits were broken for vanilla items
- Added perk support for active implants
- Added tooltips for augmentations and implants
- Fixed localization issues
- Added translations for color names
- Added color settings to mod configuration
- Expanded rarity.csv with more editable data
- Various tooltip cosmetic fixes

## 1.8.1 (6202d68)
- Fix: some items were reset to default if you switch saves without restarting the game

## 1.8.2 (40f9eb0)
- Fixed issue where pressing Esc to save and quit during a mission did not preserve item metadata, causing items to revert to default values.

## 1.8.3 (145e1b3)
- Improved wound slot augmentation localization and descriptor lookup. (This was leading to savegame load errors if you apply rare-augmentation)
- Updated item processing logic to deny all by default, with explicit allowance after validation checks.

## 1.9 (5e1999f)
### Added
- **Synthraformers System**
  - Introduced new Synthraformer items with unique effects:
    - *Amplifier*: Boosts a random stat on an item, based on rarity; can roll hindered stats and allows upgrade during crafting.
    - *Traits Recombinator*: Recombines item traits completely, preserving only those from the item’s generic counterpart.
    - *Random Rarity*: Rolls a new random rarity on standard items.
    - *Indestructible Activator*: Grants a chance to add the Indestructible trait to an item.
    - *Augmentation Catalyst*: Adds a random positive or negative stat to augments; consumed regardless of stat presence.
  - Added associated recipes under the *Synthraformers* category in Magnum Crafting.

- **Tooltips**
  - Implemented full synthraformer tooltip support and localization integration.
  - Missing weapon traits now displayed in a darker color for clarity.

### Fixed
- Fixed boosted stats triggering incorrectly in certain cases.
- Prevented station-produced items from getting non-standard rarities or being flagged as custom weapons.
- Resolved duplicate suffix issue on magnum project custom weapons by relocating naming logic.
- Corrected aug/implant processing being disabled — semi-fixed broken augment behavior. This is under testing, ping me, send saves files and logs.
- Fixed cleanup system erroneously collecting wound slots and aug records.
- Safely handle missing records in creature data (wound slots) during load.
- Fixed one-line JSON-in-JSON serialization to reduce file size and improve readability.
- Avoid applying rarity changes when removing an augment if the resulting rarity is Standard.

### Changed
- Tweaked logger prefixes for improved debugging clarity.
- Reverted change that skipped rarity processing for Standard items to ensure consistent behavior.

### Patched
- Ammo and Metadata records now properly supported in synthraformer processing pipeline.

# Changelog

## 1.9.1 (d42c08f)

### Changes
- **Tooltip improvements**:
  - Ammo and weapon traits now correctly use `Mutually Exclusive Groups`, preventing duplicate or conflicting traits.
  - Missing weapon traits displayed in darker color for clarity.
- **Magnum-produced items**:
  - Now marked with an `"M"` label to distinguish them from other rarity items.
  - Tooltip comparisons now prioritize the modified counterpart if available; otherwise, fall back to generic base item.
  - *Note:* This only affects newly generated items — existing items are unchanged.
- **Font size adjustments**: Tooltip UI now scales text down when content doesn't fit, improving readability.

### Fixed
- **Monster stats**: Corrected logic that was mistakenly applying *hindered* stats by default instead of *boosted*.
- **Action Points (AP) & Line of Sight (LoS) tooltips**: Fixed incorrect values being displayed.
- **Augmentation slot mapping**: If savegame data is missing augmentation slot entries, the system now infers correct values and repairs the map automatically.
- **Synthraformer processing**:
  - Enabled usage during missions.
- **Recipe tuning**: Adjusted synthraformer recipes and availability.
- **Cleanup system**: Fixed erroneous collection of wound slots and augmentation records.
- **JSON serialization**: Resolved one-line nested JSON issues, reducing file size and improving readability.

### Refactors & Internal
- **Project reorganization**: Folder structure overhauled for better maintainability and team collaboration.
- **DLL cleanup**: Removed unused and redundant DLLs.
- **SynthraformerController**: Major refactor for improved stability and extensibility.

### Patched
- **Ammo & Metadata records**: Now fully supported in the synthraformer processing pipeline.
- **Drop chances**: Tweaked and balanced for consistency with intended loot distribution.

### v1.9.2 (c02b853)

### Added
  - Added percentage indicators to **Traits Recombinator** and **Indestructible Activator** tooltips for transparency on success chances.
  - Traits Synthraformer now have a **50% chance to retain generic traits** when recombining, preserving useful base effects.
  - Rarity Synthraformer didn't roll condition on item.
  - Added creature **HP and resistance values** based on mastery scaling. Also fully configurable via CSV.   
  - New parameters for **Synthraformer Drop Chance** and **Production Time** are now fully configurable via CSV.
  - Augmentation tooltips now display **stat comparisons vs original weapon** when no generic augmentation counterpart exists.
  - Introduced `ConsoleCommand` `poqcleanup` support to clean the savefile from POQ data (just like Cleanup Mode).  

### Tweaks
  - Slight rebalancing of weapon trait weights to improve distribution and reduce overpowered and bad combinations.

### v1.9.3 (29e12ef)

### Added
  - PrimalCore can downgrade item to vanilla one now

### Fixes
  - Trait recombination now keeps generic traits if check passes, or adds extra traits based on generic traits count if generics are removed so keep up with generic count.

## v1.9.4 (56137a0)

### Fixes
- **Weapon / Ammo trait handling**: Prevented duplicate trait application when a weapon/ammo already has a preserved generic trait. Mutually exclusive traits are now removed from the available pool during trait recombination, ensuring no conflicting traits are added.
- **Synthraformer drop logic**: Fixed inconsistent drop chances. Global Synthraformer drop chance reduced to **2.5%** for balance. Items with the *Indestructible* trait now have a **100% chance** to drop an *Indestructible Activator* Synthraformer, reinforcing their rarity and value.

---

> **Note**: This version introduces foundational changes for future features. Please report any unexpected behavior with save files or synthraformer behavior using logs and reproducible cases.