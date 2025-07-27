using MGSC;
using Newtonsoft.Json;
using QM_PathOfQuasimorph.PoQHelpers;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Profiling;
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
        };

        public AugmentationRecordProcessorPoq(ItemRecordsControllerPoq itemRecordsControllerPoq) : base(itemRecordsControllerPoq)
        {
        }

        internal override void ProcessRecord()
        {
            ApplyParameters();
        }

        private void ApplyParameters()
        {
            float baseModifier, finalModifier;
            int numToHinder, numToImprove, improvedCount, hinderedCount;
            string boostedParamString;
            bool increase;
            PrepGenericData(out baseModifier, out finalModifier, out numToHinder, out numToImprove, out boostedParamString, out improvedCount, out hinderedCount, out increase);

            // Simply for logging
            float outOldValue = -1;
            float outNewValue = -1;

            Plugin.Logger.Log($"itemRecord.WoundSlotIds Count: {itemRecord.WoundSlotIds.Count}");
            List<string> newWoundSlotIds = new List<string>();

            bool swapWoundslots = false;
            bool randomizeSlotsStats = true;

            if (swapWoundslots)
            {
                foreach (var woundSlot in itemRecord.WoundSlotIds)
                {
                    Plugin.Logger.Log($"woundSlot: {woundSlot}, itemRecord.Id: {itemRecord.Id}");

                    // We a not doing any boosts here neither creating new would slots, just chosing random slot that fits our SlotType.

                    var originalSlot = Data.WoundSlots.GetRecord(woundSlot, false);

                    Plugin.Logger.Log($"originalSlot == null {originalSlot == null}");

                    if (originalSlot == null)
                    {
                        continue;
                    }

                    string slotType = originalSlot.SlotType;

                    // Get all available slots with matching SlotType
                    var matchingSlots = Data.WoundSlots.Records
                    .Where(x => x.SlotType == slotType &&
                                x.ImplicitBonusEffects?.Count > 0 &&
                                x.ImplicitPenaltyEffects?.Count > 0)
                    .ToList();

                    if (matchingSlots.Count == 0)
                    {
                        Plugin.Logger.Log($"No wound slots found for type: {slotType}");
                        continue;
                    }

                    // Pick a random slot from the matching ones
                    var newSlot = matchingSlots[Helpers._random.Next(matchingSlots.Count)];

                    _logger.Log($"ImplicitBonusEffects:");
                    foreach (var effect in newSlot.ImplicitBonusEffects)
                    {
                        _logger.Log($"\t\t {effect.Key} - {effect.Value}");
                    }

                    _logger.Log($"ImplicitPenaltyEffects:");
                    foreach (var effect in newSlot.ImplicitPenaltyEffects)
                    {
                        _logger.Log($"\t\t {effect.Key} - {effect.Value}");
                    }

                    newWoundSlotIds.Add(newSlot.Id);

                    Plugin.Logger.Log($"Added new wound slot: {newSlot.Id} (type: {newSlot.SlotType})");
                }

                Plugin.Logger.Log($"counts should match. {itemRecord.WoundSlotIds.Count} == {newWoundSlotIds.Count}");

                if (itemRecord.WoundSlotIds.Count == newWoundSlotIds.Count)
                {
                    itemRecord.WoundSlotIds = newWoundSlotIds;
                }
            }


            if (randomizeSlotsStats)
            {
                Plugin.Logger.Log($"\t randomizeSlotsStats");

                foreach (var woundSlot in itemRecord.WoundSlotIds)
                {
                    Plugin.Logger.Log($"\t processing wouldSlot: {woundSlot}");
                    Plugin.Logger.Log($"\t new name will be {woundSlot}_{itemId}");

                    var woundSlotRecord = Data.WoundSlots.GetRecord(woundSlot);
                    WoundSlotRecord woundSlotRecordNew = ItemRecordHelpers.CloneWoundSlotRecord(woundSlotRecord, $"{woundSlot}_{itemId}");
                    itemRecordsControllerPoq.woundSlotRecordProcessorPoq.Init(woundSlotRecordNew, itemRarity, mobRarityBoost, $"{woundSlot}_{itemId}");
                    itemRecordsControllerPoq.woundSlotRecordProcessorPoq.ProcessRecord();
                    //records.Add(augmentationRecordNew);
                    // TODO

                    newWoundSlotIds.Add($"{woundSlot}_{itemId}");

                    Data.WoundSlots.AddRecord($"{woundSlot}_{itemId}", woundSlotRecordNew);
                    RecordCollection.WoundSlotRecords.Add($"{woundSlot}_{itemId}", woundSlotRecordNew);
                }

                Plugin.Logger.Log($"counts should match. {itemRecord.WoundSlotIds.Count} == {newWoundSlotIds.Count}");

                if (itemRecord.WoundSlotIds.Count == newWoundSlotIds.Count)
                {
                    itemRecord.WoundSlotIds = newWoundSlotIds;
                }
            }
        }
    }
}
