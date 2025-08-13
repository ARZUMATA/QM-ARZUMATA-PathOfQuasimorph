using MGSC;
using Newtonsoft.Json;
using QM_PathOfQuasimorph.Controllers;
using QM_PathOfQuasimorph.Core;
using QM_PathOfQuasimorph.PoQHelpers;
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

namespace QM_PathOfQuasimorph.Processors
{
    internal class ImplantRecordProcessorPoq : ItemRecordProcessor<ImplantRecord>
    {
        private new Logger _logger = new Logger(null, typeof(ImplantRecordProcessorPoq));

        public override Dictionary<string, bool> parameters => _parameters;

        internal Dictionary<string, bool> _parameters = new Dictionary<string, bool>()
        {
        };

        internal List<string> implicitBonusEffects = new List<string>()
        {
            "backpack_weight",
            "crit_damage",
            "critchance_reduce",
            "food_calories",
            "implant_cooldown",
            "income_pain",
            "max_health",
            "melee_dmg_reduce",
            "melee_throw_range",
            "pain_to_melee_dmg",
            "passive_regen",
            "perk_cooldown",
            "perk_exp_modifier",
            "qmorph",
            "ranged_accuracy",
            "reload_duration",
            "resist_beam",
            "resist_blunt",
            "resist_fire",
            "resist_lacer",
            "resist_pierce",
            "resist_poison",
            "resist_shock",
            "satiety",
            "status_immune_shockEffect",
            "wound_chance_mult",
            "wound_immune_lacer",
            "wound_immune_pierce",
        };

        internal List<string> implicitPenaltyEffects = new List<string>()
        {
            "addiction_chance",
            "food_calories",
            "income_pain",
            "max_health",
            "ranged_accuracy",
            "regen_efficacy",
            "resist_beam",
            "resist_blunt",
            "resist_fire",
            "resist_lacer",
            "resist_pierce",
            "resist_shock",
            "satiety",
            "scatter_angle",
            "wound_chance",
            "wound_chance_mult",
        };

        public ImplantRecordProcessorPoq(ItemRecordsControllerPoq itemRecordsControllerPoq) : base(itemRecordsControllerPoq)
        {
        }

        internal override void ProcessRecord(ref string boostedParamString)
        {
            //if (itemRarity == ItemRarity.Standard)
            //{
            //    return;
            //}

            // We got perk records now
            //if (itemRecord.IsActive == true)
            //{
            //    return;
            //}

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

            var entriesImplicitBonusEffects = itemRecord.ImplicitBonusEffects.ToList();
            var entriesImplicitPenaltyEffects = itemRecord.ImplicitPenaltyEffects.ToList();

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

                itemRecord.ImplicitBonusEffects[keyValuePair.Key] = (float)valueFinal;

                //var value = keyValuePair.Value;
                //PathOfQuasimorph.raritySystem.ApplyModifier<float>(ref value, finalModifier, increase, out outOldValue, out outNewValue);
                //itemRecord.ImplicitBonusEffects[keyValuePair.Key] = value;

                Plugin.Logger.Log($"\t\t old value {outOldValue}");
                Plugin.Logger.Log($"\t\t new value {outNewValue}");
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

                itemRecord.ImplicitPenaltyEffects[keyValuePair.Key] = (float)valueFinal;

                //var value = keyValuePair.Value;
                //PathOfQuasimorph.raritySystem.ApplyModifier<float>(ref value, finalModifier, increase, out outOldValue, out outNewValue);
                //itemRecord.ImplicitPenaltyEffects[keyValuePair.Key] = value;

                Plugin.Logger.Log($"\t\t old value {outOldValue}");
                Plugin.Logger.Log($"\t\t new value {outNewValue}");
            }



            if (itemRecord.IsActive == true)
            {
                // Add new perk copying same perk under new id (yeah that's how game works)
                Plugin.Logger.Log($"\t\t perkRecord oldId {oldId}");
                Plugin.Logger.Log($"\t\t perkRecord itemId {itemId}");

                PerkRecord perkRecord = Data.Perks.GetRecord(oldId, true);

                Plugin.Logger.Log($"\t\t perkRecord null {perkRecord == null}");

                PerkRecord newPerkRecord = ItemRecordHelpers.ClonePerkRecord(perkRecord, itemId);

                foreach (var perkParameter in perkRecord.Parameters)
                {
                    switch (perkParameter.ValType)
                    {
                        case PerkParameter.ValueType.Int:
                            PathOfQuasimorph.raritySystem.ApplyModifier<int>(ref perkParameter.IntVal, finalModifier, increase, out outOldValue, out outNewValue);
                            break;
                        case PerkParameter.ValueType.Float:
                            PathOfQuasimorph.raritySystem.ApplyModifier<float>(ref perkParameter.FloatVal, finalModifier, increase, out outOldValue, out outNewValue);
                            break;
                    }
                }

                Data.Perks.AddRecord(itemId, newPerkRecord);
                RecordCollection.PerkRecords.Add(itemId, newPerkRecord);
                MetadataWrapper.TryGetBaseId(itemId, out string baseId);
                Plugin.Logger.Log($"perkRecord baseId: {baseId}");
                Plugin.Logger.Log($"perkRecord itemId: {itemId}");

                Localization.DuplicateKey("perk." + baseId + ".desc", "perk." + itemId + ".desc");
                RaritySystem.AddAffixes(itemId);
            }


        }
    }
}
