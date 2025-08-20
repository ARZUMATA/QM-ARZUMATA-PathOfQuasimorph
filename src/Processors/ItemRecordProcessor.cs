using MGSC;
using Newtonsoft.Json;
using QM_PathOfQuasimorph.Controllers;
using QM_PathOfQuasimorph.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static MGSC.SpawnSystem;
using static MGSC.TurnDebugLogger;
using static QM_PathOfQuasimorph.Contexts.PathOfQuasimorph;
using static QM_PathOfQuasimorph.Core.PathOfQuasimorph;
using Random = System.Random;

namespace QM_PathOfQuasimorph.Processors
{
    internal abstract class ItemRecordProcessor<T>
    {
        protected T itemRecord;
        protected ItemRecordsControllerPoq itemRecordsControllerPoq;
        protected Logger _logger = new Logger(null, typeof(ItemRecordProcessor<T>));

        public abstract Dictionary<string, bool> parameters { get; }
        protected ItemRarity itemRarity;
        protected bool mobRarityBoost;
        protected bool amplifierRarityBoost;
        protected string itemId;
        protected string oldId;

        // Wounds effect cumulative dict for bonus effects
        // bool flag determines sign, we may not need it but we have it

        // Info need update: TODO:
        // For penalty we have to invert
        // move_dmg is false because lowering gives less damage per move so we must lower it
        // melee_dmg_reduce - is stike damage, higher values is bonus
        // This is not required for bonus and penalty effects as we already know what to do
        // But if we need to add randoms, we need to know where to put them

