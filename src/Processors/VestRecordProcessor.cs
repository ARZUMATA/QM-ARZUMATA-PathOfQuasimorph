using MGSC;
using QM_PathOfQuasimorph.Controllers;
using QM_PathOfQuasimorph.Core;
using QM_PathOfQuasimorph.Records;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QM_PathOfQuasimorph.Processors
{
    internal class VestRecordProcessor<T> : ResistItemProcessor<T> where T : VestRecord
    {
        //private new Logger _logger = new Logger(null, typeof(VestRecordProcessor));
        public override Dictionary<string, bool> parameters => _parameters;

        public VestRecordProcessor(ItemRecordsControllerPoq controller) : base(controller) 
        { 
            // Extend the base parameters
            _parameters["SlotCapacity"] = true;
            _parameters["ReloadTurnMod"] = false;
        }

        internal override void ProcessRecord(ref string boostedParamString)
        {
            ApplyParameters(ref boostedParamString);
        }

        protected override void ApplyStat(float finalModifier, bool increase, ref float averageResist, ref bool averageResistApplied, KeyValuePair<string, bool> stat, T genericRecord = null)
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

                default:
                    // For all other stats (resists, weight, durability), use base logic
                    base.ApplyStat(finalModifier, increase, ref averageResist, ref averageResistApplied, stat, genericRecord);
                    return;
            }

            Plugin.Logger.Log($"\t\t old value {outOldValue}");
            Plugin.Logger.Log($"\t\t new value {outNewValue}");
        }
    }
}