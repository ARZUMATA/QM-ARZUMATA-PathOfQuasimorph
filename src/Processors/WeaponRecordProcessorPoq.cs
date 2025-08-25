using JetBrains.Annotations;
using MGSC;
using ModConfigMenu;
using Newtonsoft.Json;
using QM_PathOfQuasimorph.Controllers;
using QM_PathOfQuasimorph.Core;
using QM_PathOfQuasimorph.PoQHelpers;
using QM_PathOfQuasimorph.Records;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using static MGSC.SpawnSystem;
using Random = System.Random;

namespace QM_PathOfQuasimorph.Processors
{
    internal class WeaponRecordProcessorPoq : ItemRecordProcessor<WeaponRecord>
    {
        private new Logger _logger = new Logger(null, typeof(WeaponRecordProcessorPoq));

        public override Dictionary<string, bool> parameters => _parameters;

        private List<string> rangedTraitsBlacklist = new List<string> {
            "perfect_throw",
            "piercing_throw",
            "cleave",
            "unthrowable",
            "critical_throw",
            "backstab",
        };

        private List<string> meleeTraitsBlacklist = new List<string>(){
            "suppressor",
            "ramp_up",
            "bipod",
            "optic_sight",
            "collimator",
            "laser_sight",
            "suppressive_fire",
        };

        internal struct TraitWeights
        {
            public string name;
            public bool increase;
            public int weight;
        }

        internal Dictionary<string, int> positiveTraits = new Dictionary<string, int>()
        {
            { "perfect_throw", 500 },
            { "piercing_throw", 500 },
            { "mutiliating", 500 },
            { "cleave", 500 },
            { "offhand", 500 },
            { "extra_knockback", 500 },
            { "painful_crits", 500 },
            { "suppressor", 500 },
            { "wounding_pierce", 500 },
            { "piercing", 500 },
            { "full_piercing", 500 },
            { "ramp_up", 600 },
            { "selfcharging", 600 },
            { "critical_throw", 500 },
            { "overclock", 500 },
            { "bipod", 500 },
            { "optic_sight", 500 },
            { "collimator", 500 },
            { "laser_sight", 500 },
            { "backstab", 500 },
            { "suppressive_fire", 500 },
        };

        internal Dictionary<string, int> negativeTraits = new Dictionary<string, int>()
        {
            { "single_load", 200 },
            { "unthrowable", 500 },
            { "fragile", 350 },
            { "unwieldy", 350 },
            { "heavy_weapon", 350 },
            { "overheat", 350 },
        };

        // bool = should we increase the stat or decrease for benefits
        internal Dictionary<string, bool> _parameters = new Dictionary<string, bool>()
        {
           { "weight", false },
           { "max_durability", true },
           { "damage", true },
           { "crit_damage", true },
           { "accuracy", true },
           { "scatter_angle", false },
           { "reload_duration", false },
           { "magazine_capacity", true },
            //"special_ability",
            //"none",

            //"Damage_MinMax",
            //"Damage_CritChance",
            //"Damage_CritDmg",
            //"ReloadDuration",
            //"MagazineCapacity",
            //"BonusAccuracy",
            //"BonusScatterAngle",
        };

        internal Dictionary<string, int> rangedNatures = new Dictionary<string, int>()
        {
            { "moon_fist_1",                          100 },
            { "nature_human_fist",                    100 },
            { "nature_centaur_fist",                  100 },
            { "nature_bigpos_fist",                   100 },
            { "nature_possesed_fist",                 100 },
            { "nature_venusdemoncat_fist",            100 },
            { "nature_skinless_fist",                 100 },
            { "nature_aztknight_fist",                100 },
            { "nature_venusdemonwarrior_fist",        100 },
            { "nature_marsbaron_fist",                100 },
            { "nature_marscrab_fist",                 100 },
            { "nature_spider_fist",                   125 },
            { "nature_cyborg_fist",                   100 },
            { "nature_cyborg_drill",                  100 },
            { "nature_cyborgbattle_fist",             100 },
            { "nature_cyborgrecreation_fist",         100 },
            { "nature_cyborgrecreation_blade",        100 },
            { "nature_human_armstump",                100 },
            { "nature_moonknight_thorn",              100 },
            { "nature_moonservitor_fist",             100 },
            { "nature_moonslave_thorn",               100 },
            { "nature_scrivnus_fist",                 100 },
            { "nature_scrivnus_blade",                100 },
            { "nature_dog_bite",                      125 },
            { "nature_cargodrone_pitchfork",          100 },
        };

