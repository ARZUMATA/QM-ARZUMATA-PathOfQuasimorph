using MGSC;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using UnityEngine;
using UnityEngine.Profiling;
using static QM_PathOfQuasimorph.Core.MagnumPoQProjectsController;
using Random = System.Random;

namespace QM_PathOfQuasimorph.Core
{
    /* RGP themed weapon tiers
         1. **Standard**
         2. **Enhanced** // Magical
         3. **Advanced** // Rare
         4. **Premium** // Epic
         5. **Prototype** // Legendary
         6. **Quantum** // Mythic

      //Arcane
      //Exotic
      //Mythic
      //Relic
      //Premium

      * Name project cleanedDevId: devID_custom_poq_prototype_rndhash
     */

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
        internal AffixManager affixManager = new AffixManager();

        // Weights for each Rarity (lower = rarer)
        private Dictionary<ItemRarity, int> _rarityWeights = new Dictionary<ItemRarity, int>
        {
            { ItemRarity.Standard, 100 },
            { ItemRarity.Enhanced, 50 },
            { ItemRarity.Advanced, 30 },
            { ItemRarity.Premium, 15 },
            { ItemRarity.Prototype, 5 },
            { ItemRarity.Quantum, 1 }
            };

        // Define the percentage of parameters to modify per Rarity
        private Dictionary<ItemRarity, float> rarityParamPercentages = new Dictionary<ItemRarity, float>
        {
            { ItemRarity.Standard, 0.10f },     // 10% of editableParams
            { ItemRarity.Enhanced, 0.25f },     // 25%
            { ItemRarity.Advanced, 0.40f },     // 40%
            { ItemRarity.Premium, 0.55f },      // 55%
            { ItemRarity.Prototype, 0.70f },    // 70%
            { ItemRarity.Quantum, 0.85f }        // 85%
        };

        // Dictionary to store the chance of getting the special trait for each rarity
        private readonly Dictionary<ItemRarity, float> specialTraitChances = new Dictionary<ItemRarity, float>
        {
            { ItemRarity.Standard, 0f },       // 0% chance
            { ItemRarity.Enhanced, 0.05f },      // 5% chance
            { ItemRarity.Advanced, 0.1f },     // 10% chance
            { ItemRarity.Premium, 0.15f },         // 20% chance
            { ItemRarity.Prototype, 0.20f },        // 35% chance
            { ItemRarity.Quantum, 0.25f },    // 50% chance
        };

