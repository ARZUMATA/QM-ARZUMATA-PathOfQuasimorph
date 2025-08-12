using MGSC;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static MGSC.SpawnSystem;
using static MGSC.TurnDebugLogger;
using static QM_PathOfQuasimorph.Core.PathOfQuasimorph;
using Random = System.Random;

namespace QM_PathOfQuasimorph.Core.Processors
{
    internal abstract class ItemRecordProcessor<T>
    {
        protected T itemRecord;
        protected ItemRecordsControllerPoq itemRecordsControllerPoq;
        protected Logger _logger = new Logger(null, typeof(ItemRecordProcessor<T>));

        public abstract Dictionary<string, bool> parameters { get; }
        protected ItemRarity itemRarity;
        protected bool mobRarityBoost;
        protected bool amplifierRarityBoost;
        protected string itemId;
        protected string oldId;

        internal ItemRecordProcessor(ItemRecordsControllerPoq itemRecordsControllerPoq)
        {
            this.itemRecordsControllerPoq = itemRecordsControllerPoq;
        }

        internal virtual void Init(T itemRecord, ItemRarity itemRarity, bool mobRarityBoost, bool amplifierRarityBoost, string itemId, string oldId)
        {
            this.itemRecord = itemRecord;
            this.itemRarity = itemRarity;
            this.mobRarityBoost = mobRarityBoost;
            this.amplifierRarityBoost = amplifierRarityBoost;
            this.itemId = itemId;
            this.oldId = oldId;
        }

        internal abstract void ProcessRecord(ref string boostedParamString);

        internal float GetFinalModifier(float baseModifier, int numToHinder, int numToImprove, ref int improvedCount, ref int hinderedCount, string boostedParamString, ref bool increase, string statStr, bool statBool, Logger _logger)
        {
            float finalModifier;

            if (statBool == false)
            {
                increase = false;
            }
            else if (statBool == true)
            {
                increase = true;
            }

            _logger.Log($"Updating {statStr}");

            // Apply boost
            if (statStr != string.Empty && statStr == boostedParamString)
            {
                finalModifier = baseModifier * (float)Math.Round(Helpers._random.NextDouble() * (RaritySystem.PARAMETER_BOOST_MAX - RaritySystem.PARAMETER_BOOST_MIN) + RaritySystem.PARAMETER_BOOST_MIN, 2);

                _logger.Log($"\t\t boostedParamString exist, boosting final modifier from {baseModifier} to {finalModifier}");
            }
            else
            {
                finalModifier = baseModifier;
            }

            // Determine if we should hinder this parameter
            bool hinder = PathOfQuasimorph.raritySystem.ShouldHinderParameter(ref hinderedCount, ref improvedCount, numToHinder, numToImprove);

            if (hinder)
            {
                increase = !increase;
            }

            _logger.Log($"\t\t finalModifier: {finalModifier} hinder: {hinder}, boosted: {finalModifier != baseModifier}");
            return finalModifier;
        }

        internal void PrepGenericData(out float baseModifier, out float finalModifier, out int numToHinder, out int numToImprove, out string boostedParamString, out int improvedCount, out int hinderedCount, out bool increase)
        {
            baseModifier = PathOfQuasimorph.raritySystem.GetRarityModifier(itemRarity, PathOfQuasimorph.raritySystem._rarityModifiers);

            if (mobRarityBoost)
            {
                float mobModifier = baseModifier * PathOfQuasimorph.raritySystem.GetRarityModifier(MobContext.Rarity, PathOfQuasimorph.creaturesControllerPoq._masteryModifiers);
                _logger.Log($"\t\t mobRarityBoost exist, MobContext Rarity: {MobContext.Rarity}, CurrentMobId: {MobContext.CurrentMobId}");
                _logger.Log($"\t\t boosting final modifier from {baseModifier} to {mobModifier}");

                baseModifier = mobModifier;
            }

            if (amplifierRarityBoost)
            {
                float ampModifier = baseModifier * PathOfQuasimorph.raritySystem.GetRarityModifier(itemRarity, PathOfQuasimorph.raritySystem._rarityModifiers);
                _logger.Log($"\t\t amplifierRarityBoost exist, Rarity: {itemRarity}");
                _logger.Log($"\t\t boosting final modifier from {baseModifier} to {ampModifier}");

                baseModifier = ampModifier;
            }

            finalModifier = 0;
            var (Min, Max) = PathOfQuasimorph.raritySystem.rarityParamPercentages[itemRarity];

            int minParams = Math.Max(0, (int)Math.Floor(Min * parameters.Count));
            int maxParams = (int)Math.Ceiling(Max * parameters.Count);

            // Calculate the number of parameters to adjust based on the percentage
            int numToAdjust = Helpers._random.Next(minParams, maxParams + 1);

            numToHinder = (int)Math.Floor(numToAdjust * PathOfQuasimorph.raritySystem.PARAMETER_HINDER_PERCENT / 100f);
            numToImprove = numToAdjust - numToHinder;

            // Shuffle the list
            Helpers.ShuffleDictionary(parameters);

            // Select one parameter to boost more.
            // This parameter will be boosted more than the others.
            // We return index of parameter that was boosted for UID
            var boostedParam = parameters.Count == 0 ? 99 : Helpers._random.Next(parameters.Count);

            _logger.Log($"\t\t boostedParam: {boostedParam}, parameters.Count: {parameters.Count}");

            boostedParamString = boostedParam == 99 ? string.Empty : parameters.Keys.ToList()[boostedParam];

            // Counters to track how many parameters we've improved or hindered
            improvedCount = 0;
            hinderedCount = 0;

            // Determine if we need increase or decrease
            increase = true;
        }

        //protected abstract List<DmgResist> GetResistSheet();
        //protected abstract float GetResist(string resistName);
        //protected abstract void SetResist(string resistName, float value);
        //protected abstract float GetWeight();
        //protected abstract void SetWeight(float value);
        //protected abstract int GetMaxDurability();
        //protected abstract void SetMaxDurability(int value);

        // internal List<string> SelectWeightedTraits(Dictionary<string, int> traitWeights, int count
        internal List<string> SelectWeightedTraits(Dictionary<string, int> traitWeights, int count, List<HashSet<string>> exclusiveGroups = null)
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

            // for (int i = 0; i < count && availableTraits.Count > 0; i++)
            // {
            //     string selectedTrait = PathOfQuasimorph.raritySystem.SelectRarityWeighted<string>(availableTraits);
            //     selected.Add(selectedTrait);

            //     // Remove already selected trait to prevent duplicates
            //     availableTraits = availableTraits
            //         .Where(t => t.Key != selectedTrait)
            //         .ToDictionary(t => t.Key, t => t.Value);
            // }


            while (selected.Count < count && availableTraits.Count > 0)
            {
                // Select one trait using weighted randomness
                string selectedTrait = PathOfQuasimorph.raritySystem.SelectRarityWeighted<string>(availableTraits);
                selected.Add(selectedTrait);

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
            }

            return selected;
        }

    }
}