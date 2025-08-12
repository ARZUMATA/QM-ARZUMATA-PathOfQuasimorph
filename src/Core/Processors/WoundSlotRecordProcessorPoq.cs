using MGSC;
using Newtonsoft.Json;
using QM_PathOfQuasimorph.PoQHelpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using static HarmonyLib.Code;
using static MGSC.SpawnSystem;
using Random = System.Random;

namespace QM_PathOfQuasimorph.Core.Processors
{
    internal class WoundSlotRecordProcessorPoq : ItemRecordProcessor<WoundSlotRecord>
    {
        private new Logger _logger = new Logger(null, typeof(WoundSlotRecordProcessorPoq));

        public override Dictionary<string, bool> parameters => _parameters;

        internal Dictionary<string, bool> _parameters = new Dictionary<string, bool>()
        {
        };

        internal List<string> implicitBonusEffects = new List<string>()
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

        internal List<string> implicitPenaltyEffects = new List<string>()
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

        internal List<string> coreEffects = new List<string>()
        {
            "accuracy_reduce",
            "action_dmg",
            "CoreEffects",
            "dodge_reduce",
            "dot_dmg",
            "los_reduce",
            "melee_dmg_reduce",
            "move_dmg",
            "vomiting",
        };
        public WoundSlotRecordProcessorPoq(ItemRecordsControllerPoq itemRecordsControllerPoq) : base(itemRecordsControllerPoq)
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


            // Even though we're updating a value (not adding a new key), in some contexts this can still be considered a modification that invalidates the enumerator.//
            // More importantly, any write operation to the dictionary during foreach enumeration is unsafe and can throw this exception.
            // Capture all entries safely for this case

            var entriesImplicitBonusEffects = itemRecord.ImplicitBonusEffects.ToList();
            var entriesImplicitPenaltyEffects = itemRecord.ImplicitPenaltyEffects.ToList();
            var entriesCoreEffects = itemRecord.CoreEffects.ToList();

            itemRecord.BareHandWeapon = CreateBareHandWeapon();
            _logger.Log($"itemRecord.BareHandWeapon now {itemRecord.BareHandWeapon}");

            foreach (KeyValuePair<string, float> keyValuePair in entriesImplicitBonusEffects)
            {
                finalModifier = GetFinalModifier(baseModifier, numToHinder, numToImprove, ref improvedCount, ref hinderedCount, boostedParamString, ref increase, string.Empty, true, _logger);

                float valueFinal = 0;

                WoundEffectRecord record = Data.WoundEffects.GetRecord(keyValuePair.Key, true);

                switch (record.ValueFormat)
                {
                    case EffectViewShowValueFormat.Raw:
                    case EffectViewShowValueFormat.MinusInt:
                    case EffectViewShowValueFormat.MinusDamage:
                    case EffectViewShowValueFormat.ReverseInt:
                        var value = (int)keyValuePair.Value;
                        PathOfQuasimorph.raritySystem.ApplyModifier<int>(ref value, finalModifier, increase, out outOldValue, out outNewValue);
                        valueFinal = value;
                        break;
                    case EffectViewShowValueFormat.Percent100:
                    case EffectViewShowValueFormat.Percent100NoPlus:
                    case EffectViewShowValueFormat.Percent100Abs:
                        var value2 = keyValuePair.Value;
                        PathOfQuasimorph.raritySystem.ApplyModifier<float>(ref value2, finalModifier, increase, out outOldValue, out outNewValue);
                        valueFinal = value2;
                        break;
                }

                //if (record.ValueFormat.ToString().Contains("int"))
                //{
                //    var value = (int)keyValuePair.Value;
                //    PathOfQuasimorph.raritySystem.ApplyModifier<int>(ref value, finalModifier, increase, out outOldValue, out outNewValue);
                //    valueFinal = value;
                //}
                //else
                //{
                //    var value = keyValuePair.Value;
                //    PathOfQuasimorph.raritySystem.ApplyModifier<float>(ref value, finalModifier, increase, out outOldValue, out outNewValue);
                //    valueFinal = value;
                //}

                itemRecord.ImplicitBonusEffects[keyValuePair.Key] = (float)valueFinal;

                _logger.Log($"\t\t old value {outOldValue}");
                _logger.Log($"\t\t new value {outNewValue}");
            }

