using MGSC;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Profiling;
using Random = System.Random;

namespace QM_PathOfQuasimorph.Core
{
    public enum ItemRarity
    {
        Standard,
        Enhanced,
        Advanced,
        Premium,
        Prototype,
        Quantum,
    }

    internal class RaritySystem
    {
        private readonly Random _random = new Random();

        // Weights for each rarity (lower = rarer)
        private Dictionary<ItemRarity, int> _rarityWeights = new Dictionary<ItemRarity, int>
        {
            { ItemRarity.Standard, 100 },
            { ItemRarity.Enhanced, 50 },
            { ItemRarity.Advanced, 30 },
            { ItemRarity.Premium, 15 },
            { ItemRarity.Prototype, 5 },
            { ItemRarity.Quantum, 1 }
            };

        // Define the percentage of parameters to modify per rarity
        private Dictionary<ItemRarity, float> rarityParamPercentages = new Dictionary<ItemRarity, float>
        {
            { ItemRarity.Standard, 0.10f },     // 10% of editableParams
            { ItemRarity.Enhanced, 0.25f },     // 25%
            { ItemRarity.Advanced, 0.40f },     // 40%
            { ItemRarity.Premium, 0.55f },      // 55%
            { ItemRarity.Prototype, 0.70f },    // 70%
            { ItemRarity.Quantum, 0.85f }        // 85%
        };

        public static readonly Dictionary<ItemRarity, string> Colors = new Dictionary<ItemRarity, string>()
        {
            { ItemRarity.Standard,    "#FFFFFF" },         // #FFFFFF (White - common items)
            { ItemRarity.Enhanced,    "#8888FF" },         // #8888FF (Blue - magical items)
            { ItemRarity.Advanced,    "#FFFF77" },         // #FFFF77 // #FFD700 (Gold - legendary items)
            { ItemRarity.Premium,     "#AF6025" },         // ##AF6025 (Brown - rare items) 
            { ItemRarity.Prototype,   "#800080" },         // #FF0000 (Purple - set items or special tier)
            { ItemRarity.Quantum,     "#FF0000" },         // #800080 (Red - elite/unique items)
        };

        public Color RarityToUnityColor(ItemRarity rarity)
        {
            var color = Helpers.HexStringToUnityColor(Colors[rarity]);
            color.a = 0.25f;
            return color;
        }

        public ItemRarity SelectRarity()
        {
            int totalWeight = 0;
            foreach (var weight in _rarityWeights.Values)
            {
                totalWeight += weight;
            }

            int randomValue = _random.Next(totalWeight);
            int cumulativeWeight = 0;

            foreach (var rarity in Enum.GetValues(typeof(ItemRarity)))
            {
                int weight = _rarityWeights[(ItemRarity)rarity];
                if (randomValue < cumulativeWeight + weight)
                    return (ItemRarity)rarity;
                cumulativeWeight += weight;
            }

            return ItemRarity.Standard;
        }

        public void ApplyRarityToItem(Dictionary<string, MagnumProjectParameter> editableParams, ItemRarity itemRarity, ref MagnumProject newProject)
        {
            int maxParamsToAdjust = editableParams.Count;

            // Calculate the number of parameters to adjust based on the percentage
            int numParamsToAdjust = Mathf.RoundToInt(editableParams.Count * rarityParamPercentages[itemRarity]);
            numParamsToAdjust = Mathf.Max(1, numParamsToAdjust); // Ensure at least 1 parameter is adjusted

            // Shuffle the dictionary
            var editableParamsShuffled = ShuffleDictionary(editableParams);

            for (int i = 0; i < numParamsToAdjust; i++)
            {
                MagnumProjectParameter defaultParamValue = editableParamsShuffled.Values.ToArray()[i];

                // Increase
                // everything else

                // Decrease
                // _weight
                // _reload_duration
                // _scatter_angle

                // Special case
                // _special_ability

                switch (defaultParamValue.Id)
                {
                    case "_weight":
                    case "_reload_duration":
                    case "_scatter_angle":
                        AddIncreasedOrDecreased(defaultParamValue, ref newProject, itemRarity, false);
                        break;
                    case "_special_ability":
                        break;
                    case "_damage":
                    case "_crit_damage":
                    case "_max_durability":
                    case "_accuracy":
                    case "_magazine_capacity":
                        AddIncreasedOrDecreased(defaultParamValue, ref newProject, itemRarity, true);
                        break;
                    //AddSpecialAbility(defaultParamValue, ref newProject, itemRarity);
                    default:
                        break;
                }

                // TODO:
                // Unbreakable
                // Traits
            }
        }

        private void AddSpecialAbility(MagnumProjectParameter _projectParameter, ref MagnumProject project, ItemRarity itemRarity)
        {
        }

