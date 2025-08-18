using MGSC;
using Newtonsoft.Json;
using QM_PathOfQuasimorph.Controllers;
using QM_PathOfQuasimorph.Core;
using QM_PathOfQuasimorph.PoQHelpers;
using QM_PathOfQuasimorph.Records;
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

namespace QM_PathOfQuasimorph.Processors
{
    internal class AugmentationRecordProcessorPoq : ItemRecordProcessor<AugmentationRecord>
    {
        private new Logger _logger = new Logger(null, typeof(AugmentationRecordProcessorPoq));

        public override Dictionary<string, bool> parameters => _parameters;

        internal Dictionary<string, bool> _parameters = new Dictionary<string, bool>
        {
        };

        public AugmentationRecordProcessorPoq(ItemRecordsControllerPoq itemRecordsControllerPoq) : base(itemRecordsControllerPoq)
        {
        }

        internal override void ProcessRecord(ref string boostedParamString)
        {
            if (itemRarity == ItemRarity.Standard)
            {
                return;
            }

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

            bool addNewSlots = false; // I don't like how it works.
            bool randomizeSlotsStats = true;

            int howManySlotsToAdd = (int)itemRarity;
            int howManySlotsAdded = 0;

            // Add new wound slots with matched type based on rarity

            if (addNewSlots)
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
                                !itemRecord.WoundSlotIds.Contains(x.Id) &&
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

                    _logger.Log($"newSlot {newSlot.Id}:");

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
                    howManySlotsAdded++;

                    Plugin.Logger.Log($"Added new wound slot: {newSlot.Id} (type: {newSlot.SlotType})");

                    if (howManySlotsToAdd == howManySlotsAdded)
                    {
                        Plugin.Logger.Log($"Added slots {howManySlotsToAdd}");

                        break;
                    }
                }

                Plugin.Logger.Log($"counts should match. {itemRecord.WoundSlotIds.Count} == {newWoundSlotIds.Count}");

                //if (itemRecord.WoundSlotIds.Count == newWoundSlotIds.Count)
                //{
                itemRecord.WoundSlotIds.AddRange(newWoundSlotIds);
                //}
            }


            if (randomizeSlotsStats)
            {
                Plugin.Logger.Log($"\t randomizeSlotsStats");

                foreach (var woundSlot in itemRecord.WoundSlotIds)
                {
                    Plugin.Logger.Log($"\t processing wouldSlot: {woundSlot}");

                    var newId = $"{woundSlot}_{itemId}";
                    Plugin.Logger.Log($"\t new name will be {newId}");

                    var woundSlotRecord = Data.WoundSlots.GetRecord(woundSlot);
                    WoundSlotRecord woundSlotRecordNew = ItemRecordHelpers.CloneWoundSlotRecord(woundSlotRecord, $"{newId}");
                    itemRecordsControllerPoq.woundSlotRecordProcessorPoq.Init(woundSlotRecordNew, itemRarity, mobRarityBoost,false, $"{newId}", oldId);
                    itemRecordsControllerPoq.woundSlotRecordProcessorPoq.ProcessRecord(ref boostedParamString);
                    //records.Add(augmentationRecordNew);
                    // TODO

                    newWoundSlotIds.Add($"{newId}");

                    Data.WoundSlots.AddRecord($"{newId}", woundSlotRecordNew);
                    RecordCollection.WoundSlotRecords.Add($"{newId}", woundSlotRecordNew);
                    Localization.DuplicateKey("woundslot." + woundSlot + ".name", "woundslot." + newId + ".name");
                }

                Plugin.Logger.Log($"counts should match. {itemRecord.WoundSlotIds.Count} == {newWoundSlotIds.Count}");

                if (itemRecord.WoundSlotIds.Count == newWoundSlotIds.Count)
                {
                    itemRecord.WoundSlotIds = newWoundSlotIds;
                }
            }
        }

        internal void AddRandomEffect(SynthraformerRecord record, MetadataWrapper metadata)
        {
        }
    }
}
