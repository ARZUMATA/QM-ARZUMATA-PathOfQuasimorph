using JetBrains.Annotations;
using MGSC;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using UnityEngine;
using static QM_PathOfQuasimorph.Core.CreaturesControllerPoq;
using Random = System.Random;

namespace QM_PathOfQuasimorph.Core
{
    /* This class is for controlling created creatures that have extra */
    internal partial class CreaturesControllerPoq
    {
        private readonly Random _random = new Random();

        private static bool initColors = false;

        // Number of times to increaseAlphaColor/decrease brightness before reversing direction.
        private static int currentStepIndex = 0;

        public static int alphaColorStepsCount = 35; // Total number of steps to transition from 0 to 1 or vice versa
        public static bool increaseAlphaColor = true; // Whether to increaseAlphaColor or decrease the alpha value

        // How many times per second the color changes (1 / 35 = in this case, 35 times per sec).
        public static float stepDuration = 1f / alphaColorStepsCount; // Time (in seconds) per step

        /* List of our extra data for creatures that either filled during game load or creature 
         * and save to kinda neutral field serialized and base64 encoded
         */
        internal Dictionary<int, CreatureDataPoq> creatureDataPoq = new Dictionary<int, CreatureDataPoq>();

        public enum MonsterMasteryTier
        {
            None,
            Novice,
            Skilled,
            Expert,
            Grandmaster
        }

        public static readonly Dictionary<MonsterMasteryTier, string> MonsterMasteryColors = new Dictionary<MonsterMasteryTier, string>()
        {
            { MonsterMasteryTier.None,    "#FFFFFF" },     // Blue - Basic / Easier monsters
            { MonsterMasteryTier.Novice,    "#8888FF" },     // Blue - Basic / Easier monsters
            { MonsterMasteryTier.Skilled,   "#FFFF77" },     // Gold - More complex monsters
            { MonsterMasteryTier.Expert,    "#800080" },     // Purple - High threat / mastery challenge
            { MonsterMasteryTier.Grandmaster, "#FF0000" },   // Red - Elite / Legendary monsters
        };

        private List<string> statsToModify = new List<string>
        {
             "Health",
            "Dodge",
            "ActionPoints",
            "MeleeAccuracy",
            "RangeAccuracy",
            "LosLevel",
            "MeleeDamage_MinMax",
            "MeleeDamage_CritChance",
            "MeleeDamage_CritDmg",
            "MeleeThrowbackChance",

            //  "PainThresholdLimit",
            //  "PainThresholdRegen",
             //"QuazimorphosisReward",
            //  "ReceiveAmputationChance",
            //  "ReceiveWoundChanceMult",
            //  "AttackWoundChanceMult",
        };

        private Dictionary<MonsterMasteryTier, int> _masteryTierWeights = new Dictionary<MonsterMasteryTier, int>
        {
            { MonsterMasteryTier.None,    1000 },     // Common folk
            { MonsterMasteryTier.Novice,    500 },     // Easier, common monsters
            { MonsterMasteryTier.Skilled,   200 },      // Moderately challenging
            { MonsterMasteryTier.Expert,    50 },      // High threat, requires skill
            { MonsterMasteryTier.Grandmaster, 5 }      // Very rare and difficult
        };

        private Dictionary<MonsterMasteryTier, (float Min, float Max)> _masteryModifiers = new Dictionary<MonsterMasteryTier, (float Min, float Max)>
        {
            { MonsterMasteryTier.None,     ( 1.0f,   1.0f  ) },  // No change for None
            { MonsterMasteryTier.Novice,   ( 1.15f,  1.25f ) },  // Novice = Basic / Easy to handle
            { MonsterMasteryTier.Skilled,  ( 1.3f,   1.4f ) },  // Skilled = Advanced / Slightly more complex
            { MonsterMasteryTier.Expert,   ( 1.5f,   1.6f ) },  // Expert = Master / High threat and skill required
            { MonsterMasteryTier.Grandmaster, ( 2.0f,   2.5f ) },  // Grandmaster = Legendary
        };

        private List<string> resistsToModify = new List<string> { "blunt", "pierce", "lacer", "fire", "cold", "poison", "shock", "beam" };

