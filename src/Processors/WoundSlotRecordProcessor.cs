using MGSC;
using Newtonsoft.Json;
using QM_PathOfQuasimorph.Controllers;
using QM_PathOfQuasimorph.Core;
using QM_PathOfQuasimorph.PoQHelpers;
using QM_PathOfQuasimorph.Records;
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

namespace QM_PathOfQuasimorph.Processors
{
    internal class WoundSlotRecordProcessor<T> : ConfigTableRecordProcessor<T> where T : WoundSlotRecord
    {
        //private new Logger _logger = new Logger(null, typeof(WoundSlotRecordProcessor<T>));
        public WoundSlotRecordProcessor(ItemRecordsControllerPoq itemRecordsControllerPoq) : base(itemRecordsControllerPoq)
        {
        }

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


        internal override void ProcessRecord(ref string boostedParamString)
        {
            if (itemRarity == ItemRarity.Standard)
            {
                return;
            }

            ApplyParameters();

            itemRecord.BareHandWeapon = CreateBareHandWeapon();
            _logger.Log($"itemRecord.BareHandWeapon now {itemRecord.BareHandWeapon}");
        }

        private void ApplyParameters()
        {
            float baseModifier, finalModifier;
            int numToHinder, numToImprove, improvedCount, hinderedCount;
            string boostedParamString;
            bool increase;
            PrepGenericData(out baseModifier, out finalModifier, out numToHinder, out numToImprove, out boostedParamString, out improvedCount, out hinderedCount, out increase);

            // Convert to list to avoid enumeration errors during modification
            var bonusEffects = itemRecord.ImplicitBonusEffects.ToList();
            var penaltyEffects = itemRecord.ImplicitPenaltyEffects.ToList();
            var coreEffects = itemRecord.CoreEffects.ToList();

            ApplyEffectModifiers(itemRecord.ImplicitBonusEffects, bonusEffects, true, boostedParamString, baseModifier, numToHinder, numToImprove, ref improvedCount, ref hinderedCount, ref increase);
            ApplyEffectModifiers(itemRecord.ImplicitPenaltyEffects, penaltyEffects, false, boostedParamString, baseModifier, numToHinder, numToImprove, ref improvedCount, ref hinderedCount, ref increase);
            ApplyEffectModifiers(itemRecord.CoreEffects, coreEffects, true, string.Empty, baseModifier, numToHinder, numToImprove, ref improvedCount, ref hinderedCount, ref increase);
        }

        private void ApplyEffectModifiers(
            Dictionary<string, float> sourceEffects,
            List<KeyValuePair<string, float>> targetEffects,
            bool isBonus,
            string boostedParamString,
            float baseModifier,
            int numToHinder,
            int numToImprove,
            ref int improvedCount,
            ref int hinderedCount,
            ref bool increase)
        {
            float outOldValue, outNewValue;

            foreach (var kvp in targetEffects)
            {
                string key = kvp.Key;
                float value = kvp.Value;

                float finalModifier = GetFinalModifier(
                    baseModifier, numToHinder, numToImprove,
                    ref improvedCount, ref hinderedCount,
                    boostedParamString, ref increase,
                    key, isBonus, _logger);

                float valueFinal = 0;
                WoundEffectRecord record = Data.WoundEffects.GetRecord(key, true);
                outOldValue = value;
                outNewValue = value;

                switch (record.ValueFormat)
                {
                    case EffectViewShowValueFormat.Raw:
                    case EffectViewShowValueFormat.MinusInt:
                    case EffectViewShowValueFormat.MinusDamage:
                    case EffectViewShowValueFormat.ReverseInt:
                        {
                            int intValue = (int)value;
                            PathOfQuasimorph.raritySystem.ApplyModifier(ref intValue, finalModifier, increase, out outOldValue, out outNewValue);
                            valueFinal = intValue;
                            break;
                        }
                    case EffectViewShowValueFormat.Percent100:
                    case EffectViewShowValueFormat.Percent100NoPlus:
                    case EffectViewShowValueFormat.Percent100Abs:
                        {
                            float floatValue = value;
                            PathOfQuasimorph.raritySystem.ApplyModifier(ref floatValue, finalModifier, increase, out outOldValue, out outNewValue);
                            valueFinal = floatValue;
                            break;
                        }
                    default:
                        valueFinal = value;
                        break;
                }

                // Update original dictionary
                sourceEffects[key] = valueFinal;

                _logger.Log($"\t\t old value {outOldValue}");
                _logger.Log($"\t\t new value {outNewValue}");
            }
        }

        private string CreateBareHandWeapon()
        {
            if (itemRecord.BareHandWeapon == string.Empty)
            {
                _logger.Log($"itemRecord.BareHandWeapon is empty.");
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
                    Id: itemRecord.BareHandWeapon,
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

        internal void FillMobContextEffects(CreaturesControllerPoq.MonsterMasteryTier mastery,
            IDictionary<string, float> bonusEffects,
            IDictionary<string, float> penaltyEffects)
        {
            _logger.Log($"FillMobContextEffects");

            var totalEffectsPerSlot = (int)mastery * 2;
            _logger.Log($"totalEffectsPerSlot: {totalEffectsPerSlot} for mastery: {mastery}");

            var addedEffectsPerSlot = 0;

            //while (int i = 0; i < totalEffectsPerSlot; i++)
            while (addedEffectsPerSlot <= totalEffectsPerSlot)
            {
                var success = AddRandomImplicitEffect((ItemRarity)(mastery + 1), bonusEffects, penaltyEffects, false, true);

                if (success)
                {
                    addedEffectsPerSlot++;
                    _logger.Log($"SUCCESS");
                }
            }
        }
    }
}
