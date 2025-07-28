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
    internal class ImplantRecordProcessorPoq : ItemRecordProcessor<ImplantRecord>
    {
        private new Logger _logger = new Logger(null, typeof(ImplantRecordProcessorPoq));

        public override List<string> parameters => _parameters;

        internal List<string> _parameters = new List<string>()
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

            foreach (KeyValuePair<string, float> keyValuePair in itemRecord.ImplicitBonusEffects)
            {
                finalModifier = GetFinalModifier(baseModifier, numToHinder, numToImprove, ref improvedCount, ref hinderedCount, boostedParamString, ref increase, keyValuePair.Key, _logger);

                var value = keyValuePair.Value;
                PathOfQuasimorph.raritySystem.ApplyModifier<float>(ref value, finalModifier, increase, out outOldValue, out outNewValue);
                itemRecord.ImplicitBonusEffects[keyValuePair.Key] = value;

                Plugin.Logger.Log($"\t\t old value {outOldValue}");
                Plugin.Logger.Log($"\t\t new value {outNewValue}");
            }

            foreach (KeyValuePair<string, float> keyValuePair in itemRecord.ImplicitPenaltyEffects)
            {
                finalModifier = GetFinalModifier(baseModifier, numToHinder, numToImprove, ref improvedCount, ref hinderedCount, boostedParamString, ref increase, keyValuePair.Key, _logger);

                var value = keyValuePair.Value;
                PathOfQuasimorph.raritySystem.ApplyModifier<float>(ref value, finalModifier, increase, out outOldValue, out outNewValue);
                itemRecord.ImplicitPenaltyEffects[keyValuePair.Key] = value;

                Plugin.Logger.Log($"\t\t old value {outOldValue}");
                Plugin.Logger.Log($"\t\t new value {outNewValue}");
            }
        }
    }
}
