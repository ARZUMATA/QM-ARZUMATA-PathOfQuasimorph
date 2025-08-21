using MGSC;
using Newtonsoft.Json;
using QM_PathOfQuasimorph.Controllers;
using QM_PathOfQuasimorph.Core;
using QM_PathOfQuasimorph.Records;
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

        public struct WoundEffectData
        {
            // By default WoundEffectData data is a positive bonus.
            // If sign is inverted it becomes negative and added as negative effect.
            // bool sign helps determining that, if it's null it's a toggle.

            public bool? Sign { get; set; }   // null: flag/toggle
            public float Value { get; set; }
            public int Weight { get; set; }   // selection weight — higher = more common
            public List<Type> ApplicableTypes { get; set; }

            public WoundEffectData(bool? sign, float value, int weight)
            {
                Sign = sign;
                Value = value;
                Weight = weight;
                ApplicableTypes = new List<Type>
                {
                    typeof(ImplantRecord),
                    typeof(AugmentationRecord)
                };
            }
        }

        // Very strong / game-changing effects > low weight (rare)
        // Moderate bonuses > medium weight
        // Minor tweaks or situational perks > higher weight (common)

        public static Dictionary<string, WoundEffectData> woundEffects = new Dictionary<string, WoundEffectData>()
        {
            // -----------------------------------------------------------------------------
            // CORE SURVIVAL & SUSTENANCE (Food, Health, Regen)
            // -----------------------------------------------------------------------------
            { "max_health",                   new WoundEffectData(true,  20f,   30) }, // Maximum Health
            { "passive_regen",                new WoundEffectData(true,  5f,    25) }, // HP/turn, sustained healing
            { "regen_efficacy",               new WoundEffectData(true,  0.2f,  35) }, // Meds HP Regeneration
            { "food_calories",                new WoundEffectData(false, -0.7f, 40) }, // Calorie consumption, reduces food consumption — useful
            { "satiety",                      new WoundEffectData(true,  0.5f,  40) }, // Calorie gain, More calories from food — decent
            { "vomiting",                     new WoundEffectData(false, -0.1f, 50) }, // Reduces puke chance — minor quality of life
            { "hallucinations",               new WoundEffectData(false, -0.3f, 45) }, // Reduces hallucination chance — situational
            { "pain_threshold_regen",         new WoundEffectData(true,  1f,    30) }, // Pain per turn, faster pain threshold recovery

            // -----------------------------------------------------------------------------
            // DAMAGE MODIFIERS (Incoming & Outgoing)
            // -----------------------------------------------------------------------------
            { "income_dmg",                   new WoundEffectData(false, -0.2f, 30) }, // Incoming damage — strong defensive stat
            { "income_pain",                  new WoundEffectData(false, -0.3f, 35) }, // Pain reduction
            { "melee_dmg_reduce",             new WoundEffectData(true,  0.3f,  40) }, // Strike Damage, Increases melee damage
            { "critchance_reduce",            new WoundEffectData(true,  0.05f, 35) }, // Crit chance bonus
            { "crit_damage",                  new WoundEffectData(true,  0.2f,  30) }, // Multiplier on crits
            { "pain_to_melee_dmg",            new WoundEffectData(true,  4f,    20) }, // Pain into Melee Damage
            { "move_dmg",                     new WoundEffectData(false, -1f,   50) }, // Damage on move
            { "dot_dmg",                      new WoundEffectData(false, -1f,   50) }, // Damage per turn
            { "action_dmg",                   new WoundEffectData(false, -1f,   50) }, // Damage per Action
            { "eat_dmg",                      new WoundEffectData(false, -1f,   50) }, // Damage per eating — anti-heal debuff
            { "apoint_dmg",                   new WoundEffectData(false, -1f,   50) }, // Damage per AP

            // -----------------------------------------------------------------------------
            // ACCURACY, COMBAT & WEAPON BEHAVIOR
            // -----------------------------------------------------------------------------
            { "accuracy_reduce",              new WoundEffectData(true,  0.1f,    40) }, // Ranged accuracy
            { "melee_accuracy",               new WoundEffectData(true,  0.05f,   45) }, // Melee Accuracy,
            { "ranged_accuracy",              new WoundEffectData(true,  0.2f,    35) }, // Ranged Accuracy
            { "firearm_range",                new WoundEffectData(true,  1f,      30) }, // Weapon range, extended range
            { "scatter_angle",                new WoundEffectData(false, -0.2f,   40) }, // Scatter, Tighter spread
            { "multi_hit",                    new WoundEffectData(true,  1f,      25) }, // Extra hit
            { "added_projectile",             new WoundEffectData(true,  1f,      25) }, // Extra projectile
            { "reload_duration",              new WoundEffectData(false, -1f,     35) }, // Weapon Reload Duration, faster reload
            { "income_critchance",              new WoundEffectData(false, -0.1f, 35) }, // Incoming crit. chance
            { "dodge_reduce",                   new WoundEffectData(true,  0.15f, 40) }, // Dodge chance
            { "melee_throw_range",              new WoundEffectData(true,  1f,    45) }, // Throw Range
            { "fov_angle",                      new WoundEffectData(true,  0.2f,  50) }, // Field of view

            // -----------------------------------------------------------------------------
            // RESISTANCES (All Damage Types)
            // -----------------------------------------------------------------------------
            { "resist_blunt",                 new WoundEffectData(true,  15f,  25) }, // Blunt resist
            { "resist_pierce",                new WoundEffectData(true,  15f,  25) }, // Pierce resist
            { "resist_lacer",                 new WoundEffectData(true,  15f,  25) }, // Cut resist
            { "resist_fire",                  new WoundEffectData(true,  15f,  25) }, // Fire resist
            { "resist_beam",                  new WoundEffectData(true,  15f,  25) }, // Beam resist
            { "resist_shock",                 new WoundEffectData(true,  15f,  25) }, // Shock resist
            { "resist_poison",                new WoundEffectData(true,  15f,  25) }, // Poison resist
            { "resist_cold",                  new WoundEffectData(true,  15f,  25) }, // Cold resist

            // -----------------------------------------------------------------------------
            // MOVEMENT, STEALTH & DETECTION
            // -----------------------------------------------------------------------------
            { "stealth_ap",                   new WoundEffectData(true,  2f,   15) }, // More AP in stealth
            { "walk_ap",                      new WoundEffectData(true,  3f,   25) }, // More walking AP 
            { "run_ap",                       new WoundEffectData(true,  4f,   35) }, // Run AP
            { "los_reduce",                   new WoundEffectData(true,  1f,   45) }, // Vision range — situational
            { "spotted_radius",               new WoundEffectData(false, -1f,  50) }, // Smaller player detection radius
            { "no_spotted_signal",            new WoundEffectData(null,  0f,   35) }, // No enemy detection (>1 = Enabled - negative stat)
            { "run_spotted_signal",           new WoundEffectData(null,  1f,   35) }, // Detect others when running (>1 = Enabled - positive stat))
            { "walk_spotted_signal",          new WoundEffectData(null,  1f,   40) }, // Detection while walking — (>1 = Enabled - positive stat)

            // -----------------------------------------------------------------------------
            // INVENTORY & UTILITY
            // -----------------------------------------------------------------------------
            { "items_weight",                 new WoundEffectData(false, -0.1f,   45) }, // Weight modifier, lighter items — QoL
            { "backpack_weight",              new WoundEffectData(false, -0.15f,  40) }, //  Load weight, carry more — useful
            { "bonus_vest_slot",              new WoundEffectData(true,  1f,      15) }, // Extra vest slot
            { "perk_exp_modifier",            new WoundEffectData(true,  0.8f,    35) }, // Faster perk XP
            { "perk_cooldown",                new WoundEffectData(false, -0.2f,   30) }, // Shorter cooldowns
            { "implant_cooldown",             new WoundEffectData(false, -8f,     20) }, // Implant Cooldown, major reduction 

            // -----------------------------------------------------------------------------
            // CHANCE-BASED EFFECTS (Wounds, Crits, Addiction)
            // -----------------------------------------------------------------------------
            { "wound_chance",                 new WoundEffectData(false, -0.1f,   40) }, // Reduce self-wounding
            { "wound_heal_chance",            new WoundEffectData(true,  0.1f,    45) }, // Wound healing chance
            { "wound_chance_mult",            new WoundEffectData(false, -0.2f,   35) }, // Getting Wound Chance
            { "added_wound_chance_mult",      new WoundEffectData(true,  0.15f,   30) }, // Inflict Wound Chance
            { "addiction_chance",             new WoundEffectData(false, -0.15f,  45) }, // Addiction Chance, avoid addiction
            { "qmorph_summon",                new WoundEffectData(false, -0.01f,  25) }, // Chance to break through quasimorphs
            { "qmorph",                       new WoundEffectData(false, -1f,     20) }, // Quasimorphosis gain

            // -----------------------------------------------------------------------------
            // PROGRESSION & REWARD SYSTEMS
            // -----------------------------------------------------------------------------
            { "mission_points",                 new WoundEffectData(true,  0.1f,  40) }, // Mission Points

            // -----------------------------------------------------------------------------
            // SPECIAL / TOGGLE EFFECTS (bool? = null > flags)
            // -----------------------------------------------------------------------------
            // These are commented out as they are binary flags, not numeric bonuses
            // But you can uncomment and assign weight if used in random rolls
            
            // { "arm_slot_unavailable",       new WoundEffectData(null,  1f,   5) },  // Weapon slot blocked
            // { "food_unavailable",           new WoundEffectData(null,  1f,   10) }, // Food unavailable
            { "throwback_immune",           new WoundEffectData(null,  1f,   15) }, // Knockback immunity
            { "no_stealth",                 new WoundEffectData(null,  0f,   10) }, // Stealth is unavailable
            // { "frozen_stun",                new WoundEffectData(null,  0f,   5)  }, // Freeze
            // { "shock_stun",                 new WoundEffectData(null,  0f,   5)  }, // Shock
            { "self_heal",                  new WoundEffectData(null,  1f,   10) }, // Self-healing regen toggle — strong
            // { "death",                      new WoundEffectData(null,  0f,   1)  }, // Instant death — extremely rare
            { "run_unavailable",            new WoundEffectData(null,  0f,   10) }, // Running is unavailable          >1 = Enabled
            { "consume_regen",              new WoundEffectData(null,  0f,   15) }, // Regeneration is unavailable     0 = Unavail
            
            // Immunities (very rare / powerful)
            { "wound_immune_blunt",         new WoundEffectData(null,  1f,  8)  },  // Immune to Blunt Wounds
            { "wound_immune_pierce",        new WoundEffectData(null,  1f,  8)  },  // Immune to Pierce Wounds
            { "wound_immune_lacer",         new WoundEffectData(null,  1f,  8)  },  // Immune to Lacer Wounds
            { "wound_immune_fire",          new WoundEffectData(null,  1f,  8)  },  // Immune to Fire Wounds
            { "wound_immune_beam",          new WoundEffectData(null,  1f,  8)  },  // Immune to Beam Wounds
            { "wound_immune_shock",         new WoundEffectData(null,  1f,  8)  },  // Immune to Shock Wounds
            { "wound_immune_poison",        new WoundEffectData(null,  1f,  8)  },  // Immune to Poison Wounds
            { "wound_immune_cold",          new WoundEffectData(null,  1f,  8)  },  // Immune to Cold Wounds
            
            // Full damage immunities
            { "immune_blunt",               new WoundEffectData(null,  1f,  5)  }, // blunt immunity
            { "immune_pierce",              new WoundEffectData(null,  1f,  5)  }, // pierce immunity
            { "immune_lacer",               new WoundEffectData(null,  1f,  5)  }, // cut immunity
            { "immune_fire",                new WoundEffectData(null,  1f,  5)  }, // fire immunity
            { "immune_beam",                new WoundEffectData(null,  1f,  5)  }, // beam immunity
            { "immune_shock",               new WoundEffectData(null,  1f,  5)  }, // shock immunity
            { "immune_poison",              new WoundEffectData(null,  1f,  5)  }, // poison immunity
            { "immune_cold",                new WoundEffectData(null,  1f,  5)  }, // cold immunity
            
            // Status immunities
            { "status_immune_infectionEffect",    new WoundEffectData(null,  1f,  10) }, // Immune to Infection
            { "status_immune_poisonEffect",       new WoundEffectData(null,  1f,  10) }, // Immune to Poisoning
            { "status_immune_morphineAddiction",  new WoundEffectData(null,  1f,  15) }, // Immune to Drug Addiction
            { "status_immune_alcoholAddiction",   new WoundEffectData(null,  1f,  15) }, // Immune to Alcohol Addiction
            { "status_immune_nicotineAddiction",  new WoundEffectData(null,  1f,  15) }, // Immune to Nicotine Addiction
            { "status_immune_gavaahAddiction",    new WoundEffectData(null,  1f,  15) }, // Immune to Gavaakh Addiction
            { "status_immune_coldEffect",         new WoundEffectData(null,  1f,  12) }, // Immune to Hypothermia
            { "status_immune_beamEffect",         new WoundEffectData(null,  1f,  12) }, // Immune to ARS
            { "status_immune_shockEffect",        new WoundEffectData(null,  1f,  12) }, // Immune to Shock Status
        };


        public List<HashSet<string>> woundEffectsExclusiveGroups = new List<HashSet<string>>
        {
            new HashSet<string>
            {
                "immune_blunt",
                "immune_pierce",
                "immune_lacer",
                "immune_fire",
                "immune_beam",
                "immune_shock",
                "immune_poison",
                "immune_cold"
            },
             new HashSet<string>
            {
                "wound_immune_blunt",
                "wound_immune_pierce",
                "wound_immune_lacer",
                "wound_immune_fire",
                "wound_immune_beam",
                "wound_immune_shock",
                "wound_immune_poison",
                "wound_immune_cold"
            },
            new HashSet<string>
            {
                "status_immune_infectionEffect",
                "status_immune_poisonEffect",
                "status_immune_morphineAddiction",
                "status_immune_alcoholAddiction",
                "status_immune_nicotineAddiction",
                "status_immune_gavaahAddiction",
                "status_immune_coldEffect",
                "status_immune_beamEffect",
                "status_immune_shockEffect"
            },
            new HashSet<string>
            {
                "stealth_ap",
                "walk_ap",
                "run_ap"
            },
            new HashSet<string>
            {
                "no_spotted_signal",
                "run_spotted_signal",
                "walk_spotted_signal"
            },
        };

        public static Dictionary<string, (int positive, int negative)> effectsPerSlot = new Dictionary<string, (int positive, int negative)>()
        {
            // Average number of effects slot for vanilla items i.e. we can increase the number with the rarity.
            { "Arm",      (2, 1) },
            { "Body",     (2, 1) },
            { "Chest",    (3, 3) },
            { "Feet",     (1, 1) },
            { "Head",     (1, 1) },
            { "Knee",     (2, 2) },
            { "Shoulder", (2, 2) },
            { "Stomach",  (2, 1) },
            { "Thigh",    (1, 2) },
        };


        public Dictionary<ItemRarity, (int Min, int Max)> extraEffectsPerRarity = new Dictionary<ItemRarity, (int Min, int Max)>
        {
            // Extra slots we can add but don't exceed total sum of negatives/positives per slot type.
            { ItemRarity.Standard,  (0,  0) },     // 
            { ItemRarity.Enhanced,  (0,  0) },     // 
            { ItemRarity.Advanced,  (1,  3) },     // 
            { ItemRarity.Premium,   (1,  4) },     // 
            { ItemRarity.Prototype, (2,  5) },     // 
            { ItemRarity.Quantum,   (3,  6) },     // 
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

        internal List<string> SelectWeightedWoundEffects(int count, List<string> existingEffects, List<HashSet<string>> exclusiveGroups = null)
        {
            Helpers.ShuffleDictionary(woundEffects);

            var available = new Dictionary<string, int>();
            foreach (var kvp in woundEffects)
            {
                if (existingEffects.Contains(kvp.Key))
                {
                    continue;
                }

                available[kvp.Key] = kvp.Value.Weight;
            }

            var result = new List<string>();

            foreach (var group in exclusiveGroups ?? new List<HashSet<string>>())
            {
                // Remove any key from available if another in group was picked
                if (result.Intersect(group).Any())
                {
                    foreach (var key in group)
                    {
                        available.Remove(key);
                    }
                }
            }

            // Weighted random pick (simple version)
            while (count > 0 && available.Count > 0)
            {
                var totalWeight = available.Values.Sum();
                var roll = Helpers._random.Next(0, totalWeight);
                string selected = null;

                foreach (var kvp in available)
                {
                    roll -= kvp.Value;

                    if (roll < 0)
                    {
                        selected = kvp.Key;
                        break;
                    }
                }

                if (selected == null)
                {
                    break;
                }

                result.Add(selected);
                available.Remove(selected);

                // Enforce exclusivity: remove all from same group
                foreach (var group in exclusiveGroups ?? new List<HashSet<string>>())
                {
                    if (group.Contains(selected))
                    {
                        foreach (var key in group)
                        {
                            available.Remove(key);
                        }
                    }
                }

                count--;
            }

            return result;
        }

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

        private interface IHasSlotType
        {
            string SlotType { get; }
        }

        internal virtual bool AddRandomImplicitEffect(
            SynthraformerRecord record,
            MetadataWrapper metadata,
            IDictionary<string, float> bonusEffects,
            IDictionary<string, float> penaltyEffects)
        {
            _logger.Log($"AddRandomImplicitEffect");

            var positive = Helpers._random.NextDouble() > 0.5f;

            _logger.Log($"\t is positive effect: {positive}");

            var genericCount = 0;

            MetadataWrapper.TryGetBaseId(this.itemId, out string baseId);
            baseId = baseId.Split('_')[0];

            _logger.Log($"\t baseId: {baseId}");

            // Try to get generic record
            var genericRecord = Data.WoundSlots.GetRecord(baseId);
            _logger.Log($"\t genericRecord == null: {genericRecord == null}");

            if (genericRecord != null)
            {
                genericCount = positive
                    ? genericRecord.ImplicitBonusEffects.Count
                    : genericRecord.ImplicitPenaltyEffects.Count;
            }

            // Get extra count based on rarity and slot
            // Get extra count of slots per specified rarity
            // Add positive or negative count for average number of effects slot per vanilla item so we get total slots we can add
            var extraEffectsAvailableCount = Helpers._random.Next(
                extraEffectsPerRarity[itemRarity].Min,
                extraEffectsPerRarity[itemRarity].Max + 1);

            if (itemRecord is IHasSlotType hasSlot)
            {
                extraEffectsAvailableCount += positive
                    ? effectsPerSlot[hasSlot.SlotType].positive
                    : effectsPerSlot[hasSlot.SlotType].negative;
            }

            _logger.Log($"\t genericCount: {genericCount}");
            _logger.Log($"\t extraEffectsAvailableCount: {extraEffectsAvailableCount}");

            // Substract generic countto not overexceed amount of effects
            extraEffectsAvailableCount -= genericCount;

            var removeRandom = Helpers._random.NextDouble() > 0.8f;
            _logger.Log($"\t remove random effect: {removeRandom}");

            // Remove random effect right away so it doesn't interfere
            if (removeRandom)
            {
                var effects = positive ? bonusEffects : penaltyEffects;
                if (effects != null && effects.Count > 0)
                {
                    var keys = effects.Keys.ToArray();
                    var randomKey = keys[Helpers._random.Next(keys.Length)];
                    effects.Remove(randomKey);
                    extraEffectsAvailableCount += 1; // Add count as we have free slot
                }
            }

            _logger.Log($"\t extraEffectsAvailableCount Final: {extraEffectsAvailableCount}");

            if (extraEffectsAvailableCount < 0)
            {
                _logger.LogError($"Can't add effect, above limit. {extraEffectsAvailableCount}");
                extraEffectsAvailableCount = 0;
                //return false;
            }

            // Now chose random effect that will be either positive or negative based on the roll
            var combinedList = new List<string>();
            combinedList.AddRange(bonusEffects.Keys);
            combinedList.AddRange(penaltyEffects.Keys);

            var selectedEffectName = SelectWeightedWoundEffects(1, combinedList, woundEffectsExclusiveGroups).First();
            var selectedEffect = woundEffects[selectedEffectName];

            _logger.Log($"\t selectedEffect: {selectedEffectName}");

            float finalValue = selectedEffect.Value;

            // Now pick random value in [finalValue * 0.5, finalValue * 1.5]
            float minValue = finalValue * 0.5f;
            float maxValue = finalValue * 1.5f;
            _logger.Log($"\t minValue: {minValue}");
            _logger.Log($"\t maxValue: {maxValue}");

            // Handle negative values: ensure min/max are correctly ordered
            float lowerBound = Math.Min(minValue, maxValue);
            float upperBound = Math.Max(minValue, maxValue);

            float randomizedValue = (float)(Helpers._random.NextDouble() * (upperBound - lowerBound) + lowerBound);

            // We got boolean value, override.
            if (selectedEffect.Sign == null)
            {
                if (positive)
                {
                    randomizedValue = selectedEffect.Value;
                }
                else
                {
                    // invert: 0 becomes 1, 1 becomes 0
                    randomizedValue = 1 - selectedEffect.Value;
                }
            }
            else
            {
                randomizedValue = positive ? randomizedValue : -randomizedValue;
                _logger.Log($"\t\t randomizedValue ({(positive ? "bonus" : "penalty")}): {randomizedValue}");
            }

            var targetEffects = positive ? bonusEffects : penaltyEffects;

            if (targetEffects.ContainsKey(selectedEffectName))
            {
                // Effect already exists, let's fail.
                return false;
            }

            // Enforce cap: remove random if at limit
            while (targetEffects.Count >= extraEffectsAvailableCount)
            {
                var keys = targetEffects.Keys.ToArray();
                var randomKey = keys[Helpers._random.Next(keys.Length)];
                targetEffects.Remove(randomKey);

                _logger.Log($"\t targetEffects > extraEffectsAvailableCount {targetEffects.Count} {extraEffectsAvailableCount}");
                _logger.LogWarning($"\t\t REMOVING: {randomKey}");
            }

            targetEffects[selectedEffectName] = randomizedValue;

            return true;
        }
    }
}