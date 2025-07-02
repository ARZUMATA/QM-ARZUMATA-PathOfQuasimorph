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

    public enum RarityRolls
    {
        StandardRandom,
        WeightedRolls,
        D20Rolls,
        WeightedRollsAndPickWorst,
        WeightedRollsD20Selector,
    }


    internal class RaritySystem
    {
        private readonly Random _random = new Random();
        internal AffixManager affixManager = new AffixManager();
        private MagnumPoQProjectsController magnumPoQProjectsController;

        // D20 approach
        private const int NUM_ROLLS = 3; // Number of dice rolls
        private const int DICE_SIDES = 20; // Number of sides on the dice
        private RarityRolls rarityRoll = RarityRolls.WeightedRolls;

        public RaritySystem()
        {
            // Test rolls

            //rarityRoll = RarityRolls.StandardRandom;
            //SimulateDrops(); // Standard approach

            //rarityRoll = RarityRolls.D20Rolls;
            //SimulateDrops(); // D20 approach

            //rarityRoll = RarityRolls.WeightedRolls;
            //SimulateDrops(); // Weighted approach

            //rarityRoll = RarityRolls.WeightedRollsAndPickWorst;
            //SimulateDrops();

            //rarityRoll = RarityRolls.WeightedRollsD20Selector;
            //SimulateDrops();

            /*
                StandardRandom:
                    Simulated 10000 item drops:
                        Standard: 4998 (49.98%)
                        Enhanced: 2447 (24.47%)
                        Advanced: 1493 (14.93%)
                        Premium: 778 (7.78%)
                        Prototype: 232 (2.32%)
                        Quantum: 52 (0.52%)

                D20Rolls: (bad sort but too harsh anyway)
                    Simulated 10000 item drops:
                        Standard: 0 (0.00%)
                        Enhanced: 0 (0.00%)
                        Advanced: 0 (0.00%)
                        Premium: 284 (2.84%)
                        Prototype: 4962 (49.62%)
                        Quantum: 4754 (47.54%)

                WeightedRolls:
                    Simulated 10000 item drops:
                        Standard: 5550 (55.50%)
                        Enhanced: 2801 (28.01%)
                        Advanced: 1094 (10.94%)
                        Premium: 430 (4.30%)
                        Prototype: 107 (1.07%)
                        Quantum: 18 (0.18%)

                WeightedRollsAndPickWorst n=1 rolls:
                    Simulated 10000 item drops:
                        Standard: 5025 (50.25%)
                        Enhanced: 2496 (24.96%)
                        Advanced: 1505 (15.05%)
                        Premium: 700 (7.00%)
                        Prototype: 225 (2.25%)
                        Quantum: 49 (0.49%)

                WeightedRollsAndPickWorst n=3 rolls:
                    Simulated 10000 item drops:
                        Standard: 4989 (49.89%)
                        Enhanced: 2502 (25.02%)
                        Advanced: 1456 (14.56%)
                        Premium: 750 (7.50%)
                        Prototype: 256 (2.56%)
                        Quantum: 47 (0.47%)

                WeightedRollsD20Selector:
                    Simulated 10000 item drops:
                        Standard: 5577 (55.77%)
                        Enhanced: 2836 (28.36%)
                        Advanced: 1035 (10.35%)
                        Premium: 383 (3.83%)
                        Prototype: 143 (1.43%)
                        Quantum: 26 (0.26%)
            */
        }

        private void SimulateDrops()
        {
            // Simulate 10000 item drops to see the distribution

            Dictionary<ItemRarity, int> rarityCounts = new Dictionary<ItemRarity, int>();

            foreach (ItemRarity rarity in Enum.GetValues(typeof(ItemRarity)))
            {
                rarityCounts[rarity] = 0;
            }

            int numTrials = 10000;
            for (int i = 0; i < numTrials; i++)
            {
                ItemRarity rarity = SelectRarity();
                rarityCounts[rarity]++;
            }

            // Print the results
            Console.WriteLine($"\nSimulated {numTrials} item drops:");
            foreach (var rarityPair in rarityCounts)
            {
                double percentage = (double)rarityPair.Value / numTrials * 100;
                Console.WriteLine($"{rarityPair.Key}: {rarityPair.Value} ({percentage:F2}%)");
            }
        }

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

        private Dictionary<ItemRarity, int> _rarityWeightsForWeighted = new Dictionary<ItemRarity, int>
        {
            { ItemRarity.Standard, 1000 },
            { ItemRarity.Enhanced, 500 },
            { ItemRarity.Advanced, 200 },
            { ItemRarity.Premium, 75 },
            { ItemRarity.Prototype, 20 },
            { ItemRarity.Quantum, 5 }
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

        public ItemRarity SelectRarityWithD20Rolls()
        {
            // Sort rarities by weight in descending order (most common to rarest)
            var sortedRarities = _rarityWeights.OrderByDescending(x => x.Value).ToList();

            foreach (var rarityPair in sortedRarities)
            {
                ItemRarity rarity = rarityPair.Key;
                int weight = rarityPair.Value;

                // Perform multiple rolls and check if all are greater than or equal to the rarity's weight.
                bool allRollsPass = true;

                for (int i = 0; i < NUM_ROLLS; i++)
                {
                    int roll = _random.Next(1, DICE_SIDES + 1); // Simulate a d20 roll (1 to 20 inclusive)
                    if (roll < weight)
                    {
                        allRollsPass = false;
                        break;
                    }
                }

                if (allRollsPass)
                {
                    return rarity;
                }
            }

            // Fallback
            return ItemRarity.Standard;
        }

        public ItemRarity SelectRarityWeighted()
        {
            // Calculate the total weight
            int totalWeight = _rarityWeightsForWeighted.Values.Sum();

            // Generate a random number within the total weight range
            int randomNumber = _random.Next(0, totalWeight);

            // Iterate through the rarities and determine which one the random number falls into
            int cumulativeWeight = 0;

            foreach (var rarityPair in _rarityWeightsForWeighted)
            {
                cumulativeWeight += rarityPair.Value;
                if (randomNumber < cumulativeWeight)
                {
                    return rarityPair.Key;
                }
            }

            // Fallback
            return ItemRarity.Standard;
        }

        // Performs N weighted rarity rolls and selects the worst
        public ItemRarity SelectRarityWeightedAndPickWorst(int rolls = 3)
        {
            if (rolls < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(rolls), "Number of rolls must be at least 1.");
            }

            // Initialize with the 'best' possible rarity, so any roll will be 'worse' (lower in enum value) than it.
            // ItemRarity enum values are ordered where higher int value = better rarity.
            ItemRarity worstRarityFound = (ItemRarity)Enum.GetValues(typeof(ItemRarity)).Cast<int>().Max();

            for (int i = 0; i < rolls; i++)
            {
                ItemRarity currentRollRarity = SelectRarityWeighted();

                // If the current roll's rarity value is less than the worst found, update worstRarityFound.
                if ((int)currentRollRarity < (int)worstRarityFound)
                {
                    worstRarityFound = currentRollRarity;
                }
            }

            return worstRarityFound;

        }

        // Selects an item rarity by performing N weighted rolls and then having those rolls
        // compete using a D20 system modified by their base rarity score.
        public ItemRarity SelectRarityWeightedRollsD20Selector()
        {
            List<ItemRarity> rolledRarities = new List<ItemRarity>();

            rolledRarities.Add(SelectRarityWeighted());

            // Initialize base scores for D20 competition
            // Higher values mean the D20 roll has less relative impact on the final outcome.
            // Lower values mean the D20 roll has a greater chance of making a lower rarity win.
            var _rarityBaseScores = new Dictionary<ItemRarity, int>
            {
                { ItemRarity.Standard,  10 },
                { ItemRarity.Enhanced,  20 },
                { ItemRarity.Advanced,  30 },
                { ItemRarity.Premium,   40 },
                { ItemRarity.Prototype, 50 },
                { ItemRarity.Quantum,   60 }
            };

            // Determine the winner using D20 rolls + base rarity scores
            ItemRarity bestRarity = ItemRarity.Standard; // Default initial best
            int highestScore = -1; // Use -1 for initial comparison

            foreach (var rarityCandidate in rolledRarities)
            {
                // Get the base score for the current rarity candidate
                int baseScore;
                if (!_rarityBaseScores.TryGetValue(rarityCandidate, out baseScore))
                {
                    // Fallback score if rarity isn't defined in _rarityBaseScores
                    baseScore = 0;
                }

                int d20Roll = _random.Next(1, 21); // Roll a D20 (1 to 20 inclusive)

                int currentTotalScore = baseScore + d20Roll;

                // If this candidate's score is higher, it becomes the new best
                if (currentTotalScore > highestScore)
                {
                    highestScore = currentTotalScore;
                    bestRarity = rarityCandidate;
                }
            }

            return bestRarity;
        }

        public ItemRarity SelectRarity()
        {
            switch (rarityRoll)
            {
                case RarityRolls.StandardRandom:
                    return SelectRarityStandardRandom();
                case RarityRolls.D20Rolls:
                    return SelectRarityWithD20Rolls();
                case RarityRolls.WeightedRolls:
                    return SelectRarityWeighted();
                case RarityRolls.WeightedRollsAndPickWorst:
                    return SelectRarityStandardRandom();
                case RarityRolls.WeightedRollsD20Selector:
                    return SelectRarityWeightedRollsD20Selector();
            }

            return ItemRarity.Standard;
        }

        private ItemRarity SelectRarityStandardRandom()
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
                        if (canAddUnbreakableTrait)
                        {
                            breakableItemComponent.Unbreakable = true;
                            breakableItemComponent.SetMaxDurability(breakableItemComponent.MaxDurability, true);
                        }

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
                        if (canAddUnbreakableTrait)
                        {
                            breakableItemComponent.Unbreakable = true;
                            breakableItemComponent.SetMaxDurability(breakableItemComponent.MaxDurability, true);
                        }

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
                UpdateKey("item." + magnumProjectWrapper.ReturnItemUid() + ".name", $"{affix[0].Text} ", "", magnumProjectWrapper.ReturnItemUid(true));
                UpdateKey("item." + magnumProjectWrapper.ReturnItemUid() + ".shortdesc", "", $" {affix[1].Text}", magnumProjectWrapper.ReturnItemUid(true));
            }
        }

        private static void UpdateKey(string lookupItemId, string prefix, string suffix, string originalUid)
        {
            var originalName = Localization.Get("item." + originalUid + ".name");
            var originalShortdesc = Localization.Get("item." + originalUid + ".shortdesc");

            foreach (KeyValuePair<Localization.Lang, Dictionary<string, string>> languageToDict in Singleton<Localization>.Instance.db)
            {
                if (languageToDict.Value.ContainsKey(lookupItemId))
                {
                    // Update prefix
                    if (lookupItemId.Contains(".name"))
                    {
                        languageToDict.Value[lookupItemId] = prefix + originalName;
                    }

                    // Update suffix
                    if (lookupItemId.Contains(".shortdesc"))
                    {
                        languageToDict.Value[lookupItemId] = originalShortdesc + suffix;

                    }
                }
                else
                {
                    Plugin.Logger.LogWarning($"UpdateKey issue. No key {lookupItemId}");
                }
            }
        }
    }
}