        internal Dictionary<string, int> meleeNatures = new Dictionary<string, int>()
        {
            { "mercury_arm",                          100 },
            { "nature_mars_headgun",                  100 },
            { "nature_cyborgbattle_machinegun",       35 },
            { "nature_mars_flamehead",                35 },
            { "venus_hand",                           100 },
            { "nature_venusrange_fist",               100 },
            { "mars_grenade_launcher",                35 },
            { "minigun_turret",                       25 },
            { "firethrower_turret",                   25 },
            { "laser_turret",                         25 },
            { "mars_urparp_headgun",                  35 },
        };

        List<HashSet<string>> traitsMutuallyExclusiveGroups = new List<HashSet<string>>
        {
            new HashSet<string> { "piercing", "full_piercing" },
        };

        public WeaponRecordProcessorPoq(ItemRecordsControllerPoq itemRecordsControllerPoq) : base(itemRecordsControllerPoq)
        {
        }

        internal override void ProcessRecord(ref string boostedParamString)
        {
            if (itemRarity == ItemRarity.Standard)
            {
                return;
            }

            ApplyTraits(false);
            ApplyParameters(ref boostedParamString);
        }

        private void ApplyParameters(ref string boostedParamString)
        {
            float baseModifier, finalModifier;
            int numToHinder, numToImprove, improvedCount, hinderedCount;
            //string boostedParamString;
            bool increase;
            PrepGenericData(out baseModifier, out finalModifier, out numToHinder, out numToImprove, out boostedParamString, out improvedCount, out hinderedCount, out increase);

            foreach (var stat in parameters)
            {
                finalModifier = GetFinalModifier(baseModifier, numToHinder, numToImprove, ref improvedCount, ref hinderedCount, boostedParamString, ref increase, stat.Key, stat.Value, _logger);
                ApplyStat(finalModifier, increase, stat);
            }
        }

