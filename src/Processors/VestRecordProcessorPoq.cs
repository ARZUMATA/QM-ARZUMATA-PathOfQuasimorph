using MGSC;
using QM_PathOfQuasimorph.Controllers;
using QM_PathOfQuasimorph.Core;
using QM_PathOfQuasimorph.Records;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QM_PathOfQuasimorph.Processors
{
    internal class VestRecordProcessorPoq : ResistItemProcessor<VestRecord>
    {
        private new Logger _logger = new Logger(null, typeof(VestRecordProcessorPoq));
        public override Dictionary<string, bool> parameters => _parameters;

        public VestRecordProcessorPoq(ItemRecordsControllerPoq controller) : base(controller) { }

        internal new Dictionary<string, bool> _parameters = new Dictionary<string, bool>()
        {
           { "SlotCapacity", true },
           { "ReloadTurnMod", false },
        };

        internal override void ProcessRecord(ref string boostedParamString)
        {
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

        private void ApplyStat(float finalModifier, bool increase, KeyValuePair<string, bool> stat, VestRecord genericRecord = null)
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
                case "SlotCapacity":
                    PathOfQuasimorph.raritySystem.Apply<int>(v => itemRecord.SlotCapacity = v, () => genericRecord.SlotCapacity, finalModifier, increase, out outOldValue, out outNewValue);
                    break;


                case "ReloadTurnMod":
                    PathOfQuasimorph.raritySystem.Apply<int>(v => itemRecord.ReloadTurnMod = v, () => genericRecord.ReloadTurnMod, finalModifier, increase, out outOldValue, out outNewValue);
                    break;
            }

            Plugin.Logger.Log($"\t\t old value {outOldValue}");
            Plugin.Logger.Log($"\t\t new value {outNewValue}");
        }
    }
}