        private List<string> talentsList = new List<string>
        {
            "talent_the_man_who_sold_the_world",
            "talent_weapon_durability",
            "talent_weapon_distance",
            "talent_pistol_acc",
            "talent_all_resists",
            "talent_ignore_pain",
            "talent_reload_time",
            "talent_weight_dodge_affect",
        };

        private List<string> perksList = new List<string>
        {
            "military_training",
            "cqc_specialist",
            "bodybuilding",
            "hardening",
            "cold_weapon_wielding",
            "athletics",
            "reaction_training",
            "heavy_weaponary",
            "grenadier",
            "tomahawk",
            "lizard",
            "immune_response",
            "pyromaniac",
            "demolisher",
            "handmade_shotgun_ammo",
            "emergency_revival",
            "piersing_burst",
            "axe_rage",
            "weak_point",
            "enhanced_heavy_ammo",
            "arsonist",
            "reinforced_battery",
        };

        internal List<string> perksMasteries = new List<string>{
           "_basic",
           "_advanced",
           "_master",
           "_legend",
        };

        private Dictionary<MonsterMasteryTier, (string Min, string Max)> perkRanksRange = new Dictionary<MonsterMasteryTier, (string Min, string Max)>
        {
            { MonsterMasteryTier.Novice,      ( "rank_1",   "rank_2"  ) },
            { MonsterMasteryTier.Skilled,     ( "rank_2",   "rank_3"  ) },
            { MonsterMasteryTier.Expert,      ( "rank_3",   "rank_4"  ) },
            { MonsterMasteryTier.Grandmaster, ( "rank_4",   "rank_5"  ) },
        };

        public class CreatureDataPoq
        {
            public MonsterMasteryTier rarity;
            public Dictionary<string, float> statsPanelOriginal = new Dictionary<string, float>();
            public Dictionary<string, float> statsPanelDiff = new Dictionary<string, float>();
            public Dictionary<string, float> statsPanelNew = new Dictionary<string, float>();

            // public Color color;
            private static readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings();

            public string SerializeData()
            {
                var _dataString = Convert.ToBase64String(
                        Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(this, _jsonSettings)));
                return _dataString;
            }

            public static CreatureDataPoq DeserializeData(string _dataString)
            {
                try
                {
                    var base64 = _dataString.Trim();
                    if (base64.Length % 4 != 0)
                    {
                        return null;
                    }

                    try
                    {
                        var jsonBytes = Convert.FromBase64String(base64);
                        var deserializedData = JsonConvert.DeserializeObject<CreatureDataPoq>(
                            Encoding.UTF8.GetString(jsonBytes), _jsonSettings);
                        return deserializedData;
                    }
                    catch (FormatException)
                    {
                        // Invalid Base64 string
                        return null;
                    }
                }
                catch (Exception)
                {
                    // Any other decoding or deserialization error
                    return null;
                }
            }
        }

        public static float GetAlphaOverTime()
        {
            float currentTime = Time.time;

            // Total duration of one full cycle
            float alphaCycleDuration = stepDuration * alphaColorStepsCount;

            // PingPong the time to make it go back and forth between 0 and alphaCycleDuration
            float t = Mathf.PingPong(currentTime, alphaCycleDuration) / alphaCycleDuration;

            // Normalize t to be between 0 and 1 for alpha
            return t;
        }