        public static Dictionary<string, (bool? sign, float value)> woundEffects = new Dictionary<string, (bool? sign, float value)>()
        {
            { "los_reduce",                                 (true,      1f          )   },  // vision range
            { "move_dmg",                                   (false,     -1f         )   },  // Damage per Move
            { "dot_dmg",                                    (false,     -1f         )   },  // Damage per Turn
            { "action_dmg",                                 (false,     -1f         )   },  // Damage per Action
            { "eat_dmg",                                    (false,     -1f         )   },  // Damage per Meal
            { "melee_dmg_reduce",                           (true ,     0.3f        )   },  // Strike Damage
            { "income_dmg",                                 (false,     -0.2f       )   },  // incoming damage
            { "income_critchance",                          (false,     -0.1f       )   },  // Incoming crit. chance
            { "accuracy_reduce",                            (true ,     0.1f        )   },  // accuracy
            { "food_calories",                              (false,     -0.7f       )   },  // Calorie consumption
            { "satiety",                                    (true ,     0.5f        )   },  // Calorie gain
            { "vomiting",                                   (false,     -0.1f       )   },  // puke chance
            //{ "no_stealth",                                 (null ,     1f          )   },  // Stealth is unavailable          >1 = Enabled
            //{ "frozen_stun",                                (null ,     1f          )   },  // Freeze                          >1 = Enabled
            //{ "shock_stun",                                 (null ,     1f          )   },  // Shock                           >1 = Enabled
            //{ "self_heal",                                  (null ,     0f          )   },  // Self-healing                    >1 = Enabled
            //{ "death",                                      (null ,     1f          )   },  // Death                           >1 = Enabled
            //{ "run_unavailable",                            (null ,     1f          )   },  // Running is unavailable          >1 = Enabled
            //{ "consume_regen",                              (null ,     1f          )   },  // Regeneration is unavailable     0 = Unavail
            { "hallucinations",                             (false,     -0.3f       )   },  // Hallucination Chance
            { "max_health",                                 (true ,     20f         )   },  // Maximum Health
            { "pain_threshold_regen",                       (true ,     1f          )   },  // Pain per turn
            { "qmorph",                                     (false,     -1f         )   },  // Quasimorphosis
            { "qmorph_summon",                              (false,     -0.01f      )   },  // Chance to break through quasimorphs
            { "income_pain",                                (false,     -0.3f       )   },  // Incoming pain
            //{ "arm_slot_unavailable",                       (null ,     1f          )   },  // Weapon slot blocked              0 = Unavail
            { "critchance_reduce",                          (true ,     0.05f       )   },  // Crit. Chance
            //{ "food_unavailable",                           (null ,     1f          )   },  // Food unavailable                 0 = Unavail
            { "dodge_reduce",                               (true ,     0.15f       )   },  // dodge
            { "apoint_dmg",                                 (false,     -1f         )   },  // Damage per AP
            { "wound_chance",                               (false,     -0.1f       )   },  // Wounding Chance
            { "scatter_angle",                              (false,     -0.2f       )   },  // Scatter
            { "melee_accuracy",                             (true ,     0.05f       )   },  // Melee Accuracy
            { "ranged_accuracy",                            (true ,     0.2f        )   },  // Ranged Accuracy
            //{ "throwback_immune",                           (null ,     1f          )   },  // No knockback                        >1 = Enabled
            { "firearm_range",                              (true ,     1f          )   },  // Weapon range
            { "multi_hit",                                  (true ,     1f          )   },  // Extra hit
            { "added_projectile",                           (true ,     1f          )   },  // Extra projectile
            { "fov_angle",                                  (true ,     0.2f        )   },  // Field of view
            { "spotted_radius",                             (false,     -1f         )   },  // Detection radius                    // bigger better
            { "no_spotted_signal",                          (true ,     1f          )   },  // No enemy detection                  >1 = Enabled
            { "run_spotted_signal",                         (true ,     1f          )   },  // Detect enemies (run)                >1 = Enabled
            { "walk_spotted_signal",                        (true ,     1f          )   },  // Detect enemies (norm.)              >1 = Enabled
            { "items_weight",                               (false,     -0.1f       )   },  // Weight modifier
            { "backpack_weight",                            (false,     -0.15f      )   },  // Load weight mod.
            { "wound_heal_chance",                          (true ,     0.1f        )   },  // Wound healing chance
            { "bonus_vest_slot",                            (true ,     1f          )   },  // Add. vest slot
            { "regen_efficacy",                             (true ,     0.2f        )   },  // Meds HP Regeneration
            { "stealth_ap",                                 (true ,     2f          )   },  // Stealth mode AP
            { "walk_ap",                                    (true ,     3f          )   },  // Normal mode AP
            { "run_ap",                                     (true ,     4f          )   },  // Running mode AP
            { "perk_exp_modifier",                          (true ,     0.8f        )   },  // Perk XP Mod
            { "perk_cooldown",                              (false,     -0.2f       )   },  // Perk Cooldown
            { "crit_damage",                                (true ,     0.2f        )   },  // Crit. Damage
            { "mission_points",                             (true ,     0.1f        )   },  // Mission Points
            { "addiction_chance",                           (false,     -0.15f      )   },  // Addiction Chance
            { "melee_throw_range",                          (true ,     1f          )   },  // Throw Range
            { "passive_regen",                              (true ,     5f          )   },  // HP/turn
            { "pain_to_melee_dmg",                          (true ,     4f          )   },  // Pain into Melee Damage
            { "added_wound_chance_mult",                    (true ,     0.15f       )   },  // Inflict Wound Chance
            { "wound_chance_mult",                          (false,     -0.2f       )   },  // Getting Wound Chance
            { "reload_duration",                            (false,     -1f         )   },  // Weapon Reload Duration
            { "implant_cooldown",                           (false,     -8f         )   },  // Implant Cooldown
            //{ "wound_immune_blunt",                         (null ,     1f          )   },  // Immune to Blunt Wounds                       >1 = Enabled
            //{ "wound_immune_pierce",                        (null ,     1f          )   },  // Immune to Pierce Wounds                      >1 = Enabled
            //{ "wound_immune_lacer",                         (null ,     1f          )   },  // Immune to Lacer Wounds                       >1 = Enabled
            //{ "wound_immune_fire",                          (null ,     1f          )   },  // Immune to Fire Wounds                        >1 = Enabled
            //{ "wound_immune_beam",                          (null ,     1f          )   },  // Immune to Beam Wounds                        >1 = Enabled
            //{ "wound_immune_shock",                         (null ,     1f          )   },  // Immune to Shock Wounds                       >1 = Enabled
            //{ "wound_immune_poison",                        (null ,     1f          )   },  // Immune to Poison Wounds                      >1 = Enabled
            //{ "wound_immune_cold",                          (null ,     1f          )   },  // Immune to Cold Wounds                        >1 = Enabled
            { "resist_blunt",                               (true ,     15f         )   },  // Blunt resist
            { "resist_pierce",                              (true ,     15f         )   },  // Pierce resist
            { "resist_lacer",                               (true ,     15f         )   },  // Cut resist
            { "resist_fire",                                (true ,     15f         )   },  // Fire resist
            { "resist_beam",                                (true ,     15f         )   },  // Beam resist
            { "resist_shock",                               (true ,     15f         )   },  // Shock resist
            { "resist_poison",                              (true ,     15f         )   },  // Poison resist
            { "resist_cold",                                (true ,     15f         )   },  // Cold resist
            //{ "immune_blunt",                               (null ,     1f          )   },  // blunt immunity                               >1 = Enabled
            //{ "immune_pierce",                              (null ,     1f          )   },  // pierce immunity                              >1 = Enabled
            //{ "immune_lacer",                               (null ,     1f          )   },  // cut immunity                                 >1 = Enabled
            //{ "immune_fire",                                (null ,     1f          )   },  // fire immunity                                >1 = Enabled
            //{ "immune_beam",                                (null ,     1f          )   },  // beam immunity                                >1 = Enabled
            //{ "immune_shock",                               (null ,     1f          )   },  // shock immunity                               >1 = Enabled
            //{ "immune_poison",                              (null ,     1f          )   },  // poison immunity                              >1 = Enabled
            //{ "immune_cold",                                (null ,     1f          )   },  // cold immunity                                >1 = Enabled
            //{ "status_immune_infectionEffect",              (null ,     1f          )   },  // Immune to Infection                          >1 = Enabled
            //{ "status_immune_poisonEffect",                 (null ,     1f          )   },  // Immune to Poisoning                          >1 = Enabled
            //{ "status_immune_morphineAddiction",            (null ,     1f          )   },  // Immune to Drug Addiction                     >1 = Enabled
            //{ "status_immune_alcoholAddiction",             (null ,     1f          )   },  // Immune to Alcohol Addiction                  >1 = Enabled
            //{ "status_immune_nicotineAddiction",            (null ,     1f          )   },  // Immune to Nicotine Addiction                 >1 = Enabled
            //{ "status_immune_gavaahAddiction",              (null ,     1f          )   },  // Immune to Gavaakh Addiction                  >1 = Enabled
            //{ "status_immune_coldEffect",                   (null ,     1f          )   },  // Immune to Hypothermia                        >1 = Enabled
            //{ "status_immune_beamEffect",                   (null ,     1f          )   },  // Immune to ARS                                >1 = Enabled
            //{ "status_immune_shockEffect",                  (null ,     1f          )   },  // Immune to Shock Status                       >1 = Enabled
        };