            foreach (KeyValuePair<string, float> keyValuePair in entriesImplicitPenaltyEffects)
            {
                finalModifier = GetFinalModifier(baseModifier, numToHinder, numToImprove, ref improvedCount, ref hinderedCount, boostedParamString, ref increase, string.Empty, false, _logger);

                float valueFinal = 0;

                WoundEffectRecord record = Data.WoundEffects.GetRecord(keyValuePair.Key, true);

                switch (record.ValueFormat)
                {
                    case EffectViewShowValueFormat.Raw:
                    case EffectViewShowValueFormat.MinusInt:
                    case EffectViewShowValueFormat.MinusDamage:
                    case EffectViewShowValueFormat.ReverseInt:
                        var value = (int)keyValuePair.Value;
                        PathOfQuasimorph.raritySystem.ApplyModifier<int>(ref value, finalModifier, increase, out outOldValue, out outNewValue);
                        valueFinal = value;
                        break;
                    case EffectViewShowValueFormat.Percent100:
                    case EffectViewShowValueFormat.Percent100NoPlus:
                    case EffectViewShowValueFormat.Percent100Abs:
                        var value2 = keyValuePair.Value;
                        PathOfQuasimorph.raritySystem.ApplyModifier<float>(ref value2, finalModifier, increase, out outOldValue, out outNewValue);
                        valueFinal = value2;
                        break;
                }

                //if (record.ValueFormat.ToString().Contains("int"))
                //{
                //    var value = (int)keyValuePair.Value;
                //    PathOfQuasimorph.raritySystem.ApplyModifier<int>(ref value, finalModifier, increase, out outOldValue, out outNewValue);
                //    valueFinal = value;
                //}
                //else
                //{
                //    var value = keyValuePair.Value;
                //    PathOfQuasimorph.raritySystem.ApplyModifier<float>(ref value, finalModifier, increase, out outOldValue, out outNewValue);
                //    valueFinal = value;
                //}

                itemRecord.ImplicitPenaltyEffects[keyValuePair.Key] = (float)valueFinal;

                _logger.Log($"\t\t old value {outOldValue}");
                _logger.Log($"\t\t new value {outNewValue}");
            }

            foreach (KeyValuePair<string, float> keyValuePair in entriesCoreEffects)
            {
                finalModifier = GetFinalModifier(baseModifier, numToHinder, numToImprove, ref improvedCount, ref hinderedCount, boostedParamString, ref increase, string.Empty, true, _logger);

                float valueFinal = 0;

                WoundEffectRecord record = Data.WoundEffects.GetRecord(keyValuePair.Key, true);

                switch (record.ValueFormat)
                {
                    case EffectViewShowValueFormat.Raw:
                    case EffectViewShowValueFormat.MinusInt:
                    case EffectViewShowValueFormat.MinusDamage:
                    case EffectViewShowValueFormat.ReverseInt:
                        var value = (int)keyValuePair.Value;
                        PathOfQuasimorph.raritySystem.ApplyModifier<int>(ref value, finalModifier, increase, out outOldValue, out outNewValue);
                        valueFinal = value;
                        break;
                    case EffectViewShowValueFormat.Percent100:
                    case EffectViewShowValueFormat.Percent100NoPlus:
                    case EffectViewShowValueFormat.Percent100Abs:
                        var value2 = keyValuePair.Value;
                        PathOfQuasimorph.raritySystem.ApplyModifier<float>(ref value2, finalModifier, increase, out outOldValue, out outNewValue);
                        valueFinal = value2;
                        break;
                }

                //var value = keyValuePair.Value;
                //PathOfQuasimorph.raritySystem.ApplyModifier<float>(ref value, finalModifier, increase, out outOldValue, out outNewValue);

                itemRecord.CoreEffects[keyValuePair.Key] = (float)valueFinal;

                _logger.Log($"\t\t old value {outOldValue}");
                _logger.Log($"\t\t new value {outNewValue}");
            }
        }

        private string CreateBareHandWeapon()
        {
            if (itemRecord.BareHandWeapon == string.Empty)
            {
                _logger.Log($"itemRecord.BareHandWeapon == str empty {itemRecord.BareHandWeapon == string.Empty}. Quitting.");
                return string.Empty;
            }

            _logger.Log($"CreateBareHandWeapon using {itemRecord.BareHandWeapon}");

            // We create new item record
            //recreationCyborg_hand_custom_poq_1337_1289432890000001_nature_cyborgrecreation_fist

            //var newBareHandId = $"{itemId}_{itemRecord.BareHandWeapon}";
            //_logger.Log($"newBareHandId {newBareHandId}");

            if (MetadataWrapper.TryGetFinishTime(itemId, out DateTime finishTime))
            {
                //_logger.Log($"newBareHandId {newBareHandId}");
                _logger.Log($"mobRarityBoost {mobRarityBoost}");
                _logger.Log($"itemRarity {itemRarity}");
                _logger.Log($"finishTime.Ticks.ToString() {finishTime.Ticks.ToString()}");

                // We need just add record as CreatureSystem.SetBareHandSlot creates item for us.

                return PathOfQuasimorph.itemRecordsControllerPoq.InterceptAndReplaceItemId(
                    itemIdOrigin: itemRecord.BareHandWeapon,
                    mobRarityBoost: mobRarityBoost,
                    itemRarity: itemRarity,
                    selectRarity: false,
                    ignoreBlacklist: false,
                    randomUidInjected: finishTime.Ticks.ToString(),
                    applyRarity: false
                    );
            }

            return string.Empty;
        }

    }
}
