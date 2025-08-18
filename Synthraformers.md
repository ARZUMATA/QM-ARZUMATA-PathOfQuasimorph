# Synthraformer System — Comprehensive Guide

The **Synthraformer System** is a item-modification framework introduced in version `1.9` of *Path of Quasimorph*, enabling players to transform, enhance, and mutate equipment using specialized crafting components. These items function as advanced catalysts within the Magnum Crafting system, allowing for targeted alterations to item rarity, traits, stats, durability, and even item class (e.g., weapon → augment).

Each Synthraformer corresponds to a specific effect and follows a tiered crafting progression rooted in a base material: **Primal Core**. The system supports full localization, tooltips, and safe metadata preservation during transformations.


---

## 🔧 Synthraformer Types & Functions

Synthraformers are defined by the `Synthraformer Type` and each has a unique role. They are color-coded and follow a tech-tree-style crafting dependency.

| Type | Color | Function | Craftable | Status |
|------|------|--------|----------|--------|
| `Rarity` | Red | Rolls a new random rarity using blackjack logic | ✅ | Active |
| `Infuser` | Orange | (No current functionality) | ✅ | ❌ Unused |
| `Traits` | Yellow | Replaces all traits on weapons/ammo | ✅ | Active |
| `Indestructible` | Green | Attempts to apply *Indestructible* trait (50%) | ✅ | Active |
| `Amplifier` | Blue | Rerolls a random stat on non-standard items | ✅ | Active |
| `Transmuter` | Indigo | (No current functionality) | ✅ | ❌ Unused |
| `Catalyst` | Violet | Converts weapons into augments or enhances augments/implants | ✅ | Active |
| `Azure` | Azure | (No current functionality)| ✅ | ❌ Unused |
| `PrimalCore` | White | Base drop from disassembled items | ❌ Drop-only | Active |

> ⚠️ **Note**: Unused = items are present in the mod code, but not craftle neither obtainable.

---

## 🧪 Core Mechanics

### 1. **Primal Core — Base Material**
- **Source**: Dropped during item disassembly (percentage chance).
- **Role**: Acts as the universal crafting currency.
- **Behavior**: Cannot be crafted; only obtained via decomposition.

### 2. **Amplifier — Stat Reroll**
- **Effect**: Randomizes one modifiable stat (e.g., damage, accuracy, reload speed).
- **Applies To**:
  - Weapons, armor, ammo, so gear with modifiable stats.
- **Rules**:
  - Only works on non-Standard items.
- **Use Case**: Optimizing high-tier gear with suboptimal rolls.

### 3. **Traits Recombinator**
- **Effect**: Removes all current traits and rerolls a new set.
- **Applies To**:
  - Weapons
  - Ammo
- **Strategy**: Eliminate negative traits or unlock rare synergies.

### 4. **Random Rarity — Blackjack-Based Roll**
- **Mechanic**: Uses a card-based "Blackjack 21" system:

- **Outcome**:
  - Item is replaced with new rarity variant.
  - Metadata and UID are preserved and updated.

### 5. **Indestructible Activator**
- **Effect**: Adds *Indestructible* trait with **50% chance**.
- **Applies To**: Any item that can be broken and has durability (armor, weapons).
- **Restriction**: Only works on non-Standard items.
- **Result**: No degradation from use.

### 6. **Augmentation Catalyst**
- **Dual Function**:
  1. **Weapon → Augment**: Transforms a weapon into an integrated augment with random augment stats.
  2. **Augment/Implant Enhancement**: Adds a random positive or negative effect as well as removing if chance is rolled.
- **Consumption**: Always used up, regardless of effect application.
- **Endgame Use**: Enables hybrid builds combining firepower and cybernetics.


---

## 🔗 Crafting Tree & Progression

```text
PrimalCore (drop)
│
└─▶ Amplifier (7 Core) 
      │
      ├─▶ Traits (3 Amp + 4 Core = 7)
           │
           ├─▶ Rarity (3 Traits + 4 Amp = 7)
           │    │
           │    └─▶ Catalyst (2 Rarity + 1 Transmuter + 4 Core = 7)
           │
           └─▶ Indestructible (2 Traits + 2 Amp + 3 Core = 7)
       
```

### 🕒 Crafting Times
| Type | Time (hrs) |
|------|-----------|
| Amplifier | 0.5 |
| Traits | 1.0 |
| Rarity | 1.5 |
| Indestructible | 2.0 |
| Infuser / Transmuter | 2.5 |
| Catalyst | 3.0 |
| Azure | 4.0 |

### ✅ Recipe Management
- Recipes appear under *Synthraformers* category in crafting UI.

---

### ⚠️ Under Testing
- Logs and save files welcome.

---

## ✅ Summary

The Synthraformer System turns crafting into a dynamic, risk-aware process where players can:
- Upgrade gear through stat and trait manipulation.
- Chase legendary rarities via high-stakes rolls.
- Convert weapons into cybernetic augments.
- Build highly customized loadouts.

> 💡 **Tip**: Always save before using `Rarity` or `Catalyst` — the outcome might be legendary… or tragic.