        internal ItemRecordProcessor(ItemRecordsControllerPoq itemRecordsControllerPoq)
        {
            this.itemRecordsControllerPoq = itemRecordsControllerPoq;
        }

        internal virtual void Init(T itemRecord, ItemRarity itemRarity, bool mobRarityBoost, bool amplifierRarityBoost, string itemId, string oldId)
        {
            this.itemRecord = itemRecord;
            this.itemRarity = itemRarity;
            this.mobRarityBoost = mobRarityBoost;
            this.amplifierRarityBoost = amplifierRarityBoost;
            this.itemId = itemId;
            this.oldId = oldId;
        }

        internal abstract void ProcessRecord(ref string boostedParamString);

        internal float GetFinalModifier(float baseModifier, int numToHinder, int numToImprove, ref int improvedCount, ref int hinderedCount, string boostedParamString, ref bool increase, string statStr, bool statBool, Logger _logger)
        {
            float finalModifier;

            if (statBool == false)
            {
                increase = false;
            }
            else if (statBool == true)
            {
                increase = true;
            }

            _logger.Log($"Updating {statStr}");

            // Apply boost
            if (statStr != string.Empty && statStr == boostedParamString)
            {
                finalModifier = baseModifier * (float)Math.Round(Helpers._random.NextDouble() * (RaritySystem.PARAMETER_BOOST_MAX - RaritySystem.PARAMETER_BOOST_MIN) + RaritySystem.PARAMETER_BOOST_MIN, 2);

                _logger.Log($"\t\t boostedParamString exist, boosting final modifier from {baseModifier} to {finalModifier}");
            }
            else
            {
                finalModifier = baseModifier;
            }

            // Determine if we should hinder this parameter
            bool hinder = PathOfQuasimorph.raritySystem.ShouldHinderParameter(ref hinderedCount, ref improvedCount, numToHinder, numToImprove);

            if (hinder)
            {
                increase = !increase;
            }

            _logger.Log($"\t\t finalModifier: {finalModifier} hinder: {hinder}, boosted: {finalModifier != baseModifier}");
            return finalModifier;
        }