        private void ApplyStat(float finalModifier, bool increase, KeyValuePair<string, bool> stat, WeaponRecord genericRecord = null)
        {
            // Simply for logging
            float outOldValue = -1;
            float outNewValue = -1;

            // If we got declared generic we take their values for reroll, and if not, use it as actual item record.
            if (genericRecord == null)
            {
                genericRecord = itemRecord;
            }

            switch (stat.Key)
            {

                case "weight":
                    //var weight = genericRecord.Weight;
                    //PathOfQuasimorph.raritySystem.ApplyModifier<float>(ref weight, finalModifier, increase, out outOldValue, out outNewValue);
                    PathOfQuasimorph.raritySystem.Apply<float>(v => itemRecord.Weight = v, () => genericRecord.Weight, finalModifier, increase, out outOldValue, out outNewValue);
                    //itemRecord.Weight = weight;
                    break;

                case "max_durability":
                    PathOfQuasimorph.raritySystem.Apply<int>(v => itemRecord.MaxDurability = v, () => genericRecord.MaxDurability, finalModifier, increase, out outOldValue, out outNewValue);
                    break;

                case "damage":
                    var dmgInfo = genericRecord.Damage;
                    PathOfQuasimorph.raritySystem.Apply<int>(v => dmgInfo.minDmg = v, () => dmgInfo.minDmg, finalModifier, increase, out outOldValue, out outNewValue);
                    PathOfQuasimorph.raritySystem.Apply<int>(v => dmgInfo.maxDmg = v, () => dmgInfo.maxDmg, finalModifier, increase, out outOldValue, out outNewValue);
                    itemRecord.Damage = dmgInfo;
                    break;

                case "crit_damage":
                    dmgInfo = genericRecord.Damage;
                    PathOfQuasimorph.raritySystem.Apply<float>(v => dmgInfo.critDmg = v, () => dmgInfo.critDmg, finalModifier, increase, out outOldValue, out outNewValue);
                    itemRecord.Damage = dmgInfo;
                    break;

                case "accuracy":
                    PathOfQuasimorph.raritySystem.Apply<float>(v => itemRecord.BonusAccuracy = v, () => genericRecord.BonusAccuracy, finalModifier, increase, out outOldValue, out outNewValue);

                    break;
                case "scatter_angle":
                    PathOfQuasimorph.raritySystem.Apply<float>(v => itemRecord.BonusScatterAngle = v, () => genericRecord.BonusScatterAngle, finalModifier, increase, out outOldValue, out outNewValue);

                    break;

                case "reload_duration":
                    PathOfQuasimorph.raritySystem.Apply<int>(v => itemRecord.ReloadDuration = v, () => genericRecord.ReloadDuration, finalModifier, increase, out outOldValue, out outNewValue);

                    // If we get that trait
                    if (itemRecord.Traits.Contains("single_load"))
                    {
                        itemRecord.ReloadDuration = 1;
                    }
                    break;

                case "magazine_capacity":
                    PathOfQuasimorph.raritySystem.Apply<int>(v => itemRecord.MagazineCapacity = v, () => genericRecord.MagazineCapacity, finalModifier, increase, out outOldValue, out outNewValue);
                    break;

                case "special_ability":
                    break;
                case "none":
                    break;
            }

            Plugin.Logger.Log($"\t\t old value {outOldValue}");
            Plugin.Logger.Log($"\t\t new value {outNewValue}");
        }

        internal void ApplyTraits(bool clearTraits, float removeChance = 0.2f, bool keepGeneric = false)
        {
            if (itemRarity == ItemRarity.Standard)
            {
                return;
            }

            Plugin.Logger.Log($"ApplyTraits: clearTraits args: {clearTraits}, removeChance: {removeChance}, keepGeneric: {keepGeneric}");

            var weaponRecord = Data.Items.GetSimpleRecord<WeaponRecord>(oldId, true);

            Plugin.Logger.Log($"weaponRecord null: {weaponRecord == null}");
            Plugin.Logger.Log($"itemRecord null: {itemRecord.Id}");

            var extraTraitCount = 0;

            // If we keep generic, recheck chance.
            if (keepGeneric && weaponRecord != null)
            {
                keepGeneric = Helpers._random.NextDouble() < removeChance;
            }
            else
            {
                keepGeneric = false;
            }

            Plugin.Logger.Log($"Keeping generic? {keepGeneric}");

            // Existing traits
            Plugin.Logger.Log($"\tExisting traits: {itemRecord.Traits.Count}");

            foreach (var trait in itemRecord.Traits)
            {
                Plugin.Logger.Log($"\t\t {trait}");
            }

            // Generic traits
            if (weaponRecord != null)
            {
                Plugin.Logger.Log($"\tGeneric traits: {weaponRecord.Traits.Count}");

                foreach (var trait in weaponRecord.Traits)
                {
                    Plugin.Logger.Log($"\t\t {trait}");
                }

                extraTraitCount = keepGeneric ? 0 : weaponRecord.Traits.Count;
                Plugin.Logger.Log($"\textraTraitCount: {extraTraitCount}");
            }

            // Apply traits to record
            // Should we remove existing traits?
            if (clearTraits)
            {
                itemRecord.Traits.Clear();

                if (keepGeneric && weaponRecord != null)
                {
                    Plugin.Logger.Log($"Keeping generic? Yes.");
                    itemRecord.Traits.AddRange(weaponRecord.Traits);
                }
                else
                {
                    Plugin.Logger.Log($"Keeping generic? No.");
                }
            }
            else
            {
                // Randomly decide whether to remove existing traits (20% chance)
                if (Helpers._random.NextDouble() < removeChance)
                {
                    Plugin.Logger.Log($"Keeping existing? Yes.");
                    itemRecord.Traits.Clear();
                }
                else
                {
                    Plugin.Logger.Log($"Keeping existing? No.");

                }
            }

            // Select traits
            List<string> selectedTraits = PrepareTraits(extraTraitCount);

            Plugin.Logger.Log($"\tSelectedTraits traits: {selectedTraits.Count}");

            foreach (var trait in selectedTraits)
            {
                Plugin.Logger.Log($"\t\t {trait}");
            }


            // Add traits
            for (int i = 0; i < selectedTraits.Count; i++)
            {
                itemRecord.Traits.Add(selectedTraits[i]);
            }

            Plugin.Logger.Log($"\tNew traits: {itemRecord.Traits.Count}");

            foreach (var trait in itemRecord.Traits)
            {
                Plugin.Logger.Log($"\t\t {trait}");
            }
        }

