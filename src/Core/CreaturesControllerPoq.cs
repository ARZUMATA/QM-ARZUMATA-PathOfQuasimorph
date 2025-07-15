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
using System.Xml.Linq;
using UnityEngine;
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
            public ItemRarity rarity;
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
                    if (creatureDataPoq.rarity == ItemRarity.Standard)
                    {
                        continue;
                    }

                    var color = Helpers.HexStringToUnityColor(RaritySystem.Colors[creatureDataPoq.rarity]);

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

        internal void ApplyStatsFromRarity(ref Monster result, ItemRarity rarity)
        {
            // Apply the base baseModifier to all properties in the list
            float baseModifier = PathOfQuasimorph.raritySystem.GetRarityModifier(rarity);
            float finalModifier = PathOfQuasimorph.raritySystem.GetRarityModifier(rarity);

            // List of resist types to modify

            // result.CreatureData.Health.MaxValue = (int)Math.Round(baseModifier * result.CreatureData.Health.MaxValue, 0);
            // result.CreatureData.Health.Value = (int)Math.Round(baseModifier * result.CreatureData.Health.MaxValue, 0);
            // result.CreatureData.BaseDodge = baseModifier * result.CreatureData.BaseDodge;
            // result.CreatureData.BaseActionPoints = (int)Math.Round(baseModifier * result.CreatureData.BaseActionPoints, 0);
            // result.CreatureData.BaseMeleeAccuracy = baseModifier * result.CreatureData.BaseMeleeAccuracy;
            // result.CreatureData.BaseRangeAccuracy = baseModifier * result.CreatureData.BaseRangeAccuracy;
            // result.CreatureData.BaseLosLevel = (int)Math.Round(baseModifier * result.CreatureData.BaseLosLevel, 0);
            // result.CreatureData.ResistSheet._currentResist["blunt"] = baseModifier * result.CreatureData.ResistSheet._currentResist["blunt"];
            // result.CreatureData.ResistSheet._currentResist["pierce"] = baseModifier * result.CreatureData.ResistSheet._currentResist["pierce"];
            // result.CreatureData.ResistSheet._currentResist["lacer"] = baseModifier * result.CreatureData.ResistSheet._currentResist["lacer"];
            // result.CreatureData.ResistSheet._currentResist["fire"] = baseModifier * result.CreatureData.ResistSheet._currentResist["fire"];
            // result.CreatureData.ResistSheet._currentResist["cold"] = baseModifier * result.CreatureData.ResistSheet._currentResist["cold"];
            // result.CreatureData.ResistSheet._currentResist["poison"] = baseModifier * result.CreatureData.ResistSheet._currentResist["poison"];
            // result.CreatureData.ResistSheet._currentResist["shock"] = baseModifier * result.CreatureData.ResistSheet._currentResist["shock"];
            // result.CreatureData.ResistSheet._currentResist["beam"] = baseModifier * result.CreatureData.ResistSheet._currentResist["beam"];
            // //creatureData.Perks FILL PERKS

            // // DmgInfo
            // result.CreatureData.MeleeDamage.minDmg = (int)Math.Round(baseModifier * result.CreatureData.MeleeDamage.minDmg, 0);
            // result.CreatureData.MeleeDamage.maxDmg = (int)Math.Round(baseModifier * result.CreatureData.MeleeDamage.maxDmg, 0);
            // result.CreatureData.MeleeDamage.critChance = baseModifier * result.CreatureData.MeleeDamage.critChance;
            // result.CreatureData.MeleeDamage.critDmg = baseModifier * result.CreatureData.MeleeDamage.critDmg;

            // result.CreatureData.MeleeThrowbackChance = baseModifier * result.CreatureData.MeleeThrowbackChance;
            // result.CreatureData.PainThresholdLimit = (int)Math.Round(baseModifier * result.CreatureData.PainThresholdLimit, 0);
            // result.CreatureData.PainThresholdRegen = (int)Math.Round(baseModifier * result.CreatureData.PainThresholdRegen, 0);
            // result.CreatureData.ReceiveAmputationChance = baseModifier * result.CreatureData.ReceiveAmputationChance;
            // result.CreatureData.QuazimorphosisReward = (int)Math.Round(baseModifier * result.CreatureData.QuazimorphosisReward, 0);

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

            var creatureDataType = result.CreatureData.GetType();
            foreach (var propPath in propertiesToModifyPositive)
            {
                Plugin.Logger.Log($"Processing property path: {propPath.Key}");

                var props = propPath.Key.Split('.');

                Plugin.Logger.Log($"\t props: {props.Length}");

                object current = result.CreatureData;
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
                    Plugin.Logger.Log($"\t\t boosting final modifier: TRUE");

                }
                else
                {
                    finalModifier = baseModifier;
                    Plugin.Logger.Log($"\t\t boosting final modifier: FALSE");

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

            var resistSheet = result.CreatureData.ResistSheet._currentResist;
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
                        Plugin.Logger.Log($"\t\t boosting final modifier: TRUE");
                    }
                    else
                    {
                        finalModifier = baseModifier;
                        Plugin.Logger.Log($"\t\t boosting final modifier: FALSE");
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
            result.CreatureData.Perks = new List<Perk>();
            var (Min, Max) = RaritySystem.rarityParamPercentages[rarity];
            int minParams = Math.Max(0, (int)Math.Floor(Min * perksList.Count));
            int maxParams = (int)Math.Ceiling(Max * perksList.Count);
            numParamsToAdjust = _random.Next(minParams, maxParams + 1);

            // Always shuffle list for better randomness
            PathOfQuasimorph.raritySystem.ShuffleList(perksList);

            Dictionary<string, Perk> editable_perks = new Dictionary<string, Perk>();

            for (int i = 0; i < numParamsToAdjust; i++)
            {
                var perkSuffix = _random.Next(0, perksMasteries.Count);
                var perkRecord = Data.Perks.GetRecord($"{perksList[i]}{perksMasteries[perkSuffix]}");

                if (perkRecord != null)
                {
                    result.CreatureData.Perks.Add(PathOfQuasimorph.perkFactoryState.CreatePerk(perkRecord));
                    Plugin.Logger.Log($"Added perk {perksList[i]}{perksMasteries[perkSuffix]}");
                }
                else
                {
                    Plugin.Logger.Log($"{perksList[i]}{perksMasteries[perkSuffix]} null");
                }
            }
        }
    }
}