        internal void PrepGenericData(out float baseModifier, out float finalModifier, out int numToHinder, out int numToImprove, out string boostedParamString, out int improvedCount, out int hinderedCount, out bool increase)
        {
            baseModifier = PathOfQuasimorph.raritySystem.GetRarityModifier(itemRarity, PathOfQuasimorph.raritySystem._rarityModifiers);

            if (mobRarityBoost)
            {
                float mobModifier = baseModifier * PathOfQuasimorph.raritySystem.GetRarityModifier(MobContext.Rarity, PathOfQuasimorph.creaturesControllerPoq._masteryModifiers);
                _logger.Log($"\t\t mobRarityBoost exist, MobContext Rarity: {MobContext.Rarity}, CurrentMobId: {MobContext.CurrentMobId}");
                _logger.Log($"\t\t boosting final modifier from {baseModifier} to {mobModifier}");

                baseModifier = mobModifier;
            }

            if (amplifierRarityBoost)
            {
                float ampModifier = baseModifier * PathOfQuasimorph.raritySystem.GetRarityModifier(itemRarity, PathOfQuasimorph.raritySystem._rarityModifiers);
                _logger.Log($"\t\t amplifierRarityBoost exist, Rarity: {itemRarity}");
                _logger.Log($"\t\t boosting final modifier from {baseModifier} to {ampModifier}");

                baseModifier = ampModifier;
            }

            finalModifier = 0;
            var (Min, Max) = PathOfQuasimorph.raritySystem.rarityParamPercentages[itemRarity];

            int minParams = Math.Max(0, (int)Math.Floor(Min * parameters.Count));
            int maxParams = (int)Math.Ceiling(Max * parameters.Count);

            // Calculate the number of parameters to adjust based on the percentage
            int numToAdjust = Helpers._random.Next(minParams, maxParams + 1);

            numToHinder = (int)Math.Floor(numToAdjust * PathOfQuasimorph.raritySystem.PARAMETER_HINDER_PERCENT / 100f);
            numToImprove = numToAdjust - numToHinder;

            // Shuffle the list
            Helpers.ShuffleDictionary(parameters);

            // Select one parameter to boost more.
            // This parameter will be boosted more than the others.
            // We return index of parameter that was boosted for UID
            var boostedParam = parameters.Count == 0 ? 99 : Helpers._random.Next(parameters.Count);

            _logger.Log($"\t\t boostedParam: {boostedParam}, parameters.Count: {parameters.Count}");

            boostedParamString = boostedParam == 99 ? string.Empty : parameters.Keys.ToList()[boostedParam];

            // Counters to track how many parameters we've improved or hindered
            improvedCount = 0;
            hinderedCount = 0;

            // Determine if we need increase or decrease
            increase = true;
        }

        //protected abstract List<DmgResist> GetResistSheet();
        //protected abstract float GetResist(string resistName);
        //protected abstract void SetResist(string resistName, float value);
        //protected abstract float GetWeight();
        //protected abstract void SetWeight(float value);
        //protected abstract int GetMaxDurability();
        //protected abstract void SetMaxDurability(int value);

        // internal List<string> SelectWeightedTraits(Dictionary<string, int> traitWeights, int count
        internal List<string> SelectWeightedTraits(Dictionary<string, int> traitWeights, int count, List<string> itemTraitsExisting, List<HashSet<string>> exclusiveGroups = null)
        {
            var availableTraits = traitWeights
                .Where(t => t.Value > 0) // Skip traits with 0 or negative weight
                .ToDictionary(t => t.Key, t => t.Value);

            // Normalize exclusiveGroups to avoid null checks
            var groups = exclusiveGroups;

            if (groups == null)
            {
                groups = new List<HashSet<string>>();
            }

            var selected = new List<string>();

            // for (int i = 0; i < count && availableTraits.Count > 0; i++)
            // {
            //     string selectedTrait = PathOfQuasimorph.raritySystem.SelectRarityWeighted<string>(availableTraits);
            //     selected.Add(selectedTrait);

            //     // Remove already selected trait to prevent duplicates
            //     availableTraits = availableTraits
            //         .Where(t => t.Key != selectedTrait)
            //         .ToDictionary(t => t.Key, t => t.Value);
            // }


            while (selected.Count < count && availableTraits.Count > 0)
            {
                // Select one trait using weighted randomness
                string selectedTrait = PathOfQuasimorph.raritySystem.SelectRarityWeighted<string>(availableTraits);

                // Remove the selected trait from pool
                availableTraits.Remove(selectedTrait);

                // Remove all conflicting traits from the same exclusive group
                for (int i = 0; i < groups.Count; i++)
                {
                    var group = groups[i];
                    if (group.Contains(selectedTrait))
                    {
                        // Remove all members of this group from available traits
                        foreach (var conflict in group)
                        {
                            availableTraits.Remove(conflict);
                        }
                        break;
                    }
                }

                // Only add the trait if it's not already in itemTraitsExisting
                if (!itemTraitsExisting.Contains(selectedTrait))
                {
                    selected.Add(selectedTrait);
                }

                // Even if it exists, we still remove it and it's group members as we don't need them no more.

            }

            return selected;
        }

    }
}