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
    internal abstract class ResistItemProcessor<T> : ItemRecordProcessor<T> where T : ResistRecord
    {
        private new Logger _logger = new Logger(null, typeof(ResistItemProcessor<T>));

        public override Dictionary<string, bool> parameters => _parameters;


        internal Dictionary<string, bool> _parameters = new Dictionary<string, bool>()
        {
            { "resist_blunt", true },
            { "resist_pierce", true },
            { "resist_lacer", true },
            { "resist_fire", true },
            { "resist_beam", true },
            { "resist_shock", true },
            { "resist_poison", true },
            { "resist_cold", true },
            { "weight", false },
            { "max_durability", true },
            //"none"
        };

        protected ResistItemProcessor(ItemRecordsControllerPoq itemRecordsControllerPoq) : base(itemRecordsControllerPoq)
        {
            // Override logger to use the actual derived type name
            _logger = new Logger(null, GetType()); // Ensures logger shows HelmetRecordProcessor, etc.
        }

        internal override void ProcessRecord(ref string boostedParamString)
        {
            if (itemRarity == ItemRarity.Standard)
            {
                return;
            }

            ApplyParameters(ref boostedParamString);
        }

        private void ApplyParameters(ref string boostedParamString)
        {
            float baseModifier, finalModifier;
            int numToHinder, numToImprove, improvedCount, hinderedCount;
            //string boostedParamString;
            bool increase;

            PrepGenericData(out baseModifier, out finalModifier, out numToHinder, out numToImprove, out boostedParamString, out improvedCount, out hinderedCount, out increase);

            float averageResist;
            bool averageResistApplied;
            GetAverageResists(out averageResist, out averageResistApplied);

            // Apply modifiers
            foreach (var stat in parameters)
            {
                finalModifier = GetFinalModifier(baseModifier, numToHinder, numToImprove, ref improvedCount, ref hinderedCount, boostedParamString, ref increase, stat.Key, stat.Value, _logger);
                ApplyStat(finalModifier, increase, ref averageResist, ref averageResistApplied, stat);
            }
        }

        private void GetAverageResists(out float averageResist, out bool averageResistApplied, T genericRecord = null)
        {
            Plugin.Logger.Log($"GetAverageResists");
            Plugin.Logger.Log($"\t itemRecord Id: {itemRecord.Id}");

            // Get average resist for armor
            averageResist = 0;
            int resistCount = 0;
            var resistSheet = itemRecord.ResistSheet;
            averageResistApplied = false;

            // When we calculate average resists for an armor, we always need to check generic record.
            // Fallback to existing item record remains.
            if (genericRecord != null)
            {
                resistSheet = genericRecord.ResistSheet;

                Plugin.Logger.Log($"\t genericRecord resistSheet for oldId: {genericRecord.Id}");
            }

            _logger.Log($"\t\t\t\t itemRecord null {itemRecord == null}");
            _logger.Log($"\t\t\t\t resistSheet null {resistSheet == null}");

            foreach (var res in resistSheet)
            {
                // If there is only one resist, it leads to imbalance as it can get applied to others.
                averageResist += res.resistPercent;
                resistCount++;
            }

            averageResist = resistCount > 0 ? (float)Math.Round(averageResist / resistCount, 2) : 0f;
            averageResist = Math.Max(averageResist, 1.0f); // Ensure average resist is at least 1.0
            _logger.Log($"\t\t\t\t Average resist {averageResist} for total count {resistCount}");
        }

        private void ApplyStat(float finalModifier, bool increase, ref float averageResist, ref bool averageResistApplied, KeyValuePair<string, bool> stat, T genericRecord = null)
        {
            // Simply for logging
            float outOldValue = -1;
            float outNewValue = -1;

            if (genericRecord == null)
            {
                genericRecord = itemRecord;
            }

            if (stat.Key.Contains("resist"))
            {
                var resistName = stat.Key.Split('_')[1];
                var resistValue = genericRecord.GetResist(resistName);
                _logger.Log($"\t\t\t resist {resistName} with original value: {resistValue}");

                if (resistValue == 0)
                {
                    if (averageResistApplied == false)
                    {
                        // Roll random
                        var canApply = Helpers._random.Next(0, 100 + 1) < RaritySystem.AVERAGE_RESIST_APPLY_CHANCE;

                        if (canApply)
                        {
                            _logger.Log($"\t\t\t Resist with defaultValue {resistValue}, setting to {averageResist} (averageResist)");
                            PathOfQuasimorph.raritySystem.ApplyModifier<float>(ref averageResist, finalModifier, increase, out outOldValue, out outNewValue);
                            itemRecord.SetResist(resistName, averageResist);
                            averageResistApplied = true;
                        }
                    }
                }
                else
                {
                    PathOfQuasimorph.raritySystem.ApplyModifier<float>(ref resistValue, finalModifier, increase, out outOldValue, out outNewValue);
                    itemRecord.SetResist(resistName, outNewValue);
                }
            }
            else
            {
                switch (stat.Key)
                {
                    case "weight":
                        PathOfQuasimorph.raritySystem.Apply<float>(
                            v => itemRecord.Weight = v,
                            () => genericRecord.Weight,
                            finalModifier,
                            increase,
                            out outOldValue,
                            out outNewValue);
                        break;

                    case "max_durability":
                        PathOfQuasimorph.raritySystem.Apply<int>(
                            v => itemRecord.MaxDurability = v,
                            () => genericRecord.MaxDurability,
                            finalModifier,
                            increase,
                            out outOldValue,
                            out outNewValue);
                        break;
                }
            }

            Plugin.Logger.Log($"\t\t old value {outOldValue}");
            Plugin.Logger.Log($"\t\t new value {outNewValue}");
        }

        internal void RerollRandomStat(SynthraformerRecord ampRecord, MetadataWrapper metadata, bool blockHinder)
        {
            Plugin.Logger.Log($"RerollRandomStat");

            var genericRecord = Data.Items.GetSimpleRecord<T>(metadata.Id, true);
            Plugin.Logger.Log($"\t genericRecord null: {genericRecord == null}");
            Plugin.Logger.Log($"\t genericRecord null for oldId: {genericRecord.Id}");

            float baseModifier, finalModifier;
            int numToHinder, numToImprove, improvedCount, hinderedCount;
            string boostedParamString;
            bool increase;
            PrepGenericData(out baseModifier, out finalModifier, out numToHinder, out numToImprove, out boostedParamString, out improvedCount, out hinderedCount, out increase);

            Plugin.Logger.Log($"RerollRandomStat");
            Plugin.Logger.Log($"metadata: {metadata.BoostedString}");

            // During rarity generation, PrepGenericData may have set a boosted string. However, since this method is reused later,
            // we must preserve the original boosted string from metadata to prevent unintended boosting of all resist stats.
            // This ensures only the stat originally chosen during initial rarity roll (or reroll) remains boosted.
            if (metadata.BoostedString.Length > 1)
            {
                boostedParamString = metadata.BoostedString;
            }

            if (blockHinder)
            {
                hinderedCount = 999; // Test
            }

            float averageResist;
            bool averageResistApplied;
            GetAverageResists(out averageResist, out averageResistApplied, genericRecord);

            averageResistApplied = true; // We do reroll, so if average res been applied then it's been applied.

            var statIdx = Helpers._random.Next(0, parameters.Count);
            var stat = parameters.ElementAt(statIdx);

            finalModifier = GetFinalModifier(baseModifier, numToHinder, numToImprove, ref improvedCount, ref hinderedCount, boostedParamString, ref increase, stat.Key, stat.Value, _logger);

            ApplyStat(finalModifier, increase, ref averageResist, ref averageResistApplied, stat, genericRecord);
        }
    }
}