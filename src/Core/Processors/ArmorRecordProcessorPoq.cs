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
using static MGSC.TurnDebugLogger;
using Random = System.Random;

namespace QM_PathOfQuasimorph.Core.Processors
{
    internal class ArmorRecordProcessorPoq : ItemRecordProcessor<ArmorRecord>
    {
        //ArmorRecord armorRecord;
        private new Logger _logger = new Logger(null, typeof(ArmorRecordProcessorPoq));

        public override List<string> parameters => _parameters;

        internal List<string> _parameters = new List<string>()
        {
            "resist_blunt",
            "resist_pierce",
            "resist_lacer",
            "resist_fire",
            "resist_beam",
            "resist_shock",
            "resist_poison",
            "resist_cold",
            "weight",
            "max_durability",
            //"none"
        };

        public ArmorRecordProcessorPoq(ItemRecordsControllerPoq itemRecordsControllerPoq) : base(itemRecordsControllerPoq)
        {
        }

        internal override int ProcessRecord()
        {
            if (itemRarity == ItemRarity.Standard)
            {
                return 99;
            }

            int boostedStat;

            boostedStat = ApplyParameters();

            return boostedStat;
        }

        internal int ApplyParameters()
        {
            float baseModifier, finalModifier;
            int numToHinder, numToImprove, boostedParam, improvedCount, hinderedCount;
            string boostedParamString;
            bool increase;
            PrepGenericData(out baseModifier, out finalModifier, out numToHinder, out numToImprove, out boostedParam, out boostedParamString, out improvedCount, out hinderedCount, out increase);

            // Get average resist for armor.
            float averageResist = 0;
            int resistCount = 0;

            var resistSheet = itemRecord.ResistSheet;

            foreach (var res in resistSheet)
            {
                //_logger.Log($"\t\t Resist: {param.Id}");

                // If there is only one resist, it leads to imbalance as it can get applied to others.
                averageResist += res.resistPercent;
                resistCount++;
            }

            averageResist = (float)Math.Round(averageResist / resistCount, 2);
            averageResist = Math.Max(averageResist, 1.0f); // Ensure average resist is at least 1.0
            _logger.Log($"\t\t\t\t Average resist {averageResist} for total count {resistCount}");

            foreach (var stat in parameters)
            {
                finalModifier = GetFinalModifier(baseModifier, numToHinder, numToImprove, ref improvedCount, ref hinderedCount, boostedParamString, ref increase, stat, _logger);

                // Simply for logging
                float outOldValue = -1;
                float outNewValue = -1;

                if (stat.Contains("resist"))
                {
                    var resistName = stat.Split('_')[1]; // :D
                    var resistValue = itemRecord.GetResist(resistName);
                    PathOfQuasimorph.raritySystem.ApplyModifier<float>(ref resistValue, finalModifier, increase, out outOldValue, out outNewValue);
                    itemRecord.SetResist(resistName, outNewValue);
                }
                else
                {
                    switch (stat)
                    {
                        case "weight":
                            PathOfQuasimorph.raritySystem.Apply<float>(v => itemRecord.Weight = v, () => itemRecord.Weight, finalModifier, increase, out outOldValue, out outNewValue);
                            break;

                        case "max_durability":
                            PathOfQuasimorph.raritySystem.Apply<int>(v => itemRecord.MaxDurability = v, () => itemRecord.MaxDurability, finalModifier, increase, out outOldValue, out outNewValue);
                            break;

                        case "none":
                            break;
                    }
                }

                Plugin.Logger.Log($"\t\t old value {outOldValue}");
                Plugin.Logger.Log($"\t\t new value {outNewValue}");
            }

            return boostedParam;
        }
    }
}