        public static void HighlightMobsPoq(CellPosition mapCell, ObjHighlightController objHighlightController)
        {
            foreach (Creature creature in objHighlightController._creatures.Monsters)
            {
                if (!initColors && PathOfQuasimorph.pixelizatorCameraAttachment != null)
                {
                    PathOfQuasimorph.pixelizatorCameraAttachment.SetOutlinesColor(10 + (int)ItemRarity.Enhanced, Helpers.HexStringToUnityColor(RaritySystem.Colors[ItemRarity.Enhanced]));
                    PathOfQuasimorph.pixelizatorCameraAttachment.SetOutlinesColor(10 + (int)ItemRarity.Advanced, Helpers.HexStringToUnityColor(RaritySystem.Colors[ItemRarity.Advanced]));
                    PathOfQuasimorph.pixelizatorCameraAttachment.SetOutlinesColor(10 + (int)ItemRarity.Premium, Helpers.HexStringToUnityColor(RaritySystem.Colors[ItemRarity.Premium]));
                    PathOfQuasimorph.pixelizatorCameraAttachment.SetOutlinesColor(10 + (int)ItemRarity.Prototype, Helpers.HexStringToUnityColor(RaritySystem.Colors[ItemRarity.Prototype]));
                    PathOfQuasimorph.pixelizatorCameraAttachment.SetOutlinesColor(10 + (int)ItemRarity.Quantum, Helpers.HexStringToUnityColor(RaritySystem.Colors[ItemRarity.Quantum]));
                    initColors = true;
                }

                // Basic
                // pixelizatorCameraAttachment.SetOutlinesColor(4, _BlueOutlineColor);
                // 
                // 
                // foreach (Material material in creature.Creature3dView._materials)
                // {
                //     material.SetFloat(Creature3dView.PIXELIZER_ID_PROPERTY, 11);
                //     //Console.WriteLine($"material {material.name}");
                //     //Console.WriteLine($"material {material.shader.name}");
                // }

                if (PathOfQuasimorph.creaturesControllerPoq.creatureDataPoq.TryGetValue(creature.CreatureData.UniqueId, out CreatureDataPoq creatureDataPoq))
                {
                    // No highlight on common folk
                    if (creatureDataPoq.rarity == MonsterMasteryTier.None)
                    {
                        continue;
                    }

                    var color = Helpers.HexStringToUnityColor(MonsterMasteryColors[creatureDataPoq.rarity]);

                    if (increaseAlphaColor)
                    {
                        // Adjust this factor to control the amount of increaseAlphaColor
                        color.a = Mathf.Clamp(GetAlphaOverTime(), 0, 1);
                        PathOfQuasimorph.pixelizatorCameraAttachment.SetOutlinesColor((byte)(10 + (int)creatureDataPoq.rarity), color);

                        // Apply the brighter color to the material.
                        foreach (Material material in creature.Creature3dView._materials)
                        {
                            material.SetFloat(Creature3dView.PIXELIZER_ID_PROPERTY, 10 + (int)creatureDataPoq.rarity);
                        }

                        currentStepIndex++;

                        if (currentStepIndex >= alphaColorStepsCount)
                        {
                            // Reached maximum brightness, start decreasing
                            increaseAlphaColor = false;
                        }
                    }
                    else
                    {
                        // Adjust this factor to control the amount of decrease
                        color.a = Mathf.Clamp(GetAlphaOverTime(), 0, 1);
                        PathOfQuasimorph.pixelizatorCameraAttachment.SetOutlinesColor((byte)(10 + (int)creatureDataPoq.rarity), color);

                        // Apply the dimmer color to the material.
                        foreach (Material material in creature.Creature3dView._materials)
                        {
                            material.SetFloat(Creature3dView.PIXELIZER_ID_PROPERTY, 10 + (int)creatureDataPoq.rarity);
                        }

                        currentStepIndex--;


                        if (currentStepIndex <= -alphaColorStepsCount)
                        {
                            // Reached minimum brightness, start increasing again
                            increaseAlphaColor = true;
                        }
                    }

                }
            }
        }

        public static void Postfix(CellPosition cellUnderCursor, ObjHighlightController __instance)
        {
        }

