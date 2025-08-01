using JetBrains.Annotations;
using MGSC;
using Newtonsoft.Json;
using QM_PathOfQuasimorph.Core.Records;
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

namespace QM_PathOfQuasimorph.Core.Processors
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

            AddUnbreakableTrait();
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

        private List<string> PrepareTraits(bool removeExisting = false)
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
            var selectedTraits = SelectWeightedTraits(allTraitsCombined, totalTraitCount);

            // Apply blacklists
            selectedTraits.RemoveAll(t =>
                (itemRecord.IsMelee && meleeTraitsBlacklist.Contains(t)) ||
                (!itemRecord.IsMelee && rangedTraitsBlacklist.Contains(t)));

            if (removeExisting)
            {
                // Remove already present traits
                selectedTraits.RemoveAll(t => itemRecord.Traits.Contains(t));
            }


            // Filter all traits if they are not in allowed list (just in case)
            selectedTraits.RemoveAll(t => !allowedTraits.Contains(t));
            return selectedTraits;
        }

        private List<string> SelectWeightedTraits(Dictionary<string, int> traitWeights, int count)
        {
            var availableTraits = traitWeights
                .Where(t => t.Value > 0) // Skip traits with 0 or negative weight
                .ToDictionary(t => t.Key, t => t.Value);

            var selected = new List<string>();

            for (int i = 0; i < count && availableTraits.Count > 0; i++)
            {
                string selectedTrait = PathOfQuasimorph.raritySystem.SelectRarityWeighted<string>(availableTraits);
                selected.Add(selectedTrait);

                // Remove already selected trait to prevent duplicates
                availableTraits = availableTraits
                    .Where(t => t.Key != selectedTrait)
                    .ToDictionary(t => t.Key, t => t.Value);
            }

            return selected;
        }

        private void AddUnbreakableTrait()
        {
            var canAddUnbreakableTrait = false;

            // Only 20% of all items are eligible for unbreakable trait
            if (Helpers._random.NextDouble() <= PathOfQuasimorph.raritySystem.UNBREAKABLE_ENTRY_CHANCE &&
                PathOfQuasimorph.raritySystem.unbreakableTraitPercent.TryGetValue(itemRarity, out float weight) &&
                weight > 0)
            {
                // Get the list of eligible rarities and their weights
                var eligibleRarities = PathOfQuasimorph.raritySystem.unbreakableTraitPercent
                    .Where(kv => kv.Value > 0)
                    .ToDictionary(kv => kv.Key, kv => kv.Value);

                // Calculate total weight among eligible rarities
                float totalWeight = eligibleRarities.Values.Sum();

                // Check if this specific item wins based on its weight
                if (Helpers._random.NextDouble() * totalWeight <= weight)
                {
                    canAddUnbreakableTrait = true;
                }
            }

            _logger.Log($"\t\t  Unbreakable: {canAddUnbreakableTrait}");

            if (canAddUnbreakableTrait)
            {
                itemRecord.Unbreakable = true;
            }
        }

        internal void Reroll(AmplifierRecord ampRecord, MetadataWrapper metadata)
        {
            itemRarity = ampRecord.Rarity;

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

        internal void RerollRarities(WeaponRecord weaponRecord, AmplifierRecord ampRecord, MetadataWrapper metadata)
        {
            itemRarity = ampRecord.Rarity;

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

        internal void ReplaceWeaponTraits(RecombinatorRecord ampRecord, MetadataWrapper metadata)
        {
            ApplyTraits(true);
        }
    }
}
