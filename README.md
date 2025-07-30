# QM_PathOfQuasimorph

<p align="center">
  <a href="readme.md">English</a> ·
  <a href="readme_ru.md">Русский</a>
</p>

Ever wondered why your items so bad?

Do you break down more gear than you use?

Is every loot drop a disappointment?

Do you yearn for the thrill of the perfect find, the rush of discovering a truly legendary weapon?

Or maybe yoe you an ARPG gamer who miss vibe for magic and rare items?

Well this is the mod for you!

**Then descend into the Path of Quasimorph, where every item holds the potential for greatness!**

### **Forge Your Legend with Path of Quasimorph!**

## Path of Quasimorph

![thumbnail icon](media/thumbnail.jpg)

## Image Gallery

<p>
    <a href="media/160618.jpg" data-sub-html="Description">
      <img alt="Description" src="media/160618.jpg" width="300" />
    </a>
</p>

<details>
  <summary style="color: #448aff; font-weight: bold;">
    <h3 style="margin: 0; font-size: 24px;"> Click for more</h3>
  </summary>

<p>
  <a href="media/160601.jpg" data-sub-html="160601">
    <img alt="160601" src="media/160601.jpg" width="300" />
  </a>
</p>

<p>
  <a href="media/160618.jpg" data-sub-html="160618">
    <img alt="160618" src="media/160618.jpg" width="300" />
  </a>
</p>

<p>
  <a href="media/162125.jpg" data-sub-html="162125">
    <img alt="162125" src="media/162125.jpg" width="300" />
  </a>
</p>

<p>
  <a href="media/162132.jpg" data-sub-html="162132">
    <img alt="162132" src="media/162132.jpg" width="300" />
  </a>
</p>

<p>
  <a href="media/231200.jpg" data-sub-html="231200">
    <img alt="231200" src="media/231200.jpg" width="300" />
  </a>
</p>

<p>
  <a href="media/231203.jpg" data-sub-html="231203">
    <img alt="231203" src="media/231203.jpg" width="300" />
  </a>
</p>

<p>
  <a href="media/231206.jpg" data-sub-html="231206">
    <img alt="231206" src="media/231206.jpg" width="300" />
  </a>
</p>

<p>
  <a href="media/231211.jpg" data-sub-html="231211">
    <img alt="231211" src="media/231211.jpg" width="300" />
  </a>
</p>

<p>
  <a href="media/231220.jpg" data-sub-html="231220">
    <img alt="231220" src="media/231220.jpg" width="300" />
  </a>
</p>

</details>

## Features

* A New Loot Paradigm: Say goodbye to mundane loot! Path of Quasimorph introduces a dynamic rarity system, categorizing items into distinct tiers, each brimming with potential and its own amount of boosted properties and extra traits:

       - Standard: The baseline, nothing can surprise you.
       - Enhanced: A noticeable step up, with improved stats.
       - Advanced: Starting to show real power, with enhanced attributes.
       - Premium: Coveted finds, boasting significant stat boosts and the potential for more extra traits.
       - Prototype: Rare and powerful, these represent cutting-edge technology and devastating potential.
       - Quantum: The pinnacle of rarity! Discovering a Quantum item is a cause for celebration.
         Imbued - with incredible stats, unique traits, and even the possibility of unbreakable durability!

* Weighted Drops, Epic Finds: Just like the best ARPGs, rarity matters! Quantum items are incredibly rare, making each find a truly momentous occasion. You are lucky for sure!

* Visually Distinct Gear: Instantly identify your new possessions! Each rarity tier is distinguished by a unique background color, allowing you to quickly assess the value of every drop.

* Prefixes and Affixes: Each item comes with with custom prefixes and affixes, adding flavor to your most treasured gear and game. 

(This will be update with more prefixes based on boosted stats)

- **Customizable Configuration**: Allows users to adjust settings through a [Mod Configuration Menu (MCM).](https://steamcommunity.com/sharedfiles/filedetails/?id=3469678797)

## Requirements (Optional)

- **MCM (Mod Configuration Menu)**: A configuration menu framework to manage settings via an in-game interface.

As alternative you can find config files in:
- `%AppData%\..\LocalLow\Magnum Scriptum Ltd\Quasimorph_ModConfigs\QM_PathOfQuasimorph\config_mcm.ini`

# Configuration
| Name                  | Default | Description                                                                 |
|-----------------------|---------|-----------------------------------------------------------------------------|
| Enable                |true     | Enable or disable  mod. Stops generation new items. Keeping existing ones.  |
| CleanupMode           |false    | It will try to cleanup your save game in case you decide to unsubscribe.    |

CleanupMode works this way:
Once enabled, load game, be in "space" mode, it will try to cleanup all items made by mod making them "standard", save the game. Then you can keep playing without mod enabled and clean item pool.
(This will be improved)

# External files

| File             | Description                                 | Documentation |
|------------------|---------------------------------------------|---------------|
| Rarities.csv     | Contains rarity-related parameters for items. | [EN version](docs/Rarities_CSV_EN.md) / [RU version](docs/Rarities_CSV_RU.md) |

# Source Code
Source code is available on [GitHub](https://github.com/ARZUMATA/QM-ARZUMATA-PathOfQuasimorph)

Thanks to NBK_RedSpy, Crynano and all the people who make their code open source.

# Change Log
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