        private List<string> PrepareTraits(int extraTraitCount)
        {
            // Determine if the item is a melee weapon
            _logger.Log($"\t\t  isMelee: {itemRecord.IsMelee}");

            // Allowed traits for item type
            var allowedTraits = itemRecordsControllerPoq.GetAddeableTraits(ItemTraitType.WeaponTrait);

            // Combined dict of positive and negative traits
            Dictionary<string, int> allTraitsCombined = positiveTraits.Concat(negativeTraits).ToDictionary(pair => pair.Key, pair => pair.Value);

            // Log traits in allowedTraits not present in allTraitsCombined
            foreach (var trait in allowedTraits)
            {
                if (!allTraitsCombined.ContainsKey(trait))
                {
                    _logger.LogWarning($"[WARNING] Allowed trait '{trait}' is not present in allTraitsCombined.");
                }
            }

            // Determine total number of traits to add based on rarity
            //var totalTraitCount = PathOfQuasimorph.raritySystem.GetTraitCountByRarity(itemRarity, allTraitsCombined.Count + extraTraitCount);
            var totalTraitCount = (int)itemRarity + extraTraitCount; // Avoid percentages

            // Select traits based on weights
            var selectedTraits = SelectWeightedTraits(allTraitsCombined, totalTraitCount, itemRecord.Traits, traitsMutuallyExclusiveGroups);

            // Apply blacklists
            selectedTraits.RemoveAll(t =>
                (itemRecord.IsMelee && meleeTraitsBlacklist.Contains(t)) ||
                (!itemRecord.IsMelee && rangedTraitsBlacklist.Contains(t)));

            // // Remove already present traits
            // selectedTraits.RemoveAll(t => itemRecord.Traits.Contains(t));

            // Filter all traits if they are not in allowed list (just in case)
            selectedTraits.RemoveAll(t => !allowedTraits.Contains(t));
            return selectedTraits;
        }

        internal void RerollRandomStat(SynthraformerRecord ampRecord, MetadataWrapper metadata, bool blockHinder)
        {
            var genericRecord = Data.Items.GetSimpleRecord<WeaponRecord>(metadata.Id, true);

            float baseModifier, finalModifier;
            int numToHinder, numToImprove, improvedCount, hinderedCount;
            string boostedParamString;
            bool increase;
            PrepGenericData(out baseModifier, out finalModifier, out numToHinder, out numToImprove, out boostedParamString, out improvedCount, out hinderedCount, out increase);

            Plugin.Logger.Log($"RerollRandomStat");
            Plugin.Logger.Log($"metadata: {metadata.BoostedString}");

            if (metadata.BoostedString.Length > 1)
            {
                boostedParamString = metadata.BoostedString;
            }

            if (blockHinder)
            {
                hinderedCount = 999; // Test
            }

            var statIdx = Helpers._random.Next(0, parameters.Count);
            var stat = parameters.ElementAt(statIdx);

            finalModifier = GetFinalModifier(baseModifier, numToHinder, numToImprove, ref improvedCount, ref hinderedCount, boostedParamString, ref increase, stat.Key, stat.Value, _logger);
            ApplyStat(finalModifier, increase, stat, genericRecord);
        }

