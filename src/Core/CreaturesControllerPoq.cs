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

        private Dictionary<string, bool> propertiesToModifyPositive = new Dictionary<string, bool>
        {
            //{ "BaseHealth",    true },
            { "Health.MaxValue",    true },
            { "Health.Value",   true },
            { "BaseDodge",  true },
            { "BaseActionPoints",   true },
            { "BaseMeleeAccuracy",  true },
            { "BaseRangeAccuracy",  true },
            { "BaseLosLevel",   true },
            { "MeleeDamage.minDmg", true },
            { "MeleeDamage.maxDmg", true },
            { "MeleeDamage.critChance", true },
            { "MeleeDamage.critDmg",    true },
            { "MeleeThrowbackChance",   true },
            // { "PainThresholdLimit",   true },
            // { "PainThresholdRegen",   true },
            { "QuazimorphosisReward",   true },
            // { "ReceiveAmputationChance",  true },
            // { "ReceiveWoundChanceMult",   true },
            // { "AttackWoundChanceMult",    true },
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

        public class CreatureDataPoq
        {
            public int id;
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
                var jsonBytes = Convert.FromBase64String(_dataString);
                var deserializedData = JsonConvert.DeserializeObject<CreatureDataPoq>(
                    Encoding.UTF8.GetString(jsonBytes), _jsonSettings);
                return deserializedData;
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
            // Skip for common folk
            if (rarity == MonsterMasteryTier.None)
            {
                return;
            }

            // Apply the base baseModifier to all properties in the list
            float baseModifier = PathOfQuasimorph.raritySystem.GetRarityModifier(rarity, _masteryModifiers);
            float finalModifier = PathOfQuasimorph.raritySystem.GetRarityModifier(rarity, _masteryModifiers);

            // List of resist types to modify

            // monster.CreatureData.Health.MaxValue = (int)Math.Round(baseModifier * monster.CreatureData.Health.MaxValue, 0);
            // monster.CreatureData.Health.Value = (int)Math.Round(baseModifier * monster.CreatureData.Health.MaxValue, 0);
            // monster.CreatureData.BaseDodge = baseModifier * monster.CreatureData.BaseDodge;
            // monster.CreatureData.BaseActionPoints = (int)Math.Round(baseModifier * monster.CreatureData.BaseActionPoints, 0);
            // monster.CreatureData.BaseMeleeAccuracy = baseModifier * monster.CreatureData.BaseMeleeAccuracy;
            // monster.CreatureData.BaseRangeAccuracy = baseModifier * monster.CreatureData.BaseRangeAccuracy;
            // monster.CreatureData.BaseLosLevel = (int)Math.Round(baseModifier * monster.CreatureData.BaseLosLevel, 0);
            // monster.CreatureData.ResistSheet._currentResist["blunt"] = baseModifier * monster.CreatureData.ResistSheet._currentResist["blunt"];
            // monster.CreatureData.ResistSheet._currentResist["pierce"] = baseModifier * monster.CreatureData.ResistSheet._currentResist["pierce"];
            // monster.CreatureData.ResistSheet._currentResist["lacer"] = baseModifier * monster.CreatureData.ResistSheet._currentResist["lacer"];
            // monster.CreatureData.ResistSheet._currentResist["fire"] = baseModifier * monster.CreatureData.ResistSheet._currentResist["fire"];
            // monster.CreatureData.ResistSheet._currentResist["cold"] = baseModifier * monster.CreatureData.ResistSheet._currentResist["cold"];
            // monster.CreatureData.ResistSheet._currentResist["poison"] = baseModifier * monster.CreatureData.ResistSheet._currentResist["poison"];
            // monster.CreatureData.ResistSheet._currentResist["shock"] = baseModifier * monster.CreatureData.ResistSheet._currentResist["shock"];
            // monster.CreatureData.ResistSheet._currentResist["beam"] = baseModifier * monster.CreatureData.ResistSheet._currentResist["beam"];
            // //creatureData.Perks FILL PERKS

            // // DmgInfo
            // monster.CreatureData.MeleeDamage.minDmg = (int)Math.Round(baseModifier * monster.CreatureData.MeleeDamage.minDmg, 0);
            // monster.CreatureData.MeleeDamage.maxDmg = (int)Math.Round(baseModifier * monster.CreatureData.MeleeDamage.maxDmg, 0);
            // monster.CreatureData.MeleeDamage.critChance = baseModifier * monster.CreatureData.MeleeDamage.critChance;
            // monster.CreatureData.MeleeDamage.critDmg = baseModifier * monster.CreatureData.MeleeDamage.critDmg;

            // monster.CreatureData.MeleeThrowbackChance = baseModifier * monster.CreatureData.MeleeThrowbackChance;
            // monster.CreatureData.PainThresholdLimit = (int)Math.Round(baseModifier * monster.CreatureData.PainThresholdLimit, 0);
            // monster.CreatureData.PainThresholdRegen = (int)Math.Round(baseModifier * monster.CreatureData.PainThresholdRegen, 0);
            // monster.CreatureData.ReceiveAmputationChance = baseModifier * monster.CreatureData.ReceiveAmputationChance;
            // monster.CreatureData.QuazimorphosisReward = (int)Math.Round(baseModifier * monster.CreatureData.QuazimorphosisReward, 0);

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

            // Basic vars
            // Mob properties
            bool hinder = false;
            int improvedCount = 0;
            int hinderedCount = 0;

            int numParamsToAdjust = propertiesToModifyPositive.Count;
            int numParamsToHinder = (int)Math.Floor(numParamsToAdjust * PathOfQuasimorph.raritySystem.PARAMETER_HINDER_PERCENT / 100f); // 20% of adjusted parameters to hinder
            int numParamsToImprove = numParamsToAdjust - numParamsToHinder;
            PathOfQuasimorph.raritySystem.ShuffleDictionary(propertiesToModifyPositive);
            var boostedParam = _random.Next(propertiesToModifyPositive.Count);

            var creatureDataType = monster.CreatureData.GetType();
            foreach (var propPath in propertiesToModifyPositive)
            {
                Plugin.Logger.Log($"Processing property path: {propPath.Key}");

                var props = propPath.Key.Split('.');

                Plugin.Logger.Log($"\t props: {props.Length}");

                object current = monster.CreatureData;
                object finalTarget = null;
                string finalPropOrFieldName = props.Last();

                Plugin.Logger.Log($"\t finalPropOrFieldName: {finalPropOrFieldName}");

                // Traverse the nested objects
                for (int i = 0; i < props.Length; i++)
                {
                    string propertyName = props[i];

                    Plugin.Logger.Log($"\t\t propertyName: {propertyName}");

                    // Try to get either a property or a field
                    PropertyInfo propInfo = current.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
                    FieldInfo fieldInfo = current.GetType().GetField(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);

                    if (propInfo != null)
                    {
                        Plugin.Logger.Log($"\t\t\t propInfo: {propInfo}, {propInfo.Name}");
                        // Our last prop
                        if (propInfo.Name == finalPropOrFieldName)
                        {
                            finalTarget = current;
                            break;
                        }
                        else
                        {
                            current = propInfo.GetValue(current);
                        }
                    }
                    else if (fieldInfo != null)
                    {
                        Plugin.Logger.Log($"\t\t\t fieldInfo: {fieldInfo}, {fieldInfo.Name}");

                        // Our last field
                        // Our last prop
                        if (fieldInfo.Name == finalPropOrFieldName)
                        {
                            finalTarget = current;
                            break;
                        }
                        else
                        {
                            current = fieldInfo.GetValue(current);
                        }
                    }
                    else
                    {
                        Plugin.Logger.Log($"Property/Field '{propertyName}' not found on type: {current.GetType()}");
                        break;
                    }

                    if (current == null)
                    {
                        Plugin.Logger.Log($"Value of {propertyName} is null.");
                        break;
                    }

                    Plugin.Logger.Log($"\t\t\t\t current: {propertyName}");
                }

                if (finalTarget == null)
                {
                    Plugin.Logger.Log($"No access to the final target of path: {propPath.Key}");
                    continue;
                }

                // Try to get the final property or field
                PropertyInfo finalProp = finalTarget.GetType().GetProperty(finalPropOrFieldName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
                FieldInfo finalField = finalTarget.GetType().GetField(finalPropOrFieldName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
                Plugin.Logger.Log($"\t\t propInfo: {finalProp}");
                Plugin.Logger.Log($"\t\t fieldInfo: {finalField}");
                object finalValue = null;

                if (finalProp != null)
                {
                    finalValue = finalProp.GetValue(finalTarget);
                }
                else if (finalField != null)
                {
                    finalValue = finalField.GetValue(finalTarget);
                }
                else
                {
                    Plugin.Logger.Log($"Final property or field '{finalPropOrFieldName}' not found in the object of type: {finalTarget.GetType()}");
                    continue;
                }

                // Apply boost
                if (propPath.Key == propertiesToModifyPositive.ElementAt(boostedParam).Key)
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

                // Hinder or not
                hinder = PathOfQuasimorph.raritySystem.ShouldHinderParameter(ref hinderedCount, ref improvedCount, numParamsToHinder, numParamsToImprove);

                // Apply baseModifier
                if (finalValue is int intValue)
                {
                    if (finalProp != null)
                    {
                        Plugin.Logger.Log($"Updating {propPath.Key} with {intValue} and finalModifier {finalModifier}, hinder: {hinder}");

                        if (hinder)
                        {
                            finalProp.SetValue(finalTarget, (int)Math.Ceiling(intValue / finalModifier));
                        }
                        else
                        {
                            finalProp.SetValue(finalTarget, (int)Math.Ceiling(intValue * finalModifier));
                        }

                        Plugin.Logger.Log($"Updated {propPath.Key} to {(int)finalProp.GetValue(finalTarget)}");
                    }
                    else if (finalField != null)
                    {
                        Plugin.Logger.Log($"Updating {propPath.Key} with {intValue} and finalModifier {finalModifier}, hinder: {hinder}");

                        if (hinder)
                        {
                            finalField.SetValue(finalTarget, (int)Math.Ceiling(intValue / finalModifier));
                        }
                        else
                        {
                            finalField.SetValue(finalTarget, (int)Math.Ceiling(intValue * finalModifier));
                        }

                        Plugin.Logger.Log($"Updated {propPath.Key} to {(int)finalField.GetValue(finalTarget)}");
                    }

                }
                else if (finalValue is float floatValue)
                {
                    if (finalProp != null)
                    {
                        if (hinder)
                        {
                            finalProp.SetValue(finalTarget, floatValue / finalModifier);
                        }
                        else
                        {
                            finalProp.SetValue(finalTarget, floatValue * finalModifier);
                        }

                        Plugin.Logger.Log($"Updated {propPath.Key} to {(float)finalProp.GetValue(finalTarget)}");
                    }
                    else if (finalField != null)
                    {
                        if (hinder)
                        {
                            finalField.SetValue(finalTarget, floatValue / finalModifier);
                        }
                        else
                        {
                            finalField.SetValue(finalTarget, floatValue * finalModifier);
                        }
                        Plugin.Logger.Log($"Updated {propPath.Key} to {(float)finalField.GetValue(finalTarget)}");
                    }
                }
                else
                {
                    Plugin.Logger.Log($"Unsupported type for property '{propPath.Key}': {finalValue?.GetType()}");
                }
            }

            // Resists

            improvedCount = 0;
            hinderedCount = 0;
            numParamsToAdjust = resistsToModify.Count;
            numParamsToHinder = (int)Math.Floor(numParamsToAdjust * PathOfQuasimorph.raritySystem.PARAMETER_HINDER_PERCENT / 100f); // 20% of adjusted parameters to hinder
            numParamsToImprove = numParamsToAdjust - numParamsToHinder;
            PathOfQuasimorph.raritySystem.ShuffleList(resistsToModify);
            boostedParam = _random.Next(resistsToModify.Count);

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

                    // Hinder or not
                    hinder = PathOfQuasimorph.raritySystem.ShouldHinderParameter(ref hinderedCount, ref improvedCount, numParamsToHinder, numParamsToImprove);

                    Plugin.Logger.Log($"Updating {resistType} with {value} and finalModifier {finalModifier}, hinder: {hinder}");

                    // If resist zero
                    if (!averageResistApplied)
                    {
                        Plugin.Logger.Log($"Applying average resist {resistType} with {averageResist}, ONCE");

                        resistSheet[resistType] = averageResist;
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

            // Perks
            monster.CreatureData.Perks = new List<Perk>();
            numParamsToAdjust = 6; // Six perks always, we base on their masteries.

            // Always shuffle list for better randomness
            PathOfQuasimorph.raritySystem.ShuffleList(perksList);

            Dictionary<string, Perk> editable_perks = new Dictionary<string, Perk>();

            // Define mastery
            var perkSuffix = perksMasteries[(int)rarity - 1];

            // Select random perks
            for (int i = 0; i < numParamsToAdjust; i++)
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

            // Save new stasts

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

        internal void CleanCreatureDataPoq()
        {
            creatureDataPoq.Clear();
        }

        internal MonsterMasteryTier SelectRarity()
        {
            return PathOfQuasimorph.raritySystem.SelectRarityWeighted(_masteryTierWeights);
        }
    }
}