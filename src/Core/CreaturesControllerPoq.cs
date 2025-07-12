using JetBrains.Annotations;
using MGSC;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
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

        internal List<string> perksList = new List<string> {
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
            //Console.WriteLine("Postfix Patch_ObjHighlightController_Process");

        }

        internal void ApplyStatsFromRarity(ref Monster result, ItemRarity rarity)
        {
            // Get modifier that will be used to boost or hinder stats
            float modifier = PathOfQuasimorph.raritySystem.GetRarityModifier(rarity);



            result.CreatureData.Health.MaxValue = (int)Math.Round(modifier * result.CreatureData.Health.MaxValue, 0);
            result.CreatureData.Health.Value = (int)Math.Round(modifier * result.CreatureData.Health.MaxValue, 0);
            result.CreatureData.BaseDodge = modifier * result.CreatureData.BaseDodge;
            result.CreatureData.BaseActionPoints = (int)Math.Round(modifier * result.CreatureData.BaseActionPoints, 0);
            result.CreatureData.BaseMeleeAccuracy = modifier * result.CreatureData.BaseMeleeAccuracy;
            result.CreatureData.BaseRangeAccuracy = modifier * result.CreatureData.BaseRangeAccuracy;
            result.CreatureData.BaseLosLevel = (int)Math.Round(modifier * result.CreatureData.BaseLosLevel, 0);
            result.CreatureData.ResistSheet._currentResist["blunt"] = modifier * result.CreatureData.ResistSheet._currentResist["blunt"];
            result.CreatureData.ResistSheet._currentResist["pierce"] = modifier * result.CreatureData.ResistSheet._currentResist["pierce"];
            result.CreatureData.ResistSheet._currentResist["lacer"] = modifier * result.CreatureData.ResistSheet._currentResist["lacer"];
            result.CreatureData.ResistSheet._currentResist["fire"] = modifier * result.CreatureData.ResistSheet._currentResist["fire"];
            result.CreatureData.ResistSheet._currentResist["cold"] = modifier * result.CreatureData.ResistSheet._currentResist["cold"];
            result.CreatureData.ResistSheet._currentResist["poison"] = modifier * result.CreatureData.ResistSheet._currentResist["poison"];
            result.CreatureData.ResistSheet._currentResist["shock"] = modifier * result.CreatureData.ResistSheet._currentResist["shock"];
            result.CreatureData.ResistSheet._currentResist["beam"] = modifier * result.CreatureData.ResistSheet._currentResist["beam"];
            //creatureData.Perks FILL PERKS

            // DmgInfo
            result.CreatureData.MeleeDamage.minDmg = (int)Math.Round(modifier * result.CreatureData.MeleeDamage.minDmg, 0);
            result.CreatureData.MeleeDamage.maxDmg = (int)Math.Round(modifier * result.CreatureData.MeleeDamage.maxDmg, 0);
            result.CreatureData.MeleeDamage.critChance = modifier * result.CreatureData.MeleeDamage.critChance;
            result.CreatureData.MeleeDamage.critDmg = modifier * result.CreatureData.MeleeDamage.critDmg;

            result.CreatureData.MeleeThrowbackChance = modifier * result.CreatureData.MeleeThrowbackChance;
            result.CreatureData.PainThresholdLimit = (int)Math.Round(modifier * result.CreatureData.PainThresholdLimit, 0);
            result.CreatureData.PainThresholdRegen = (int)Math.Round(modifier * result.CreatureData.PainThresholdRegen, 0);
            result.CreatureData.ReceiveAmputationChance = modifier * result.CreatureData.ReceiveAmputationChance;
            result.CreatureData.QuazimorphosisReward = (int)Math.Round(modifier * result.CreatureData.QuazimorphosisReward, 0);

            // Perks
            result.CreatureData.Perks = new List<Perk>();
            var (Min, Max) = RaritySystem.rarityParamPercentages[rarity];
            int minParams = Math.Max(0, (int)Math.Floor(Min * perksList.Count));
            int maxParams = (int)Math.Ceiling(Max * perksList.Count);
            int numParamsToAdjust = _random.Next(minParams, maxParams + 1);

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

                }
                else
                {
                    Plugin.Logger.Log($"{perksList[i]}{perksMasteries[perkSuffix]} null");
                }
            }
        }
    }
}