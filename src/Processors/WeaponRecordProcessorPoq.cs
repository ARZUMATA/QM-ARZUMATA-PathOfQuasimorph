using JetBrains.Annotations;
using MGSC;
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
            { "perfect_throw", 5 },
            { "piercing_throw", 5 },
            { "mutiliating", 5 },
            { "cleave", 5 },
            { "offhand", 5 },
            { "extra_knockback", 5 },
            { "painful_crits", 5 },
            { "suppressor", 5 },
            { "wounding_pierce", 5 },
            { "piercing", 5 },
            { "full_piercing", 5 },
            { "ramp_up", 5 },
            { "selfcharging", 5 },
            { "critical_throw", 5 },
            { "overclock", 5 },
            { "bipod", 5 },
            { "optic_sight", 5 },
            { "collimator", 5 },
            { "laser_sight", 5 },
            { "backstab", 5 },
            { "suppressive_fire", 5 },
        };

        internal Dictionary<string, int> negativeTraits = new Dictionary<string, int>()
        {
            { "single_load", 5 },
            { "unthrowable", 5 },
            { "fragile", 5 },
            { "unwieldy", 5 },
            { "heavy_weapon", 5 },
            { "overheat", 5 },
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

            ApplyTraits();
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
                    dmgInfo = itemRecord.Damage;
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

        internal void ApplyTraits(bool replaceTraits = false)
        {
            if (itemRarity == ItemRarity.Standard)
            {
                return;
            }

            List<string> selectedTraits = PrepareTraits();

            // Apply traits to record
            // Should we remove existing traits?
            if (replaceTraits)
            {
                itemRecord.Traits.Clear();
            }
            else
            {
                // Randomly decide whether to remove existing traits (20% chance)
                if (Helpers._random.NextDouble() < 0.2)
                {
                    itemRecord.Traits.Clear();
                }
            }

            // Add traits
            for (int i = 0; i < selectedTraits.Count; i++)
            {
                itemRecord.Traits.Add(selectedTraits[i]);
            }
        }

        private List<string> PrepareTraits()
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
            var totalTraitCount = PathOfQuasimorph.raritySystem.GetTraitCountByRarity(itemRarity, allTraitsCombined.Count);

            // Select traits based on weights
            var selectedTraits = SelectWeightedTraits(allTraitsCombined, totalTraitCount, traitsMutuallyExclusiveGroups);

            // Apply blacklists
            selectedTraits.RemoveAll(t =>
                (itemRecord.IsMelee && meleeTraitsBlacklist.Contains(t)) ||
                (!itemRecord.IsMelee && rangedTraitsBlacklist.Contains(t)));

            // Remove already present traits
            selectedTraits.RemoveAll(t => itemRecord.Traits.Contains(t));

            // Filter all traits if they are not in allowed list (just in case)
            selectedTraits.RemoveAll(t => !allowedTraits.Contains(t));
            return selectedTraits;
        }

        internal void RerollRandomStat(SynthraformerRecord ampRecord, MetadataWrapper metadata)
        {
            var genericRecord = Data.Items.GetSimpleRecord<WeaponRecord>(metadata.Id, true);

            float baseModifier, finalModifier;
            int numToHinder, numToImprove, improvedCount, hinderedCount;
            string boostedParamString;
            bool increase;
            PrepGenericData(out baseModifier, out finalModifier, out numToHinder, out numToImprove, out boostedParamString, out improvedCount, out hinderedCount, out increase);

            var statIdx = Helpers._random.Next(0, parameters.Count);
            var stat = parameters.ElementAt(statIdx);

            finalModifier = GetFinalModifier(baseModifier, numToHinder, numToImprove, ref improvedCount, ref hinderedCount, boostedParamString, ref increase, stat.Key, stat.Value, _logger);
            ApplyStat(finalModifier, increase, stat, genericRecord);
        }

        internal void ReplaceWeaponTraits(SynthraformerRecord recomb, MetadataWrapper metadata)
        {
            ApplyTraits(true);

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

            _logger.Log($"\augmentationRecord Id: {itemId}");

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

            _logger.Log($"\armSlot_NewId: {armSlot_NewId}");

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

            _logger.Log($"\ashoulderSlot_NewId: {shoulderSlot_NewId}");

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
    }
}
