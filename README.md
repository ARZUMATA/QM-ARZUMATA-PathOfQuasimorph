# QM_PathOfQuasimorph

<p align="center">
  <a href="README.md">English</a> ·
  <a href="README_RU.md">Русский</a>
</p>

<p align="center">
  <a href="GALLERY.md">Screenshots</a>
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

<div align="center">
  <img src="media/thumbnail.png" alt="thumbnail icon">
</div>

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

* **Synthraformer System — Item Alchemy & Crafting Evolution**  
  Transform your gear through powerful Synthraformer catalysts, each enabling unique modifications:
  - **Rarity**: Roll a new random rarity using a blackjack-style risk system.
  - **Traits Recombinator**: Strip and replace all traits on weapons and ammo.
  - **Indestructible Activator**: Attempt to make an item unbreakable (% chance).
  - **Amplifier**: Reroll a random stat on non-Standard items for optimization.
  - **Augmentation Catalyst**: Turn weapons into integrated augments or enhance existing ones.
  - **Primal Core**: The base material, dropped when disassembling items (% chance).

  Higher-tier Synthraformers are crafted from lower-tier ones, forming a progression tree rooted in Primal Cores. Unused types (`Infuser`, `Transmuter`, `Azure`) are reserved for future updates.
  
<p align="center">
  <a href="Synthraformers.md">Synthraformer System — Comprehensive Guide</a>
</p>


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

(Right now cleaning not doing its job right. This will be improved)

# External files

| File             | Description                                 | Documentation |
|------------------|---------------------------------------------|---------------|
| Rarities.csv     | Contains rarity-related parameters for items. | [EN version](docs/Rarities_CSV_EN.md) / [RU version](docs/Rarities_CSV_RU.md) |

# Source Code
Source code is available on [GitHub](https://github.com/ARZUMATA/QM-ARZUMATA-PathOfQuasimorph)

Thanks to NBK_RedSpy, Crynano and all the people who make their code open source.

## [Changelog](CHANGELOG.md)