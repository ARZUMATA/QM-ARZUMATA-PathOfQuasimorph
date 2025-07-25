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
        };

        internal List<string> woundSlotImplicitBonusEffects = new List<string>()
        {
            "accuracy_reduce",
            "added_projectile",
            "added_wound_chance_mult",
            "backpack_weight",
            "bonus_vest_slot",
            "crit_damage",
            "critchance_reduce",
            "dodge_reduce",
            "firearm_range",
            "fov_angle",
            "income_pain",
            "items_weight",
            "los_reduce",
            "max_health",
            "melee_accuracy",
            "melee_dmg_reduce",
            "melee_throw_range",
            "multi_hit",
            "passive_regen",
            "qmorph",
            "ranged_accuracy",
            "regen_efficacy",
            "resist_beam",
            "resist_blunt",
            "resist_fire",
            "resist_lacer",
            "resist_pierce",
            "resist_poison",
            "resist_shock",
            "run_ap",
            "scatter_angle",
            "throwback_immune",
            "walk_spotted_signal",
            "wound_chance",
            "wound_heal_chance",
            "wound_immune_fire",
            "wound_immune_poison",
        };

        internal List<string> woundSlotImplicitPenaltyEffects = new List<string>()
        {
            "arm_slot_unavailable",
            "backpack_weight",
            "dodge_reduce",
            "fov_angle",
            "income_critchance",
            "los_reduce",
            "max_health",
            "melee_accuracy",
            "melee_dmg_reduce",
            "no_stealth",
            "qmorph",
            "ranged_accuracy",
            "regen_efficacy",
            "resist_blunt",
            "resist_fire",
            "resist_lacer",
            "resist_pierce",
            "resist_shock",
            "run_unavailable",
            "scatter_angle",
            "wound_chance",
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

            return ApplyParameters();
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
            }


            if (randomizeSlotsStats)
            {


            }



                Plugin.Logger.Log($"counts should match. {itemRecord.WoundSlotIds.Count} == {newWoundSlotIds.Count}");

            if (itemRecord.WoundSlotIds.Count == newWoundSlotIds.Count)
            {
                itemRecord.WoundSlotIds = newWoundSlotIds;
            }

            return boostedParam;
        }
    }
}
