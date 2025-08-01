﻿using MGSC;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static MGSC.TurnDebugLogger;

namespace QM_PathOfQuasimorph.Core.Processors
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

            // Get average resist for armor
            float averageResist = 0;
            int resistCount = 0;
            var resistSheet = itemRecord.ResistSheet;
            bool averageResistApplied = false;

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

            // Apply modifiers
            foreach (var stat in parameters)
            {
                finalModifier = GetFinalModifier(baseModifier, numToHinder, numToImprove, ref improvedCount, ref hinderedCount, boostedParamString, ref increase, string.Empty, true, _logger);

                // Simply for logging
                float outOldValue = -1;
                float outNewValue = -1;

                if (stat.Key.Contains("resist"))
                {
                    var resistName = stat.Key.Split('_')[1];
                    var resistValue = itemRecord.GetResist(resistName);

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
                                () => itemRecord.Weight,
                                finalModifier,
                                increase,
                                out outOldValue,
                                out outNewValue);
                            break;

                        case "max_durability":
                            PathOfQuasimorph.raritySystem.Apply<int>(
                                v => itemRecord.MaxDurability = v,
                                () => itemRecord.MaxDurability,
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
        }
    }
}