        internal void ApplyStatsFromRarity(ref Monster monster, MonsterMasteryTier rarity)
        {
            Plugin.Logger.Log($"ApplyStatsFromRarity");

            // Skip for common folk
            if (rarity == MonsterMasteryTier.None)
            {
                Plugin.Logger.Log($"\t MonsterMasteryTier.None: Skip");

                return;
            }

            // Define baseModifier used to boost stats
            float baseModifier = PathOfQuasimorph.raritySystem.GetRarityModifier(rarity, _masteryModifiers);

            // Store original values
            CreatureDataPoq creatureData = null;

            if (creatureDataPoq.ContainsKey(monster.CreatureData.UniqueId))
            {
                creatureData = creatureDataPoq[monster.CreatureData.UniqueId];
            }

            if (creatureData != null)
            {
                var _original_basicRangeAccuracy = monster.CreatureData.GetRangeAccuracyNorm(null, false, false, false);
                var _original_visionDistance = monster.CreatureData.GetLosLevel();
                var _original_weaponsDamage = monster.CreatureData.GetTotalPerkRangeDamageBonus();
                var _original_hitChance = monster.CreatureData.GetMeleeAccuracyNorm(null, false, false);
                var _original_handsDamageMin = (float)Math.Round(monster.CreatureData.MeleeDamage.minDmg * monster.CreatureData.OverallMeleeDamageMult(null, false), 0);
                var _original_handsDamageMax = (float)Math.Round(monster.CreatureData.MeleeDamage.maxDmg * monster.CreatureData.OverallMeleeDamageMult(null, false), 0);
                var _original_meleeBoost = monster.CreatureData.GetTotalPerkMeleeDamageBonus();
                var _original_meleeCritChance = monster.CreatureData.GetFinalMeleeCritChance();
                var _original_dodgeChance = monster.CreatureData.GetDodge();

                creatureData.statsPanelOriginal.Add("_health", monster.CreatureData.Health.MaxValue);
                creatureData.statsPanelOriginal.Add("_basicRangeAccuracy", _original_basicRangeAccuracy);
                creatureData.statsPanelOriginal.Add("_visionDistance", _original_visionDistance);
                creatureData.statsPanelOriginal.Add("_weaponsDamage", _original_weaponsDamage);
                creatureData.statsPanelOriginal.Add("_hitChance", _original_hitChance);
                creatureData.statsPanelOriginal.Add("_handsDamageMin", _original_handsDamageMin);
                creatureData.statsPanelOriginal.Add("_handsDamageMax", _original_handsDamageMax);
                creatureData.statsPanelOriginal.Add("_meleeBoost", _original_meleeBoost);
                creatureData.statsPanelOriginal.Add("_meleeCritChance", _original_meleeCritChance);
                creatureData.statsPanelOriginal.Add("_dodgeChance", _original_dodgeChance);
            }

            // Perks
            ApplyPerks(monster, rarity);

            // Resists
            ApplyResists(monster, baseModifier);

            // Stats
            ApplyStats(monster, baseModifier);

            // Save new stats
            if (creatureData != null)
            {
                var _newdata_basicRangeAccuracy = monster.CreatureData.GetRangeAccuracyNorm(null, false, false, false);
                var _newdata_visionDistance = monster.CreatureData.GetLosLevel();
                var _newdata_weaponsDamage = monster.CreatureData.GetTotalPerkRangeDamageBonus();
                var _newdata_hitChance = monster.CreatureData.GetMeleeAccuracyNorm(null, false, false);
                var _newdata_handsDamageMin = (float)Math.Round(monster.CreatureData.MeleeDamage.minDmg * monster.CreatureData.OverallMeleeDamageMult(null, false), 0);
                var _newdata_handsDamageMax = (float)Math.Round(monster.CreatureData.MeleeDamage.maxDmg * monster.CreatureData.OverallMeleeDamageMult(null, false), 0);
                var _newdata_meleeBoost = monster.CreatureData.GetTotalPerkMeleeDamageBonus();
                var _newdata_meleeCritChance = monster.CreatureData.GetFinalMeleeCritChance();
                var _newdata_dodgeChance = monster.CreatureData.GetDodge();

                creatureData.statsPanelNew.Add("_health", monster.CreatureData.Health.MaxValue);
                creatureData.statsPanelNew.Add("_basicRangeAccuracy", _newdata_basicRangeAccuracy);
                creatureData.statsPanelNew.Add("_visionDistance", _newdata_visionDistance);
                creatureData.statsPanelNew.Add("_weaponsDamage", _newdata_weaponsDamage);
                creatureData.statsPanelNew.Add("_hitChance", _newdata_hitChance);
                creatureData.statsPanelNew.Add("_handsDamageMin", _newdata_handsDamageMin);
                creatureData.statsPanelNew.Add("_handsDamageMax", _newdata_handsDamageMax);
                creatureData.statsPanelNew.Add("_meleeBoost", _newdata_meleeBoost);
                creatureData.statsPanelNew.Add("_meleeCritChance", _newdata_meleeCritChance);
                creatureData.statsPanelNew.Add("_dodgeChance", _newdata_dodgeChance);

                // Assign the difference manually
                creatureData.statsPanelDiff["_health"] = creatureData.statsPanelNew["_health"] - creatureData.statsPanelOriginal["_health"];
                creatureData.statsPanelDiff["_basicRangeAccuracy"] = creatureData.statsPanelNew["_basicRangeAccuracy"] - creatureData.statsPanelOriginal["_basicRangeAccuracy"];
                creatureData.statsPanelDiff["_visionDistance"] = creatureData.statsPanelNew["_visionDistance"] - creatureData.statsPanelOriginal["_visionDistance"];
                creatureData.statsPanelDiff["_weaponsDamage"] = creatureData.statsPanelNew["_weaponsDamage"] - creatureData.statsPanelOriginal["_weaponsDamage"];
                creatureData.statsPanelDiff["_hitChance"] = creatureData.statsPanelNew["_hitChance"] - creatureData.statsPanelOriginal["_hitChance"];
                creatureData.statsPanelDiff["_handsDamageMin"] = creatureData.statsPanelNew["_handsDamageMin"] - creatureData.statsPanelOriginal["_handsDamageMin"];
                creatureData.statsPanelDiff["_handsDamageMax"] = creatureData.statsPanelNew["_handsDamageMax"] - creatureData.statsPanelOriginal["_handsDamageMax"];
                creatureData.statsPanelDiff["_meleeBoost"] = creatureData.statsPanelNew["_meleeBoost"] - creatureData.statsPanelOriginal["_meleeBoost"];
                creatureData.statsPanelDiff["_meleeCritChance"] = creatureData.statsPanelNew["_meleeCritChance"] - creatureData.statsPanelOriginal["_meleeCritChance"];
                creatureData.statsPanelDiff["_dodgeChance"] = creatureData.statsPanelNew["_dodgeChance"] - creatureData.statsPanelOriginal["_dodgeChance"];
            }

            // Save new stats to mob
            monster.CreatureData.UltimateSkullItemId = creatureData.SerializeData();
        }