        private MagnumPoQProjectsController magnumPoQProjectsController;
        public static readonly Dictionary<ItemRarity, string> Colors = new Dictionary<ItemRarity, string>()
        {
            { ItemRarity.Standard,    "#FFFFFF" },         // #FFFFFF (White - common items)
            { ItemRarity.Enhanced,    "#8888FF" },         // #8888FF (Blue - magical items)
            { ItemRarity.Advanced,    "#FFFF77" },         // #FFFF77 // #FFD700 (Gold - rare items)
            { ItemRarity.Premium,     "#AF6025" },         // ##AF6025 (Brown - legendary items) 
            { ItemRarity.Prototype,   "#800080" },         // #FF0000 (Purple - set items or special tier)
            { ItemRarity.Quantum,     "#FF0000" },         // #800080 (Red - elite/unique items)
            // #60C060 (Green - items)
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
            // Define multipliers for each Rarity class
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

        internal void ApplyProjectParameters(ref MagnumProject magnumProject, ItemRarity itemRarity)
        {
            var editableParameters = GetEditableParameters(magnumProject.ProjectType);

            int maxParamsToAdjust = editableParameters.Count;

            // Calculate the number of parameters to adjust based on the percentage
            int numParamsToAdjust = Mathf.RoundToInt(editableParameters.Count * rarityParamPercentages[itemRarity]);
            numParamsToAdjust = Mathf.Max(1, numParamsToAdjust); // Ensure at least 1 parameter is adjusted

            // Shuffle the dictionary
            var editableParamsShuffled = ShuffleDictionary(editableParameters);

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
                    case "_resist":
                        AddIncreasedOrDecreased(defaultParamValue, ref magnumProject, itemRarity, false);
                        break;
                    case "_special_ability":
                        break;
                    case "_damage":
                    case "_crit_damage":
                    case "_max_durability":
                    case "_accuracy":
                    case "_magazine_capacity":
                        AddIncreasedOrDecreased(defaultParamValue, ref magnumProject, itemRarity, true);
                        break;
                    default:
                        break;
                }

                // TODO:
                // Unbreakable
                // Traits
            }
        }

        // Traits
        private void ApplyTraits(ref BasePickupItem item, ItemRarity itemRarity, ItemTraitType itemTraitType, CompositeItemRecord compositeItemRecord)
        {
            var traitsForItemType = GetAddeableTraits(itemTraitType);
            var traitsForItemTypeShuffled = ShuffleDictionary(traitsForItemType);

            // Calculate the number of parameters to adjust based on the percentage
            int numParamsToAdjust = (int)itemRarity;
            
            //Mathf.RoundToInt(traitsForItemType.Count * rarityParamPercentages[itemRarity]);
            numParamsToAdjust = Mathf.Max(1, numParamsToAdjust); // Ensure at least 1 parameter is adjusted

            var canAddUnbreakableTrait = false;

            if (specialTraitChances.TryGetValue(itemRarity, out float chance) && (new Random().Next(1, 100) / 100f) >= chance)
            {
                canAddUnbreakableTrait = true;
            }

            if (itemTraitType == ItemTraitType.ArmorTrait)
            {
                // Armor has no traits. T_T
                foreach (PickupItemComponent comp in ((PickupItem)item).Components)
                {
                    var breakableItemComponent = comp as BreakableItemComponent;
                    if (breakableItemComponent != null)
                    {
                        breakableItemComponent.Unbreakable = canAddUnbreakableTrait;
                        break;
                    }
                }
            }

            if (itemTraitType == ItemTraitType.WeaponTrait)
            {
                foreach (PickupItemComponent comp in ((PickupItem)item).Components)
                {
                    var breakableItemComponent = comp as BreakableItemComponent;
                    if (breakableItemComponent != null)
                    {
                        breakableItemComponent.Unbreakable = canAddUnbreakableTrait;
                        break;
                    }
                }

                foreach (PickupItemComponent comp in ((PickupItem)item).Components)
                {
                    var weaponComponent = comp as WeaponComponent;
                    if (weaponComponent != null)
                    {
                        // If trait already there, don't touch it and remove from candidates.
                        for (int i = 0; i < weaponComponent.Traits.Count; i++)
                        {
                            if (traitsForItemTypeShuffled.ContainsKey(weaponComponent.Traits[i].TraitId))
                            {
                                traitsForItemTypeShuffled.Remove(weaponComponent.Traits[i].TraitId);
                            }
                        }

                        // In case the dictionary is smaller now.
                        if (numParamsToAdjust > traitsForItemTypeShuffled.Count)
                        {
                            numParamsToAdjust = traitsForItemTypeShuffled.Count;
                        }

                        for (int i = 0; i < numParamsToAdjust; i++)
                        {
                            ItemTrait item2 = ItemTraitSystem.CreateItemTrait(traitsForItemTypeShuffled.ElementAt(i).Key);
                            weaponComponent.Traits.Add(item2);
                        }
                    }
                }
            }

            if (itemTraitType == ItemTraitType.AmmoTrait)
            {
                // No way to reliably get uniuque id as it's not a moddable magnum project.
            }
        }

        internal void ApplyTraits(ref BasePickupItem item)
        {
            var wrapper = MagnumProjectWrapper.SplitItemUid(item.Id);

            // We have that item in list so we need to process it and remove later on.
            CompositeItemRecord compositeItemRecord = Data.Items.GetRecord(item.Id, true) as CompositeItemRecord;

            foreach (BasePickupItemRecord basePickupItemRecord in compositeItemRecord.Records)
            {
                Type recordType = basePickupItemRecord.GetType();

                switch (recordType.Name)
                {
                    case nameof(WeaponRecord):
                        ApplyTraits(ref item, wrapper.RarityClass, ItemTraitType.WeaponTrait, compositeItemRecord);
                        break;
                    case nameof(ArmorRecord):
                    case nameof(HelmetRecord):
                    case nameof(LeggingsRecord):
                    case nameof(BootsRecord):
                        ApplyTraits(ref item, wrapper.RarityClass, ItemTraitType.ArmorTrait, compositeItemRecord);
                        break;
                        //case nameof(AmmoRecord):
                        //    ApplyTraits(ref item, wrapper.RarityClass, ItemTraitType.ArmorTrait, compositeItemRecord);
                        break;
                    default:
                        break;
                }
            }
        }

        private static Dictionary<string, ItemTraitRecord> GetAddeableTraits(ItemTraitType itemTraitType)
        {
            Dictionary<string, ItemTraitRecord> addeableTraits = new Dictionary<string, ItemTraitRecord>();

            foreach (var param in Data.ItemTraits._records)
            {
                if (param.Value.ItemTraitType == itemTraitType)
                {
                    addeableTraits.Add(param.Value.Id, param.Value);
                }
            }

            return addeableTraits;
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
            }

            return editable_magnum_projects_params;
        }

        private void ShuffleList<T>(IList<T> list)
        {
            // Fisher-Yates shuffle
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = _random.Next(i + 1);
                T temp = list[i];
                list[i] = list[j];
                list[j] = temp;
            }
        }

        private Dictionary<TKey, TValue> ShuffleDictionary<TKey, TValue>(Dictionary<TKey, TValue> dictionary)
        {
            List<TKey> keys = dictionary.Keys.ToList();
            ShuffleList(keys);  // Use the generic shuffle method for the list of keys

            Dictionary<TKey, TValue> shuffled = new Dictionary<TKey, TValue>();

            foreach (TKey key in keys)
            {
                shuffled[key] = dictionary[key];
            }

            return shuffled;
        }

        internal static void AddAffixes(MagnumProject magnumProject)
        {
            // Add affixes for localization data.
            // English as of time being.

            var magnumProjectWrapper = new MagnumProjectWrapper(magnumProject);

            if (magnumProjectWrapper.PoqItem)
            {
                var affix = AffixManager.GetAffix(magnumProjectWrapper.RarityClass, magnumProject.ProjectType);

                if (affix == null || affix.Count != 2)
                {
                    Plugin.Logger.LogWarning($"AddAffixes failed. Nothing was found.");
                    return;
                }

                // Add our item language keys.
                // We do it here because this method fires earlier than we actually inject item record.
                //// Since Localization.DuplicateKey just copies key and nothing else, it will do same in inject item record method.

                //Localization.DuplicateKey("item." + magnumProjectWrapper.Id + ".name", "item." + magnumProjectWrapper.ReturnItemUid() + ".name");
                //Localization.DuplicateKey("item." + magnumProjectWrapper.Id + ".shortdesc", "item." + magnumProjectWrapper.ReturnItemUid() + ".shortdesc");

                Plugin.Logger.LogWarning($"Updating {affix[0].Text} and {affix[1].Text} for {magnumProjectWrapper.ReturnItemUid()}");

                // Problem, on game load it doesn't have effect.
                UpdateKey("item." + magnumProjectWrapper.ReturnItemUid() + ".name", $"{affix[0].Text} ", "");
                UpdateKey("item." + magnumProjectWrapper.ReturnItemUid() + ".shortdesc", "", $" {affix[1].Text}");
            }
        }

        private static void UpdateKey(string lookupItemId, string prefix, string suffix)
        {
            foreach (KeyValuePair<Localization.Lang, Dictionary<string, string>> languageToDict in Singleton<Localization>.Instance.db)
            {
                if (languageToDict.Value.ContainsKey(lookupItemId))
                {
                    languageToDict.Value[lookupItemId] = prefix + languageToDict.Value[lookupItemId] + suffix;
                }
                else
                {
                    Plugin.Logger.LogWarning($"UpdateKey issue. No key {lookupItemId}");
                }
            }
        }
    }
}