        internal void ReplaceWeaponTraits(SynthraformerRecord recomb, MetadataWrapper metadata, float removeChance, bool keepGeneric)
        {
            ApplyTraits(true, removeChance, keepGeneric);

            //weaponComponent.Traits.Clear();

            //foreach (var trait in itemRecord.Traits)
            //{
            //    weaponComponent.Traits.Add(ItemTraitSystem.CreateItemTrait(trait));
            //}

            _logger.Log($"ReplaceWeaponTraits: Success!");
        }

        internal void ReplaceRequiredAmmo(SynthraformerRecord recomb, MetadataWrapper meta)
        {
            Dictionary<string, string> requiredAmmo = new Dictionary<string, string>
             {
                { "BatteryCells" , "battery_basic_ammo" },
                { "Bolts" , "nail_bolts_ammo" },
                { "Bullets" , "small_basic_ammo" },
                { "Gas" , "gas_ammo" },
                { "Heavy" , "rifle_basic_ammo" },
                { "Medium" , "medium_basic_ammo" },
                { "QuasiCells" , "quasi_basic_ammo" },
                { "Rocket" , "rocket_basic_ammo" },
                { "SawBlade" , "sawblade_ammo" },
                { "Shells" , "shotgun_bullet_ammo" },
                { "SuperHeavy" , "heavy_basic_ammo" },
                { "Toxic" , "toxic_ammo" },
                { string.Empty , string.Empty },
             };

            var rndIndx = Helpers._random.Next(0, requiredAmmo.Keys.Count);

            itemRecord.RequiredAmmo = requiredAmmo.ElementAt(rndIndx).Key;
            itemRecord.DefaultAmmoId = requiredAmmo.ElementAt(rndIndx).Value;
        }