        private void ApplyPerks(Monster monster, MonsterMasteryTier rarity)
        {
            monster.CreatureData.Perks = new List<Perk>();
            int numToAdjust = 6; // Six perks always, we base on their masteries.

            // Always shuffle list for better randomness
            PathOfQuasimorph.raritySystem.ShuffleList(perksList);

            Dictionary<string, Perk> editable_perks = new Dictionary<string, Perk>();

            // Define mastery
            var perkSuffix = perksMasteries[(int)rarity - 1];

            // Add talent perk
            string talentSelected = talentsList[_random.Next(0, talentsList.Count)];
            monster.CreatureData.Perks.Add(PathOfQuasimorph.perkFactoryState.CreatePerk(Data.Perks.GetRecord(talentSelected)));

            // Add rank perk
            var (Min, Max) = perkRanksRange[rarity];
            string selectedWord = _random.Next(0, 100 + 1) < 50 ? Min : Max;
            monster.CreatureData.Perks.Add(PathOfQuasimorph.perkFactoryState.CreatePerk(Data.Perks.GetRecord(selectedWord)));

            // Add ultimate perk
            //monster.CreatureData.Perks.Add(null); // For ordung!
            //monster.CreatureData.Perks.Add(PathOfQuasimorph.perkFactoryState.CreatePerk(Data.Perks.GetRecord("ultimate_ray")));

            // Select random perks
            for (int i = 0; i < numToAdjust; i++)
            {
                var perkRecord = Data.Perks.GetRecord($"{perksList[i]}{perkSuffix}");

                if (perkRecord != null)
                {
                    monster.CreatureData.Perks.Add(PathOfQuasimorph.perkFactoryState.CreatePerk(perkRecord));
                    Plugin.Logger.Log($"Added perk {perksList[i]}{perkSuffix}");
                }
                else
                {
                    Plugin.Logger.Log($"{perksList[i]}{perkSuffix} null");
                }
            }
        }