        // Used part of code from  MagnumProjectNumericParameterPanel.Initialize
        private void AddIncreasedOrDecreased(MagnumProjectParameter _projectParameter, ref MagnumProject project, ItemRarity itemRarity, bool increase)
        {
            float _defaultValue = 0f;

            switch (_projectParameter.ParameterType)
            {
                case MagnumProjectParameterType.Integer:
                    _defaultValue = (float)((int)project.GetParameterDefaultValue(_projectParameter));
                    break;
                case MagnumProjectParameterType.Float:
                case MagnumProjectParameterType.CritDamage:
                case MagnumProjectParameterType.WeaponAccuracy:
                case MagnumProjectParameterType.WeaponScatterAngle:
                case MagnumProjectParameterType.ResistBlunt:
                case MagnumProjectParameterType.ResistPierce:
                case MagnumProjectParameterType.ResistLacer:
                case MagnumProjectParameterType.ResistFire:
                case MagnumProjectParameterType.ResistBeam:
                case MagnumProjectParameterType.ResistShock:
                case MagnumProjectParameterType.ResistPoison:
                case MagnumProjectParameterType.ResistCold:
                    _defaultValue = (float)project.GetParameterDefaultValue(_projectParameter);
                    break;
                case MagnumProjectParameterType.Damage:
                    {
                        DmgInfo dmgInfo2 = (DmgInfo)project.GetParameterDefaultValue(_projectParameter);
                        _defaultValue = (float)dmgInfo2.minDmg;
                        break;
                    }
            }

            Plugin.Logger.Log($"project {project.ProjectType}");
            Plugin.Logger.Log($"\t AppliedModifications:");

            foreach (var mod in project.AppliedModifications)
            {
                Plugin.Logger.Log($"\t {mod.Key} - {mod.Value}:");
            }

            Plugin.Logger.Log($"\t We want:");
            Plugin.Logger.Log($"\t\t _projectParameter.Id {_projectParameter.Id}");
            Plugin.Logger.Log($"\t\t _projectParameter.val {CalculateParamValue(_defaultValue, itemRarity, increase).ToString()}");

            project.AppliedModifications.Remove(_projectParameter.Id);
            project.AppliedModifications.Add(_projectParameter.Id, CalculateParamValue(_defaultValue, itemRarity, increase).ToString());
        }

        private float CalculateParamValue(float defaultValue, ItemRarity rarity, bool increase)
        {
            float[] rarityModifiers = GetRarityModifiers(rarity);
            float modifier = rarityModifiers[_random.Next(rarityModifiers.Length)];

            if (increase)
            {
                return defaultValue * modifier;
            }
            else
            {
                return defaultValue / modifier;
            }
        }

        private float[] GetRarityModifiers(ItemRarity rarity)
        {
            // Define multipliers for each rarity class
            // Feel free to adjust these values to balance the system
            switch (rarity)
            {
                case ItemRarity.Enhanced:
                    return new float[] { 1.2f, 1.3f, 1.5f };
                case ItemRarity.Advanced:
                    return new float[] { 1.5f, 1.6f, 1.8f };
                case ItemRarity.Premium:
                    return new float[] { 1.7f, 1.8f, 2.0f };
                case ItemRarity.Prototype:
                    return new float[] { 2.0f, 2.2f, 2.5f };
                case ItemRarity.Quantum:
                    return new float[] { 2.5f, 3.0f, 4.0f };
                default:
                    return new float[] { 1.0f }; // No change for Standard
            }
        }

        internal void ApplyProjectParameters(ref MagnumProject newProject, ItemRarity itemRarity)
        {
            // How many upgrades it can hold?
            var upgradeLimit = newProject.ModifyLevelLimit;
            Plugin.Logger.Log($"\t\t upgradeLimit: {upgradeLimit}");

            // Determine projectType i.e. RangeWeapon
            var projectType = newProject.ProjectType;

            var editableParameters = GetEditableParameters(projectType);

            ApplyRarityToItem(editableParameters, itemRarity, ref newProject);
        }

        private static Dictionary<string, MagnumProjectParameter> GetEditableParameters(MagnumProjectType projectType)
        {
            // Get magnum_projects_params that we can edit for that projectType
            Dictionary<string, MagnumProjectParameter> editable_magnum_projects_params = new Dictionary<string, MagnumProjectParameter>();

            // Iterate whole list of record to get what we need.
            foreach (var param in Data.MagnumProjectParameters._records)
            {
                // Plugin.Logger.Log($"\t\t record: {param.Key}"); // record: rangeweapon_damage
                if (param.Value.ProjectType == projectType)
                {
                    editable_magnum_projects_params.Add(param.Value.Id, param.Value);
                }

                // Example of dictionary
                //rangeweapon_damage
                //rangeweapon_crit_damage
                //rangeweapon_max_durability
                //rangeweapon_accuracy
                //rangeweapon_scatter_angle
                //rangeweapon_weight
                //rangeweapon_reload_duration
                //rangeweapon_magazine_capacity
                //rangeweapon_special_ability

                // Use reflection to get all public fields of mpj
                //var type = param.Value.GetType();
                //Plugin.Logger.Log($"\t\t type: {type}");

                //var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
                //var members = type.GetMembers(BindingFlags.Public | BindingFlags.Instance);


                //Plugin.Logger.Log($"\t\t fields: {fields.Length}");
                //Plugin.Logger.Log($"\t\t members: {members.Length}");

                //foreach (var field in members)
                //{
                //    Plugin.Logger.Log($"\t\t\t\t Field: {field.Name} - Value: {field.Name}");
                //    //Plugin.Logger.Log($"\t\t\t\t Field: {field.Name} - Value: {field.GetValue(type)}");
                //}

                //Plugin.Logger.Log($"\t\t\t test: {mpj.ParameterName}");

            }
            return editable_magnum_projects_params;
        }

        private Dictionary<string, MagnumProjectParameter> ShuffleDictionary(Dictionary<string, MagnumProjectParameter> dictionary)
        {
            List<KeyValuePair<string, MagnumProjectParameter>> list = dictionary.ToList();

            // Fisher-Yates shuffle
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = _random.Next(i + 1);
                KeyValuePair<string, MagnumProjectParameter> temp = list[i];
                list[i] = list[j];
                list[j] = temp;
            }

            // Create a new dictionary from the shuffled list
            return list.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }
    }
}