        internal void CreateAugmentation(SynthraformerRecord record, MetadataWrapper metadata, CompositeItemRecord obj)
        {
            _logger.Log($"\tCreateAugmentation");
            string boostedParamString = string.Empty;

            // Ah here we go again. We got just weapon record and we need to create an augmentation from it retaining all parameters from augmentation.
            // Weapon record left as is and used as an aug weapon so it's ok here.
            // We need to add woundslot and augmentation record now that will be rolled on the fly.

            // We may "borrow" augrecord right?
            //CompositeItemRecord obj = Data.Items.GetRecord(itemIdOrigin) as CompositeItemRecord;

            _logger.Log($"\taugmentationRecord Id: {itemId}");

            var augmentationRecord = new AugmentationRecord();
            augmentationRecord.Id = itemId;
            augmentationRecord.Categories = new List<string>();
            augmentationRecord.Categories.Add("CyberAug");
            augmentationRecord.TechLevel = itemRecord.TechLevel;
            augmentationRecord.Price = itemRecord.Price;
            augmentationRecord.Weight = itemRecord.Weight;
            augmentationRecord.InventoryWidthSize = itemRecord.InventoryWidthSize;
            augmentationRecord.ItemClass = itemRecord.ItemClass;
            augmentationRecord.AugmentationClass = AugmentationClass.Combat;
            augmentationRecord.WoundSlotIds = new List<string>();
            augmentationRecord.TooltipIconTag = "aug_type_arm";

            //augmentationRecord.ContentDescriptor = itemRecord.ContentDescriptor;

            var legitSlotsArms = Data.WoundSlots._records
                .Where(kv => kv.Value.SlotType == "Arm")
                .ToDictionary(kv => kv.Key, kv => kv.Value);

            // Now we create new woundSlot record based on these lists
            var armSlotRecord = Data.WoundSlots.GetRecord(legitSlotsArms.GetRandomItem().Key);
            var armSlot_NewId = $"{armSlotRecord.Id}_{itemId}";

            _logger.Log($"\tarmSlot_NewId: {armSlot_NewId}");

            WoundSlotRecord armSlotRecordNew = ItemRecordHelpers.CloneWoundSlotRecord(armSlotRecord, $"{armSlot_NewId}");
            itemRecordsControllerPoq.woundSlotRecordProcessorPoq.Init(armSlotRecordNew, itemRarity, mobRarityBoost, false, $"{armSlot_NewId}", oldId);
            itemRecordsControllerPoq.woundSlotRecordProcessorPoq.ProcessRecord(ref boostedParamString);

            Data.WoundSlots.AddRecord($"{armSlot_NewId}", armSlotRecordNew);
            RecordCollection.WoundSlotRecords.Add($"{armSlot_NewId}", armSlotRecordNew);
            Localization.DuplicateKey("woundslot." + armSlotRecord.Id + ".name", "woundslot." + armSlot_NewId + ".name");

            // Penalty effects
            armSlotRecordNew.ImplicitPenaltyEffects["arm_slot_unavailable"] = 1;
            armSlotRecordNew.BareHandWeapon = itemId;

            augmentationRecord.WoundSlotIds.Add(armSlot_NewId);

            var legitSlotsShoulders = Data.WoundSlots._records
                .Where(kv => kv.Value.SlotType == "Shoulder")
                .ToDictionary(kv => kv.Key, kv => kv.Value);

            var shoulderSlotRecord = Data.WoundSlots.GetRecord(legitSlotsShoulders.GetRandomItem().Key);
            var shoulderSlot_NewId = $"{shoulderSlotRecord.Id}_{itemId}";

            _logger.Log($"\tashoulderSlot_NewId: {shoulderSlot_NewId}");

            WoundSlotRecord shoulderSlotRecordNew = ItemRecordHelpers.CloneWoundSlotRecord(shoulderSlotRecord, $"{shoulderSlot_NewId}");
            itemRecordsControllerPoq.woundSlotRecordProcessorPoq.Init(shoulderSlotRecordNew, itemRarity, mobRarityBoost, false, $"{shoulderSlot_NewId}", oldId);
            itemRecordsControllerPoq.woundSlotRecordProcessorPoq.ProcessRecord(ref boostedParamString);

            Data.WoundSlots.AddRecord($"{shoulderSlot_NewId}", shoulderSlotRecordNew);
            RecordCollection.WoundSlotRecords.Add($"{shoulderSlot_NewId}", shoulderSlotRecordNew);
            Localization.DuplicateKey("woundslot." + shoulderSlotRecord.Id + ".name", "woundslot." + shoulderSlot_NewId + ".name");

            augmentationRecord.WoundSlotIds.Add(shoulderSlot_NewId);

            obj.Records.Add(augmentationRecord);

            // Finalize
            Data.Items._records[itemId] = obj;

            //Data.Items.RemoveRecord(itemId);
            //Data.Items.AddRecord(itemId, obj);

            RecordCollection.ItemRecords[itemId] = obj;
            //RecordCollection.MetadataWrapperRecords.Add(itemId, wrapper);
        }

        internal void RemoveImplicit(SynthraformerRecord record, MetadataWrapper metadata)
        {
            itemRecord.IsImplicit = false;
        }

        internal BasePickupItem TransmuteWeapon(SynthraformerRecord record, MetadataWrapper metadata)
        {
            if (new[] { "human_hand", "human_leg", } // "human_wrist", "human_feet",
                         .Any(oldId.Equals))
            {
                // First, determine if the item breaks
                var breakItem = Helpers._random.NextDouble() < 0.3f;

                if (breakItem)
                {
                    return null; // Item breaks, return nothing
                }

                // Only proceed to weapon type selection if item didn't break
                var melee = Helpers._random.NextDouble() < 0.5f;
                itemRecord.IsImplicit = false;
                string selectedNature = string.Empty;

                if (melee)
                {
                    selectedNature = PathOfQuasimorph.raritySystem.SelectRarityWeighted<string>(meleeNatures);
                }
                else
                {
                    selectedNature = PathOfQuasimorph.raritySystem.SelectRarityWeighted<string>(rangedNatures);
                }

                return ItemFactoryPoq.CreateNewItem(selectedNature);
            }

            return null;
        }
    }
}
