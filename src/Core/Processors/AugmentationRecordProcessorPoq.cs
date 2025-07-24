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
    internal class AugmentationRecordProcessorPoq : ItemRecordProcessor<AugmentationRecord>
    {
        private new Logger _logger = new Logger(null, typeof(AugmentationRecordProcessorPoq));

        public override List<string> parameters => _parameters;

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
        };

        public AugmentationRecordProcessorPoq(ItemRecordsControllerPoq itemRecordsControllerPoq) : base(itemRecordsControllerPoq)
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

        private int ApplyParameters()
        {
            float baseModifier, finalModifier;
            int numToHinder, numToImprove, boostedParam, improvedCount, hinderedCount;
            string boostedParamString;
            bool increase;
            PrepGenericData(out baseModifier, out finalModifier, out numToHinder, out numToImprove, out boostedParam, out boostedParamString, out improvedCount, out hinderedCount, out increase);

            // Simply for logging
            float outOldValue = -1;
            float outNewValue = -1;

            Plugin.Logger.Log($"itemRecord.WoundSlotIds Count: {itemRecord.WoundSlotIds.Count}");

            foreach (var woundSlot in itemRecord.WoundSlotIds)
            {
                Plugin.Logger.Log($"woundSlot: {woundSlot}");

                // We a not doing any boosts here neither creating new would slots, just chosing random slot that fits our SlotType.
                var originalSlot = Data.WoundSlots.GetRecord(itemRecord.Id, true);
                if (originalSlot == null)
                {
                    continue;
                }

                string slotType = originalSlot.SlotType;

                // Get all available slots with matching SlotType
                var matchingSlots = Data.WoundSlots.Records
                    .Where(x => x.SlotType == slotType)
                    .ToList();

                if (matchingSlots.Count == 0)
                {
                    Plugin.Logger.Log($"No wound slots found for type: {slotType}");
                    continue;
                }

                // Pick a random slot from the matching ones
                var newSlot = matchingSlots[Helpers._random.Next(matchingSlots.Count)];

                Plugin.Logger.Log($"Assigned new wound slot: {newSlot.Id} (type: {newSlot.SlotType})");
            }

            return boostedParam;
        }
    }
}
