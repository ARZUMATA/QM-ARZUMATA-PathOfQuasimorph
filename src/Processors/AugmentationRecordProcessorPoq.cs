using MGSC;
using Newtonsoft.Json;
using QM_PathOfQuasimorph.Controllers;
using QM_PathOfQuasimorph.Core;
using QM_PathOfQuasimorph.PoQHelpers;
using QM_PathOfQuasimorph.Records;
using QM_PathOfQuasimorph.PoqHelpers;
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
                    itemRecordsControllerPoq.woundSlotRecordProcessorPoq.Init(woundSlotRecordNew, itemRarity, mobRarityBoost, false, $"{newId}", oldId);
                    itemRecordsControllerPoq.woundSlotRecordProcessorPoq.ProcessRecord(ref boostedParamString);

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

        internal bool AddRandomEffect(SynthraformerRecord record, MetadataWrapper metadata)
        {
            Plugin.Logger.Log($"AddRandomEffect");

            if (itemRecord.WoundSlotIds.Count == 0)
            {
                return false;
            }

            // Determine wound slot we can process
            var slotIdxToProcess = Helpers._random.Next(0, itemRecord.WoundSlotIds.Count);
            Plugin.Logger.Log($"slotIdxToProcess: {slotIdxToProcess}");

            for (int i = 0; i < itemRecord.WoundSlotIds.Count; i++)
            {
                if (i == slotIdxToProcess)
                {
                    var woundSlotString = itemRecord.WoundSlotIds[i];
                    var woundSlotRecord = Data.WoundSlots.GetRecord(woundSlotString);

                    itemRecordsControllerPoq.woundSlotRecordProcessorPoq.Init(woundSlotRecord, itemRarity, mobRarityBoost, false, woundSlotRecord.Id, oldId);
                    var success = itemRecordsControllerPoq.woundSlotRecordProcessorPoq.AddRandomImplicitEffect(metadata.RarityClass, woundSlotRecord.ImplicitBonusEffects, woundSlotRecord.ImplicitPenaltyEffects);

                    if (!success)
                    {
                        return false;
                    }

                    Data.WoundSlots._records[woundSlotRecord.Id] = woundSlotRecord;
                    RecordCollection.WoundSlotRecords[woundSlotRecord.Id] = woundSlotRecord;
                    break;
                }
            }

            return true;
        }

        internal bool RandomWoundSlot(SynthraformerRecord record, MetadataWrapper metadata)
        {
            Plugin.Logger.Log($"RandomWoundSlot");

            string boostedParamString = string.Empty;
            Plugin.Logger.Log($"StripBodyPart");

            Plugin.Logger.Log($"\t parts:");

            // Extract body part types from existing wound slots
            var existingSlotTypes = itemRecord.WoundSlotIds
                .Select(id => PoqHelpers.PoqHelpers.StripBodyPart(id.Split('_')[0]).bodyPart) // Extract "Shoulder", "Arm", etc.
                .Where(part => part != null) // Only valid parts
                .Distinct()
                .ToList();

            Plugin.Logger.LogError($"\t existingSlotTypes:");

            foreach (var type in existingSlotTypes)
            {
                Plugin.Logger.LogError($"\t\t {type}");
            }

            if (!existingSlotTypes.Any())
            {
                Plugin.Logger.LogError($"\t Nothing to replace: existingSlotTypes");
                // Nothing to replace
                return false;
            }

            // Find all possible base IDs (e.g., "Moon", "Centaur") that have *all* required body parts defined in existingSlotTypes 
            var candidateBases = Data.WoundSlots.Records
                .GroupBy(x => PoqHelpers.PoqHelpers.StripBodyPart(x.Id).baseId)
                .Where(g => !string.IsNullOrEmpty(g.Key))
                .Select(g => new
                {
                    BaseId = g.Key,
                    AvailableParts = g.Select(x => PoqHelpers.PoqHelpers.StripBodyPart(x.Id).bodyPart).ToHashSet()
                })

                .Where(x => existingSlotTypes.All(requiredPart => x.AvailableParts.Contains(requiredPart)))
                .Select(x => x.BaseId)
                // .Except(new[] { "Human", "Skinless" }) // Blacklist full base IDs (Optional: We can blacklist some we don't need)
                .Where(id => !MetadataWrapper.IsPoqItemUid(id)) // Avoid already-modified POQ variants
                .Distinct()
                .ToList();

            Plugin.Logger.LogError($"\t candidateBases:");

            foreach (var baseId in candidateBases)
            {
                Plugin.Logger.Log($"\t\t {baseId}");
            }

            if (!candidateBases.Any())
            {
                Plugin.Logger.Log($"\t Nothing in candidates: candidateBases");
                Plugin.Logger.LogError($"\t No complete set found for required parts: {string.Join(", ", existingSlotTypes)}");
                return false;
            }

            // Pick a random new base ID (e.g., "Moon")
            var newBaseId = candidateBases[Helpers._random.Next(candidateBases.Count)];
            Plugin.Logger.LogError($"\t newBaseId: {newBaseId}");

            // Build mapping: body part > available slot in new base
            var newSlotIds = new List<string>();

            foreach (var bodyPart in existingSlotTypes)
            {
                Plugin.Logger.Log($"\t processing: {bodyPart}");

                var replacementSlot = Data.WoundSlots.Records
                    .FirstOrDefault(x =>
                        PoqHelpers.PoqHelpers.StripBodyPart(x.Id) == (newBaseId, bodyPart) &&
                        !itemRecord.WoundSlotIds.Contains(x.Id)); // Avoid reusing same

                Plugin.Logger.Log($"\t replacementSlot == null {replacementSlot == null}");

                if (replacementSlot != null)
                {
                    Plugin.Logger.Log($"\t processing replacementSlot.Id: {replacementSlot.Id}");

                    var replacementSlotRecord = Data.WoundSlots.GetRecord(replacementSlot.Id);
                    var newId = $"{replacementSlotRecord.Id}_{itemId}";

                    Plugin.Logger.Log($"\t replacementSlot.Id will be: {newId}");

                    WoundSlotRecord woundSlotRecordNew = ItemRecordHelpers.CloneWoundSlotRecord(replacementSlotRecord, $"{newId}");
                    itemRecordsControllerPoq.woundSlotRecordProcessorPoq.Init(woundSlotRecordNew, itemRarity, mobRarityBoost, false, $"{newId}", oldId);
                    itemRecordsControllerPoq.woundSlotRecordProcessorPoq.ProcessRecord(ref boostedParamString);

                    newSlotIds.Add($"{newId}");

                    Data.WoundSlots._records[newId] = woundSlotRecordNew;
                    RecordCollection.WoundSlotRecords[newId] = woundSlotRecordNew;
                    Localization.DuplicateKey("woundslot." + replacementSlot.Id + ".name", "woundslot." + newId + ".name");
                }
            }

            // Replace all existing wound slots with new themed ones
            itemRecord.WoundSlotIds.Clear();
            itemRecord.WoundSlotIds.AddRange(newSlotIds);

            return true;
        }
    }
}