        private void ApplyResists(Monster monster, float baseModifier)
        {
            int improvedCount = 0;
            int hinderedCount = 0;
            bool hinder = false;
            float finalModifier = 0;

            int numToAdjust = resistsToModify.Count;
            int numToHinder = (int)Math.Floor(numToAdjust * PathOfQuasimorph.raritySystem.PARAMETER_HINDER_PERCENT / 100f); // 20% of adjusted parameters to hinder
            int numToImprove = numToAdjust - numToHinder;
            PathOfQuasimorph.raritySystem.ShuffleList(resistsToModify);
            int boostedParam = _random.Next(resistsToModify.Count);

            var resistSheet = monster.CreatureData.ResistSheet._currentResist;
            float averageResist = 0;
            int resistCount = 0;
            bool averageResistApplied = false;

            Plugin.Logger.Log($"Getting average resist");

            foreach (var resistType in resistsToModify)
            {
                averageResist += resistSheet[resistType];
                resistCount++;
            }

            averageResist = (float)Math.Round(averageResist / resistCount, 2);
            averageResist = Math.Max(averageResist, 1.0f); // Ensure average resist is at least 1.0

            // Apply the base baseModifier to all resists in the list
            foreach (var resistType in resistsToModify)
            {
                Plugin.Logger.Log($"Processing resistType: {resistType}");

                if (resistSheet.TryGetValue(resistType, out float value))
                {
                    // Hinder or not
                    hinder = PathOfQuasimorph.raritySystem.ShouldHinderParameter(ref hinderedCount, ref improvedCount, numToHinder, numToImprove);

                    // Apply boost
                    if (resistType == resistsToModify.ElementAt(boostedParam))
                    {
                        finalModifier = baseModifier * (float)Math.Round(_random.NextDouble() * (RaritySystem.PARAMETER_BOOST_MAX - RaritySystem.PARAMETER_BOOST_MIN) + RaritySystem.PARAMETER_BOOST_MIN, 2);
                        Plugin.Logger.Log($"\t\t boosting final modifier from {baseModifier} to {finalModifier} : TRUE");
                    }
                    else
                    {
                        finalModifier = baseModifier;
                        Plugin.Logger.Log($"\t\t boosting final modifier from {baseModifier} to {finalModifier} : FALSE");
                    }

                    Plugin.Logger.Log($"\t\t finalModifier: {finalModifier}");
                    
                    Plugin.Logger.Log($"Updating {resistType} with {value} and finalModifier {finalModifier}, hinder: {hinder}");

                    // If resist zero
                    if (!averageResistApplied)
                    {
                        Plugin.Logger.Log($"Applying average resist {resistType} with {averageResist}, ONCE");

                        resistSheet[resistType] = averageResist;
                        averageResistApplied = true;
                    }

                    // Apply
                    if (hinder)
                    {
                        resistSheet[resistType] = value / finalModifier;
                    }
                    else
                    {
                        resistSheet[resistType] = value * finalModifier;
                    }

                    Plugin.Logger.Log($"Updated {resistType} to {resistSheet[resistType]}");
                }
            }
        }

