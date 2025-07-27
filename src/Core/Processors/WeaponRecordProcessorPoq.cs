using MGSC;
using Newtonsoft.Json;
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

        public override List<string> parameters => _parameters;

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

        internal List<string> _parameters = new List<string>()
        {
            "weight",
            "max_durability",
            "damage",
            "crit_damage",
            "accuracy",
            "scatter_angle",
            "reload_duration",
            "magazine_capacity",
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
                finalModifier = GetFinalModifier(baseModifier, numToHinder, numToImprove, ref improvedCount, ref hinderedCount, boostedParamString, ref increase, stat, _logger);

                // Simply for logging
                float outOldValue = -1;
                float outNewValue = -1;

                switch (stat)
                {

                    case "weight":
                        //var weight = itemRecord.Weight;
                        //PathOfQuasimorph.raritySystem.ApplyModifier<float>(ref weight, finalModifier, increase, out outOldValue, out outNewValue);
                        //itemRecord.Weight = weight;

                        PathOfQuasimorph.raritySystem.Apply<float>(v => itemRecord.Weight = v, () => itemRecord.Weight, finalModifier, increase, out outOldValue, out outNewValue);
                        break;

                    case "max_durability":
                        PathOfQuasimorph.raritySystem.Apply<int>(v => itemRecord.MaxDurability = v, () => itemRecord.MaxDurability, finalModifier, increase, out outOldValue, out outNewValue);
                        break;

                    case "damage":
                        var dmgInfo = itemRecord.Damage;
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
                        PathOfQuasimorph.raritySystem.Apply<float>(v => itemRecord.BonusAccuracy = v, () => itemRecord.BonusAccuracy, finalModifier, increase, out outOldValue, out outNewValue);

                        break;
                    case "scatter_angle":
                        PathOfQuasimorph.raritySystem.Apply<float>(v => itemRecord.BonusScatterAngle = v, () => itemRecord.BonusScatterAngle, finalModifier, increase, out outOldValue, out outNewValue);

                        break;

                    case "reload_duration":
                        PathOfQuasimorph.raritySystem.Apply<int>(v => itemRecord.ReloadDuration = v, () => itemRecord.ReloadDuration, finalModifier, increase, out outOldValue, out outNewValue);

                        // If we get that trait
                        if (itemRecord.Traits.Contains("single_load"))
                        {
                            itemRecord.ReloadDuration = 1;
                        }

                        break;

                    case "magazine_capacity":
                        PathOfQuasimorph.raritySystem.Apply<int>(v => itemRecord.MagazineCapacity = v, () => itemRecord.MagazineCapacity, finalModifier, increase, out outOldValue, out outNewValue);
                        break;

                    case "special_ability":
                        break;
                    case "none":
                        break;
                }

                Plugin.Logger.Log($"\t\t old value {outOldValue}");
                Plugin.Logger.Log($"\t\t new value {outNewValue}");
            }
        }

        internal void ApplyTraits()
        {
            // Apply Traits
            var traitsForItemType = itemRecordsControllerPoq.GetAddeableTraits(ItemTraitType.WeaponTrait);
            Helpers.ShuffleList(traitsForItemType);

            // Determine if the item is a melee weapon
            bool isMelee = itemRecord.IsMelee;

            _logger.Log($"\t\t  isMelee: {isMelee}");

            // Apply traits blacklist
            for (int i = traitsForItemType.Count - 1; i >= 0; i--)
            {
                var key = traitsForItemType.ElementAt(i);
                if (isMelee && meleeTraitsBlacklist.Contains(key))
                {
                    _logger.Log($"[RaritySystem] Removing key '{key}' from traitsForItemTypeShuffled as it's in the meleeTraitsBlacklist.");
                    traitsForItemType.Remove(key);
                }

                if (!isMelee && rangedTraitsBlacklist.Contains(key))
                {
                    _logger.Log($"[RaritySystem] Removing key '{key}' from traitsForItemTypeShuffled as it's in the rangedTraitsBlacklist.");
                    traitsForItemType.Remove(key);
                }
            }

            var traitCount = PathOfQuasimorph.raritySystem.GetTraitCountByRarity(itemRarity, traitsForItemType.Count);

            // Calculate the number of traits to adjust based on the percentage
            int numParamsToAdjust = Mathf.Max(1, traitCount); // Ensure at least 1 parameter is adjusted

            var canAddUnbreakableTrait = false;

            // Only 20% of all items are eligible for unbreakable trait
            if (new Random().NextDouble() <= PathOfQuasimorph.raritySystem.UNBREAKABLE_ENTRY_CHANCE &&
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
                if (new Random().NextDouble() * totalWeight <= weight)
                {
                    canAddUnbreakableTrait = true;
                }
            }

            _logger.Log($"\t\t  Unbreakable: {canAddUnbreakableTrait}");

            if (canAddUnbreakableTrait)
            {
                itemRecord.Unbreakable = true;
            }

            // Add traits in the end

            // Note, here we can only apply existing traits in game, we can't change any parameters here.
            // Parameters can we changes either by adding new trait record (bad), or modifying resulting pickupitem traits parameters (good).

            // In case the dictionary is smaller now.
            if (numParamsToAdjust > traitsForItemType.Count)
            {
                numParamsToAdjust = traitsForItemType.Count;
            }

            for (int i = 0; i < numParamsToAdjust; i++)
            {
                // If trait already there, don't touch it.
                if (itemRecord.Traits.Contains(traitsForItemType[i]))
                {
                    continue;
                }

                itemRecord.Traits.Add(traitsForItemType[i]);
            }
        }
    }
}
