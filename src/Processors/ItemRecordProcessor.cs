using MGSC;
using QM_PathOfQuasimorph.Controllers;
using QM_PathOfQuasimorph.Core;
using QM_PathOfQuasimorph.Records;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static MGSC.TurnDebugLogger;

namespace QM_PathOfQuasimorph.Processors
{
    internal abstract class ItemRecordProcessor<T> : BasePickupItemRecordProcessor<T> where T : ItemRecord
    {
        //private new Logger _logger = new Logger(null, typeof(ItemRecordProcessor<T>));
        public override Dictionary<string, bool> parameters => _parameters;

        protected ItemRecordProcessor(ItemRecordsControllerPoq itemRecordsControllerPoq) : base(itemRecordsControllerPoq)
        {
            _parameters["Weight"] = true;
        }

        protected virtual void ApplyStat(float finalModifier, bool increase, KeyValuePair<string, bool> stat, T genericRecord = null)
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
                case "Weight":
                    PathOfQuasimorph.raritySystem.Apply<float>(v => itemRecord.Weight = v, () => genericRecord.Weight, finalModifier, increase, out outOldValue, out outNewValue);
                    break;
            }

            Plugin.Logger.Log($"\t\t old value {outOldValue}");
            Plugin.Logger.Log($"\t\t new value {outNewValue}");
        }


        protected virtual List<string> GetTraitsList() => null;
        protected virtual T GetGenericRecord() => null;
        protected virtual ItemTraitType? GetTraitType() => null;
        protected virtual List<HashSet<string>> GetMutuallyExclusiveGroups() => new();
        protected virtual bool IsMelee() => false;
        protected virtual HashSet<string> GetBlacklist() => new();

        protected virtual Dictionary<string, int> BuildTraitWeightsDictionary(IEnumerable<string> allowedTraits)
        {
            // Default: flat weight of 5 for each allowed trait
            return allowedTraits.ToDictionary(trait => trait, trait => 5);
        }

        internal virtual void ApplyTraits(bool clearTraits, float removeChance = 0.2f, bool tryToKeepGeneric = false)
        {
            Plugin.Logger.Log($"ApplyTraits: clearTraits={clearTraits}, removeChance={removeChance}, tryToKeepGeneric={tryToKeepGeneric}");

            var traitsList = GetTraitsList();
            var genericRecord = GetGenericRecord();
            var traitType = GetTraitType();

            if (traitsList == null || traitType == null)
            {
                Plugin.Logger.Log($"ApplyTraits: Not supported for this item type.");
                return;
            }

            var extraTraitCount = 0;
            bool keepGeneric = false;

            // Only attempt to keep generic traits if:
            // - Caller wants to try, AND
            // - There are generic traits available (weaponRecordGeneric exists)
            if (tryToKeepGeneric && genericRecord != null)
            {
                // Roll the dice: chance to actually keep them is based on `removeChance`
                // Example: removeChance = 0.2 → 20% chance to keep, 80% to strip
                keepGeneric = Helpers._random.NextDouble() < removeChance;
                Plugin.Logger.Log($"Rolled to keep generic traits: {keepGeneric} (chance: {removeChance})");
            }
            else
            {
                Plugin.Logger.Log("Not attempting to keep generic traits.");
            }

            Plugin.Logger.Log($"Final keepGeneric: {keepGeneric}");

            // Log existing traits
            Plugin.Logger.Log($"\tExisting traits: {traitsList.Count}");
            foreach (var trait in traitsList)
            {
                Plugin.Logger.Log($"\t\t {trait}");
            }

            // Log generic traits
            if (genericRecord != null)
            {
                var genericTraits = GetTraitsFromRecord(genericRecord);
                Plugin.Logger.Log($"\tGeneric traits: {genericTraits.Count}");

                foreach (var trait in genericTraits)
                {
                    Plugin.Logger.Log($"\t\t {trait}");
                }

                extraTraitCount = keepGeneric ? 0 : genericTraits.Count;
                Plugin.Logger.Log($"\textraTraitCount: {extraTraitCount}");
            }

            // Clear existing traits based on rules
            if (clearTraits)
            {
                traitsList.Clear();
                if (keepGeneric && genericRecord != null)
                {
                    Plugin.Logger.Log("Re-adding generic traits.");
                    traitsList.AddRange(GetTraitsFromRecord(genericRecord));
                }
                else
                {
                    Plugin.Logger.Log("Not re-adding generic traits.");
                }
            }
            else if (Helpers._random.NextDouble() < removeChance)
            {
                // 20% chance to remove existing traits even if not clearing
                Plugin.Logger.Log("Randomly clearing existing traits.");
                traitsList.Clear();
            }
            else
            {
                Plugin.Logger.Log("Preserving existing traits.");
            }

            // Select and apply new traits
            var selectedTraits = PrepareTraits(extraTraitCount);
            Plugin.Logger.Log($"\tSelected traits: {selectedTraits.Count}");
            foreach (var trait in selectedTraits)
            {
                Plugin.Logger.Log($"\t\t {trait}");
            }

            // Add traits
            traitsList.AddRange(selectedTraits);
            Plugin.Logger.Log($"\tFinal trait count: {traitsList.Count}");
            foreach (var trait in traitsList)
            {
                Plugin.Logger.Log($"\t\t {trait}");
            }
        }

        private List<string> PrepareTraits(int extraTraitCount)
        {
            // Allowed traits for item type
            var allowedTraits = itemRecordsControllerPoq.GetAddeableTraits((ItemTraitType)GetTraitType());

            if (!allowedTraits.Any())
            {
                Plugin.Logger.Log("PrepareTraits: No allowed traits found.");
                return new List<string>();
            }

            // Combined dictionary of traits
            // Let subclass decide how to assign weights (flat, positive/negative, etc.)
            var allTraitsCombined = BuildTraitWeightsDictionary(allowedTraits);

            Helpers.ShuffleDictionary(allTraitsCombined);

            // Warn about missing allowed traits
            foreach (var trait in allowedTraits)
            {
                if (!allTraitsCombined.ContainsKey(trait))
                {
                    Plugin.Logger.LogWarning($"[WARNING] Allowed trait '{trait}' not in positive/negative trait weights.");
                }
            }

            // Determine total number of traits to add based on rarity
            //var totalTraitCount = PathOfQuasimorph.raritySystem.GetTraitCountByRarity(itemRarity, allTraitsCombined.Count + extraTraitCount);
            var totalTraitCount = (int)itemRarity + extraTraitCount;

             // Select traits based on weights
            var selectedTraits = SelectWeightedTraits(allTraitsCombined, totalTraitCount, GetTraitsList(), GetMutuallyExclusiveGroups());

            // Apply blacklist (e.g., ranged vs melee)
            var blacklist = GetBlacklist();
            selectedTraits.RemoveAll(t => blacklist.Contains(t));

            // Final filter, all traits if they are not in allowed list (just in case)
            selectedTraits.RemoveAll(t => !allowedTraits.Contains(t));
            return selectedTraits;
        }

        private List<string> GetTraitsFromRecord(T record)
        {
            // Assumes T has a `.Traits` field — unsafe but acceptable if all T do
            return typeof(T).GetProperty("Traits")?.GetValue(record) as List<string>
                   ?? new List<string>();
        }

        internal List<string> SelectWeightedTraits(Dictionary<string, int> traitWeights, int count, List<string> itemTraitsExisting, List<HashSet<string>> exclusiveGroups = null)
        {
            var availableTraits = traitWeights
                .Where(t => t.Value > 0) // Skip traits with 0 or negative weight
                .ToDictionary(t => t.Key, t => t.Value);

            // Normalize exclusiveGroups to avoid null checks
            var groups = exclusiveGroups;

            if (groups == null)
            {
                groups = new List<HashSet<string>>();
            }

            var selected = new List<string>();

            while (selected.Count < count && availableTraits.Count > 0)
            {
                // Select one trait using weighted randomness
                string selectedTrait = PathOfQuasimorph.raritySystem.SelectRarityWeighted<string>(availableTraits);

                // Remove the selected trait from pool
                availableTraits.Remove(selectedTrait);

                // Remove all conflicting traits from the same exclusive group
                for (int i = 0; i < groups.Count; i++)
                {
                    var group = groups[i];
                    if (group.Contains(selectedTrait))
                    {
                        // Remove all members of this group from available traits
                        foreach (var conflict in group)
                        {
                            availableTraits.Remove(conflict);
                        }
                        break;
                    }
                }

                // Only add the trait if it's not already in itemTraitsExisting
                if (!itemTraitsExisting.Contains(selectedTrait))
                {
                    selected.Add(selectedTrait);
                }

                // Even if it exists, we still remove it and it's group members as we don't need them no more.
            }

            return selected;
        }
    }
}