        private void ApplyStats(Monster monster, float baseModifier)
        {
            int improvedCount = 0;
            int hinderedCount = 0;
            float finalModifier = 0;

            int numToAdjust = statsToModify.Count;
            int numToHinder = (int)Math.Floor(numToAdjust * PathOfQuasimorph.raritySystem.PARAMETER_HINDER_PERCENT / 100f); // 20% of adjusted parameters to hinder
            int numToImprove = numToAdjust - numToHinder;
            PathOfQuasimorph.raritySystem.ShuffleList(statsToModify);
            var boostedParam = _random.Next(statsToModify.Count);

            // Reflection based approach went good but sadly switch case is better in this scenario.

            foreach (var prop in statsToModify)
            {
                Plugin.Logger.Log($"Updating {prop}");

                // Hinder or not
                bool hinder = PathOfQuasimorph.raritySystem.ShouldHinderParameter(ref hinderedCount, ref improvedCount, numToHinder, numToImprove);

                // Apply boost
                if (prop == statsToModify.ElementAt(boostedParam))
                {
                    finalModifier = baseModifier * (float)Math.Round(_random.NextDouble() * (RaritySystem.PARAMETER_BOOST_MAX - RaritySystem.PARAMETER_BOOST_MIN) + RaritySystem.PARAMETER_BOOST_MIN, 2);
                    Plugin.Logger.Log($"\t\t boosting final modifier from {baseModifier} to {finalModifier}");

                }
                else
                {
                    finalModifier = baseModifier;
                    //Plugin.Logger.Log($"\t\t boosting final modifier from {baseModifier} to {finalModifier} : FALSE");
                }

                Plugin.Logger.Log($"\t\t finalModifier: {finalModifier} hinder: {hinder} boosted: {finalModifier != baseModifier}");

                float outOldValue = -1;
                float outNewValue = -1;

                switch (prop)
                {

                    case "Health":
                        ApplyModifier<int>(ref monster.CreatureData.BaseHealth, finalModifier, hinder, out outOldValue, out outNewValue);
                        outOldValue = monster.CreatureData.Health.MaxValue;

                        // Get health bonus from perks
                        Plugin.Logger.Log($"\t\t maxHealthBonus: {monster.CreatureData.GetMaxHealthBonus()}");
                        AugmentationSystem.UpdateMaxHealth(monster.CreatureData);
                        Plugin.Logger.Log($"\t\t intermediate value: {monster.CreatureData.Health.MaxValue}");

                        outNewValue = monster.CreatureData.Health.MaxValue;
                        break;

                    case "Dodge":
                        ApplyModifier<float>(ref monster.CreatureData.BaseDodge, finalModifier, hinder, out outOldValue, out outNewValue);
                        break;

                    case "ActionPoints":
                        ApplyModifier<int>(ref monster.CreatureData.BaseActionPoints, finalModifier, hinder, out outOldValue, out outNewValue);
                        break;

                    case "MeleeAccuracy":
                        ApplyModifier<float>(ref monster.CreatureData.BaseMeleeAccuracy, finalModifier, hinder, out outOldValue, out outNewValue);
                        break;

                    case "RangeAccuracy":
                        ApplyModifier<float>(ref monster.CreatureData.BaseRangeAccuracy, finalModifier, hinder, out outOldValue, out outNewValue);
                        break;

                    case "LosLevel":
                        ApplyModifier<int>(ref monster.CreatureData.BaseLosLevel, finalModifier, hinder, out outOldValue, out outNewValue);
                        break;

                    case "MeleeDamage_MinMax":
                        ApplyModifier<int>(ref monster.CreatureData.MeleeDamage.minDmg, finalModifier, hinder, out outOldValue, out outNewValue);
                        ApplyModifier<int>(ref monster.CreatureData.MeleeDamage.maxDmg, finalModifier, hinder, out outOldValue, out outNewValue);
                        break;

                    case "MeleeDamage_CritChance":
                        ApplyModifier<float>(ref monster.CreatureData.MeleeDamage.critChance, finalModifier, hinder, out outOldValue, out outNewValue);
                        break;

                    case "MeleeDamage_CritDmg":
                        ApplyModifier<float>(ref monster.CreatureData.MeleeDamage.critDmg, finalModifier, hinder, out outOldValue, out outNewValue);
                        break;

                    case "MeleeThrowbackChance":
                        ApplyModifier<float>(ref monster.CreatureData.MeleeThrowbackChance, finalModifier, hinder, out outOldValue, out outNewValue);
                        break;
                }

                Plugin.Logger.Log($"\t\t old value {outOldValue}");
                Plugin.Logger.Log($"\t\t new value {outNewValue}");

            }
        }

        internal void CleanCreatureDataPoq()
        {
            creatureDataPoq.Clear();
        }

        internal MonsterMasteryTier SelectRarity()
        {
            return PathOfQuasimorph.raritySystem.SelectRarityWeighted(_masteryTierWeights);
        }
        
        
        public static void ApplyModifier<T>(ref T value, float finalModifier, bool hinder, out float outOldValue, out float outNewValue) where T : struct
        {
            outOldValue = (float)Convert.ChangeType(value,typeof(float));

            float tempValue = outOldValue;
            tempValue = hinder ? tempValue / finalModifier : tempValue * finalModifier;

            if (typeof(T) == typeof(int))
            {
                tempValue = (float)Math.Ceiling(tempValue);
            }

            if (value is int intValue)
            {
                value = (T)Convert.ChangeType((int)tempValue, typeof(T));
            }
            else if (value is float)
            {
                value = (T)Convert.ChangeType(tempValue, typeof(T));
            }
            else
            {
                Plugin.Logger.Log($"Unsupported type: {typeof(T)}, {nameof(value)}");
            }

            outNewValue = tempValue;
        }
    }
}