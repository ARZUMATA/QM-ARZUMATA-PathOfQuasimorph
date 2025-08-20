using MGSC;
using QM_PathOfQuasimorph.Controllers;
using QM_PathOfQuasimorph.PoqHelpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Windows;
using static QM_PathOfQuasimorph.Controllers.CreaturesControllerPoq;
using static QM_PathOfQuasimorph.Controllers.MagnumPoQProjectsController;
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
        internal AffixManager affixManager = new AffixManager();
        private MagnumPoQProjectsController magnumPoQProjectsController;
        public const int AMOUNT_PREFIXES = 10; // csv has 10 prefixes per rarity
        public const int AMOUNT_SUFFIXES = 5; // CSV has 5 suffies per rarity param
        private static Logger _logger = new Logger(null, typeof(RaritySystem));
        public BlackJackRoller blackJackRoller = new BlackJackRoller(5);

        // D20 approach
        private const int NUM_ROLLS = 3; // Number of dice rolls
        private const int DICE_SIDES = 20; // Number of sides on the dice
        public static float PARAMETER_BOOST_MIN = 1.2f;
        public static float PARAMETER_BOOST_MAX = 1.8f;
        public static float AVERAGE_RESIST_APPLY_CHANCE = 50;
        public float PARAMETER_HINDER_CHANCE = 50;
        public float PARAMETER_HINDER_PERCENT = 20;
        public float UNBREAKABLE_ENTRY_CHANCE = 0.20f;
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

            //SimulateHinder();
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

            LoadCustomWeights();
        }

        private void SimulateHinder()
        {
            int totalHindered = 0;
            int totalImproved = 0;
            int numParamsToAdjust = 8;
            int numParamsToHinder = (int)Math.Floor(numParamsToAdjust * PARAMETER_HINDER_PERCENT / 100f); // 20% of adjusted parameters to hinder
            int numParamsToImprove = numParamsToAdjust - numParamsToHinder;

            for (int i = 0; i < 100; i++)
            {
                int hinderedCount = 0;
                int improvedCount = 0;

                for (int j = 0; j < numParamsToAdjust; j++)
                {
                    bool hinder = ShouldHinderParameter(
                        ref hinderedCount, ref improvedCount,
                        numParamsToHinder, numParamsToImprove);

                    if (hinder)
                    {
                        totalHindered++;
                    }
                    else
                    {
                        totalImproved++;
                    }
                }
            }

            Console.WriteLine($"Total Hindered (out of 800): {totalHindered}");
            Console.WriteLine($"Total Improved (out of 800): {totalImproved}");

        }
        private void LoadCustomWeights()
        {
            string weightPath = Path.Combine(Plugin.ConfigDirectories.ModPersistenceFolder, "Rarities.csv");
            long fileSize = 0;

            if (File.Exists(weightPath))
            {
                fileSize = new FileInfo(weightPath).Length;
                if (fileSize == 0)
                {

                }
            }
            // If we don't have our rarities csv configs, dump a new file for user.

            if (!File.Exists(weightPath) || fileSize == 0)
            {
                using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("QM_PathOfQuasimorph.Files.Rarities.csv"))
                using (StreamReader reader = new StreamReader(stream))
                {
                    File.WriteAllText(weightPath, reader.ReadToEnd());
                }
            }
            else
            {
                // If file exists, try migrate settings
                CompareRaritiesFiles(weightPath, "QM_PathOfQuasimorph.Files.Rarities.csv");
            }

            if (Plugin.Config.CustomWeights)
            {
                // Load the custom weights from the CSV file
                LoadRaritiesFromCSV(weightPath);
            }
        }

        private void CompareRaritiesFiles(string weightPath, string embeddedFile)
        {
            RaritySystemCSVHelper.DoMerge(weightPath, embeddedFile);
        }

        private void LoadRaritiesFromCSV(string filePath)
        {

            _logger.Log($"Loading rarity data from CSV {filePath}");

            string line;
            bool inArbitrarySection = false;
            bool inRaritySection = false;
            bool inMonsterMasteriesSection = false;
            bool inSynthraformersSection = false;
            string[] headers = null;

            Dictionary<string, float> arbitraryValues = new Dictionary<string, float>();

            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (StreamReader reader = new StreamReader(fileStream))
            {
                while ((line = reader.ReadLine()) != null)
                {
                    line = line.Trim();

                    // Skip empty lines if any
                    if (string.IsNullOrEmpty(line)) continue;
                    // If the line is the header

                    if (line.StartsWith("Rarity"))
                    {
                        inRaritySection = true;
                        headers = line.Split(',');
                        continue;
                    }

                    if (line.StartsWith("MonsterMasteries"))
                    {
                        inMonsterMasteriesSection = true;
                        headers = line.Split(',');
                        continue;
                    }

                    if (line.StartsWith("Synthraformers"))
                    {
                        inSynthraformersSection = true;
                        headers = line.Split(',');
                        continue;

                    }
                    // Check if the line starts the Arbitrary Values section
                    if (line.StartsWith("Arbitrary"))
                    {
                        inArbitrarySection = true;
                        continue;
                    }

                    if (inRaritySection)
                    {
                        string[] parts = line.Split(',');
                        string rarityName = parts[0].Trim();

                        if (Enum.TryParse(rarityName, out ItemRarity rarity))
                        {
                            // Assign values to dictionaries.
                            _rarityModifiers[rarity] = (float.Parse(parts[1], CultureInfo.InvariantCulture),
                                                       float.Parse(parts[2], CultureInfo.InvariantCulture));
                            _rarityWeightsForWeighted[rarity] = int.Parse(parts[3], CultureInfo.InvariantCulture);
                            rarityParamPercentages[rarity] = (float.Parse(parts[4], CultureInfo.InvariantCulture) / 100f,
                                                            float.Parse(parts[5], CultureInfo.InvariantCulture) / 100f);
                            rarityTraitRanges[rarity] = (float.Parse(parts[6], CultureInfo.InvariantCulture) / 100f,
                                                       float.Parse(parts[7], CultureInfo.InvariantCulture) / 100f);
                            unbreakableTraitPercent[rarity] = float.Parse(parts[8], CultureInfo.InvariantCulture);
                        }
                    }

                    if (inMonsterMasteriesSection)
                    {
                        string[] parts = line.Split(',');
                        string rarityName = parts[0].Trim();

                        if (Enum.TryParse(rarityName, out MonsterMasteryTier mastery))
                        {
                            // Assign values to dictionaries.
                            PathOfQuasimorph.creaturesControllerPoq._masteryModifiers[mastery] =
                                (float.Parse(parts[1], CultureInfo.InvariantCulture),
                                float.Parse(parts[2], CultureInfo.InvariantCulture));

                            PathOfQuasimorph.creaturesControllerPoq._masteryTierWeights[mastery] = int.Parse(parts[3], CultureInfo.InvariantCulture);

                            PathOfQuasimorph.creaturesControllerPoq._masteryModifiers_Health[mastery] =
                             (float.Parse(parts[4], CultureInfo.InvariantCulture),
                             float.Parse(parts[5], CultureInfo.InvariantCulture));

                            PathOfQuasimorph.creaturesControllerPoq._masteryModifiers_Resists[mastery] =
                             (float.Parse(parts[6], CultureInfo.InvariantCulture),
                             float.Parse(parts[7], CultureInfo.InvariantCulture));
                        }
                    }

                    //DropChances
                    if (inSynthraformersSection)
                    {
                        string[] parts = line.Split(',');
                        string rarityName = parts[0].Trim();

                        if (Enum.TryParse(rarityName, out SynthraformerController.SynthraformerType type))
                        {
                            PathOfQuasimorph.synthraformerController.DropChances[type] = float.Parse(parts[1], CultureInfo.InvariantCulture);
                            PathOfQuasimorph.synthraformerController.ProduceTimeMap[type] = float.Parse(parts[2], CultureInfo.InvariantCulture);
                        }
                    }

                    if (inArbitrarySection)
                    {
                        string[] parts = line.Split(',');
                        string key = parts[0].Trim();
                        float value = float.Parse(parts[1].Trim(), CultureInfo.InvariantCulture);
                        _logger.Log($"Writing arbitrary data {key} {value}");

                        arbitraryValues[key] = value;
                    }



                }
            }

            PARAMETER_BOOST_MIN = arbitraryValues["PARAMETER_BOOST_MIN"];
            PARAMETER_BOOST_MAX = arbitraryValues["PARAMETER_BOOST_MAX"];
            AVERAGE_RESIST_APPLY_CHANCE = arbitraryValues["AVERAGE_RESIST_APPLY_CHANCE"];
            UNBREAKABLE_ENTRY_CHANCE = arbitraryValues["UNBREAKABLE_ENTRY_CHANCE"];
            PARAMETER_HINDER_CHANCE = arbitraryValues["PARAMETER_HINDER_CHANCE"];
            PARAMETER_HINDER_PERCENT = arbitraryValues["PARAMETER_HINDER_PERCENT"];
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

        // ParamIdentifires for encoding to UID
        public static List<string> ParamIdentifiers = new List<string>
        {
            "resist_blunt",
            "resist_pierce",
            "resist_lacer",
            "resist_fire",
            "resist_beam",
            "resist_shock",
            "resist_poison",
            "resist_cold",
            "weight",
            "max_durability",
            "damage",
            "crit_damage",
            "accuracy",
            "scatter_angle",
            "reload_duration",
            "magazine_capacity",
            "special_ability",
            "none"
        };


        [Obsolete]
        private List<string> rangedTraitsBlacklist = new List<string> {
            "perfect_throw",
            "piercing_throw",
            "cleave",
            "unthrowable",
            "critical_throw",
            "backstab",
        };

        [Obsolete]
        private List<string> meleeTraitsBlacklist = new List<string>(){
            "suppressor",
            "ramp_up",
            "bipod",
            "optic_sight",
            "collimator",
            "laser_sight",
            "suppressive_fire",
        };

        // Define multipliers for each Rarity class
        public Dictionary<ItemRarity, (float Min, float Max)> _rarityModifiers = new Dictionary<ItemRarity, (float Min, float Max)>
        {
            { ItemRarity.Standard,   ( 1.0f,   1.0f  ) },  // Standard = Common // No change for Standard 
            { ItemRarity.Enhanced,   ( 1.05f,  1.15f ) },  // Enhanced = Magic
            { ItemRarity.Advanced,   ( 1.15f,  1.25f ) },  // Advanced = Rare
            { ItemRarity.Premium,    ( 1.30f,  1.40f ) },  // Premium = Epic
            { ItemRarity.Prototype,  ( 1.5f,   1.7f ) },  // Prototype = Legendary
            { ItemRarity.Quantum,    ( 2f,     3f ) },  // Quantum = Mythic

            //{ ItemRarity.Premium,   new float[] { 8.0f, 10.0f, 12.0f } }, // Premium = Epic
            //{ ItemRarity.Prototype, new float[] { 15.0f, 17.0f, 19.0f } }, // Prototype = Legendary
            //{ ItemRarity.Quantum,   new float[] { 19.5f, 20.0f, 30f } }, // Quantum = Mythic
            // Quantum is extremely top-tier with near-maximum boost
        };

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
        public Dictionary<ItemRarity, (float Min, float Max)> rarityParamPercentages = new Dictionary<ItemRarity, (float Min, float Max)>
        {
            { ItemRarity.Standard,  (0f    , 0f   ) },          // 0% of editableParams
            { ItemRarity.Enhanced,  (0.125f , 0.25f) },       // 25%
            { ItemRarity.Advanced,  (0.20f , 0.40f) },       // 40%
            { ItemRarity.Premium,   (0.275f , 0.55f) },       // 55%
            { ItemRarity.Prototype, (0.425f , 0.85f) },       // 85%
            { ItemRarity.Quantum,   (0.5f , 1.00f) }        // 100%
        };

        // Define the percentage of traits to modify per Rarity
        public Dictionary<ItemRarity, (float Min, float Max)> rarityTraitRanges = new Dictionary<ItemRarity, (float Min, float Max)>
        {
            { ItemRarity.Standard,  (0f,     0f) },          // Ignored, no traits applied
            { ItemRarity.Enhanced,  (0f,     0.0435f) },     // 1 trait (4.35% of max 23 traits)
            { ItemRarity.Advanced,  (0.087f, 0.13f) },       // 2-3 traits (8.70% to 13.04% of max 23)
            { ItemRarity.Premium,   (0.13f,  0.174f) },      // 3-4 traits (13.04% to 17.39% of max 23)
            { ItemRarity.Prototype, (0.174f, 0.217f) },      // 4-5 traits (17.39% to 21.74% of max 23)
            { ItemRarity.Quantum,   (0.348f, 0.348f) },      // 8 traits (34.78% of max 23)
        };

        public readonly Dictionary<ItemRarity, float> unbreakableTraitPercent = new Dictionary<ItemRarity, float>
        {
            { ItemRarity.Standard,  0f },       // 0% (never gets unbreakable)
            { ItemRarity.Enhanced,  0.1f },      // 0.1% in eligible pool
            { ItemRarity.Advanced,  1f },        // 1% in eligible pool
            { ItemRarity.Premium,   5f },        // 5% in eligible pool
            { ItemRarity.Prototype, 20f },       // 20% in eligible pool
            { ItemRarity.Quantum,   75f },     // 75% in eligible pool
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
                    int roll = Helpers._random.Next(1, DICE_SIDES + 1); // Simulate a d20 roll (1 to 20 inclusive)
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

        public T SelectRarityWeighted<T>(Dictionary<T, int> weights)// where T : Enum
        {
            // Calculate the total weight
            int totalWeight = weights.Values.Sum();

            // Generate a random number within the total weight range
            int randomNumber = Helpers._random.Next(0, totalWeight + 1);

            // Iterate through the rarities and determine which one the random number falls into
            int cumulativeWeight = 0;

            foreach (var rarityPair in weights)
            {
                cumulativeWeight += rarityPair.Value;
                if (randomNumber < cumulativeWeight)
                {
                    return rarityPair.Key;
                }
            }

            // Fallback (e.g., default enum value or first key)
            return weights.Keys.First();
        }

        // Performs N weighted rarity rolls and selects the worst
        public ItemRarity SelectRarityWeightedAndPickWorst(int rolls = 3)
        {
            if (rolls < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(rolls), "Number of rolls must be at least 1.");
            }

            // Initialize with the 'best' possible rarity, so any roll will be 'worse' (lower in enum _defaultValue) than it.
            // ItemRarity enum values are ordered where higher int _defaultValue = better rarity.
            ItemRarity worstRarityFound = (ItemRarity)Enum.GetValues(typeof(ItemRarity)).Cast<int>().Max();

            for (int i = 0; i < rolls; i++)
            {
                ItemRarity currentRollRarity = SelectRarityWeighted(_rarityWeightsForWeighted);

                // If the current roll's rarity _defaultValue is less than the worst found, update worstRarityFound.
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

            rolledRarities.Add(SelectRarityWeighted(_rarityWeightsForWeighted));

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

                int d20Roll = Helpers._random.Next(1, DICE_SIDES + 1); // Roll a D20 (1 to 20 inclusive)

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
                    return SelectRarityWeighted(_rarityWeightsForWeighted);
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

            int randomValue = Helpers._random.Next(totalWeight + 1);
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
        private void AddIncreasedOrDecreased(
            MagnumProjectParameter _projectParameter,
            ref MagnumProject project,
            ItemRarity itemRarity,
            bool increase,
            MagnumProjectParameter boostedParam,
            float averageResist,
            bool hinder,
            bool rarityExtraBoost)
        {
            float _defaultValue = 0f;
            bool isResist = false;
            bool boost = false;
            bool averageResistApplied = false;

            if (boostedParam.Id == _projectParameter.Id)
            {
                _logger.Log($"\t\t Bosting {boostedParam.Id}");
                boost = true;
            }

            _logger.Log($"\t\t Hinder {hinder}");

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

            switch (_projectParameter.ParameterType)
            {
                case MagnumProjectParameterType.ResistBlunt:
                case MagnumProjectParameterType.ResistPierce:
                case MagnumProjectParameterType.ResistLacer:
                case MagnumProjectParameterType.ResistFire:
                case MagnumProjectParameterType.ResistBeam:
                case MagnumProjectParameterType.ResistShock:
                case MagnumProjectParameterType.ResistPoison:
                case MagnumProjectParameterType.ResistCold:
                    isResist = true;
                    break;
            }

            // Case where we have zero resist, let boost it a bit.

            project.AppliedModifications.Remove(_projectParameter.Id);
            var calculatedValue = CalculateParamValue(_defaultValue, itemRarity, increase, boost, isResist, averageResist, averageResistApplied, out averageResistApplied, hinder, rarityExtraBoost);
            var clampedValue = Mathf.Clamp(
                    calculatedValue,
                    _projectParameter.MinValue,
                    _projectParameter.MaxValue
                    );

            _logger.Log($"\t\t AppliedModifications");
            _logger.Log($"\t\t\t {_projectParameter.Id}");
            _logger.Log($"\t\t\t\t Default: {_defaultValue}");
            _logger.Log($"\t\t\t\t ClampedValue: {clampedValue}\n");

            // Apply back
            switch (_projectParameter.ParameterType)
            {
                case MagnumProjectParameterType.Integer:
                case MagnumProjectParameterType.Damage:
                    project.AppliedModifications.Add(_projectParameter.Id, ((int)Math.Round(clampedValue, 0)).ToString(CultureInfo.InvariantCulture));
                    break;
                case MagnumProjectParameterType.Float:
                case MagnumProjectParameterType.CritDamage:
                case MagnumProjectParameterType.WeaponAccuracy:
                case MagnumProjectParameterType.WeaponScatterAngle:
                    project.AppliedModifications.Add(_projectParameter.Id, clampedValue.ToString(CultureInfo.InvariantCulture));
                    break;
                case MagnumProjectParameterType.ResistBlunt:
                case MagnumProjectParameterType.ResistPierce:
                case MagnumProjectParameterType.ResistLacer:
                case MagnumProjectParameterType.ResistFire:
                case MagnumProjectParameterType.ResistBeam:
                case MagnumProjectParameterType.ResistShock:
                case MagnumProjectParameterType.ResistPoison:
                case MagnumProjectParameterType.ResistCold:
                    project.AppliedModifications.Add(_projectParameter.Id, clampedValue.ToString(CultureInfo.InvariantCulture));
                    break;
                default:
                    _logger.Log($"unknown parameter type {_projectParameter.ParameterType}");
                    return;
            }
        }

        public float CalculateParamValue(float defaultValue, ItemRarity rarity, bool increase, bool boost, bool isResist,
            float averageResist,
            bool averageResistApplied,
            out bool averageResistAppliedResult,
            bool hinder,
            bool rarityExtraBoost
            )
        {
            float modifier = GetRarityModifier(rarity, _rarityModifiers);
            float modifierExtraBoost = GetRarityModifier(rarity, _rarityModifiers);
            if (!rarityExtraBoost)
            {
                modifierExtraBoost = 1f;
            }

            averageResistAppliedResult = false;

            // Since armor reistances gotta be higher and this is the only thing we can do for armor.
            if (isResist && defaultValue == 0)
            {
                if (averageResistApplied == false)
                {
                    // Roll random
                    var canApply = Helpers._random.Next(0, 100 + 1) < AVERAGE_RESIST_APPLY_CHANCE;
                    if (canApply)
                    {
                        averageResistAppliedResult = true;
                        _logger.Log($"---");
                        _logger.Log($"\t\t\t Resist with defaultValue {defaultValue}, setting to {averageResist} (averageResist)");
                        defaultValue = averageResist;
                    }
                }
                else
                {
                    _logger.Log($"\t\t Resist with defaultValue {defaultValue}, setting to {averageResist} (averageResist) already applied. SKIPPING.");
                }

            }
            else if (isResist && defaultValue != 0)
            {
                _logger.Log($"\t\t Resist with defaultValue {defaultValue}, SKIPPING AND NOT setting to {averageResist}");
            }

            float result = 0;
            // float boostAmount = (float)Math.Round(Helpers._random.Next((int)(PARAMETER_BOOST_MIN * 100), (int)(PARAMETER_BOOST_MAX * 100) + 1) / 100f, 2);
            float boostAmount = boost == true ? (float)Math.Round(Helpers._random.NextDouble() * (PARAMETER_BOOST_MAX - PARAMETER_BOOST_MIN) + PARAMETER_BOOST_MIN, 2) : 1;

            _logger.Log($"\t\t Modifier: {modifier}, modifierExtraBoost: {modifierExtraBoost}, boosting: {boost}, boostAmount: {boostAmount}, hinder: {hinder}");

            if (hinder)
            {
                increase = !increase;
            }

            if (increase)
            {
                result = (defaultValue * modifier) * boostAmount * modifierExtraBoost;
            }
            else
            {
                result = (defaultValue / modifier) / boostAmount / modifierExtraBoost;
            }

            //_logger.Log($"\t\t\t Result: {result}");

            return result;
        }

        internal float GetRarityModifier<T>(T rarity, Dictionary<T, (float, float)> modifiers) where T : Enum
        {
            var (Min, Max) = modifiers[rarity];
            float modifier = (float)Math.Round(Helpers._random.NextDouble() * (Max - Min) + Min, 2);
            return modifier;
        }

        [Obsolete]
        internal int ApplyProjectParameters(ref MagnumProject magnumProject, ItemRarity itemRarity, bool rarityExtraBoost)
        {
            var editableParameters = GetEditableParameters(magnumProject.ProjectType);

            var (Min, Max) = rarityParamPercentages[itemRarity];
            int minParams = Math.Max(0, (int)Math.Floor(Min * editableParameters.Count));
            int maxParams = (int)Math.Ceiling(Max * editableParameters.Count);

            // Calculate the number of parameters to adjust based on the percentage
            int numParamsToAdjust = Helpers._random.Next(minParams, maxParams + 1);

            int numParamsToHinder = (int)Math.Floor(numParamsToAdjust * PARAMETER_HINDER_PERCENT / 100f); // 20% of adjusted parameters to hinder
            int numParamsToImprove = numParamsToAdjust - numParamsToHinder;

            //// Get _defaultValue for randomized Prefix
            //var randomPrefix = Helpers._random.Next(0, AMOUNT_PREFIXES);

            // Shuffle the dictionary
            var editableParamsShuffled = ShuffleDictionary(editableParameters);

            // Get average resist for armor.
            float averageResist = 0;
            int resistCount = 0;

            _logger.Log($"\n\n#FF0000 ApplyProjectParameters");
            _logger.Log($" {magnumProject.ProjectType}");
            _logger.Log($"\t {magnumProject.DevelopId}");
            _logger.Log($"\t\t Rarity: {itemRarity}");

            if (
                magnumProject.ProjectType == MagnumProjectType.Armor ||
                magnumProject.ProjectType == MagnumProjectType.Helmet ||
                magnumProject.ProjectType == MagnumProjectType.Boots ||
                magnumProject.ProjectType == MagnumProjectType.Leggings
              )
            {
                _logger.Log($"\t\t\t Getting average resistances:");

                foreach (var param in editableParameters.Values)
                {
                    if (param.Id.Contains("_resist"))
                    {
                        //_logger.Log($"\t\t Resist: {param.Id}");

                        var _defaultValue = magnumProject.GetParameterDefaultValue(param);

                        if (_defaultValue != null)
                        {
                            float value = Convert.ToSingle(_defaultValue);

                            // If there is only one resist, it leads to imbalance as it can get applied to others.
                            //if (value > 0)
                            //{
                            averageResist += value;
                            resistCount++;
                            //}
                        }
                    }
                }

                averageResist = (float)Math.Round(averageResist / resistCount, 2);
                averageResist = Math.Max(averageResist, 1.0f); // Ensure average resist is at least 1.0
                _logger.Log($"\t\t\t\t Average resist {averageResist} for total count {resistCount}");
            }

            // Select one parameter to boost more.
            // This parameter will be boosted more than the others.
            var boostedParam = editableParamsShuffled.Values.ToArray()[Helpers._random.Next(editableParamsShuffled.Count)];

            string[] boostedParamParts = boostedParam.Id.Split('_');
            string boostedParamResult = string.Join("_", boostedParamParts.Skip(1));

            // We return index of parameter that was boosted for UID
            int boostedParamIndex = Math.Min(ParamIdentifiers.IndexOf(boostedParamResult), 99); // Just so 99 means not found.

            bool hinder = false;

            // Counters to track how many parameters we've improved or hindered
            int improvedCount = 0;
            int hinderedCount = 0;

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

                // Determine if we should hinder this parameter
                hinder = ShouldHinderParameter(ref hinderedCount, ref improvedCount, numParamsToHinder, numParamsToImprove);

                if (new[] { "_weight", "_reload_duration", "_scatter_angle" }.Any(defaultParamValue.Id.Contains))
                {
                    AddIncreasedOrDecreased(defaultParamValue, ref magnumProject, itemRarity, false, boostedParam, 0, hinder, rarityExtraBoost);
                }
                else if (new[] { "_resist", "_damage", "_crit_damage", "_max_durability", "_accuracy", "_magazine_capacity" }
                         .Any(defaultParamValue.Id.Contains))
                {
                    AddIncreasedOrDecreased(defaultParamValue, ref magnumProject, itemRarity, true, boostedParam, averageResist, hinder, rarityExtraBoost);
                }
                else if (defaultParamValue.Id.Contains("_special_ability"))
                {
                    // Nothing we got here for now.
                }
            }

            return boostedParamIndex;
        }

        public bool ShouldHinderParameter(ref int hinderedCount, ref int improvedCount, int numParamsToHinder, int numParamsToImprove)
        {
            // 20% chance to hinder first, regardless of improvement status
            if (Helpers._random.Next(0, 100 + 1) < 20)
            {
                if (hinderedCount < numParamsToHinder)
                {
                    hinderedCount++;
                    return true;
                }
                else
                {
                    // Can't hinder anymore, so improve
                    improvedCount++;
                    return false;
                }
            }

            // After 20% chance, follow the original logic
            if (hinderedCount < numParamsToHinder && improvedCount >= numParamsToImprove)
            {
                // 50/50 chance to hinder a parameter (after improvement threshold is met)
                if (Helpers._random.Next(0, 100 + 1) < PARAMETER_HINDER_CHANCE)
                {
                    hinderedCount++;
                    return true;
                }
                else
                {
                    improvedCount++;
                    return false;
                }
            }
            else if (improvedCount < numParamsToImprove)
            {
                // Otherwise improve as usual
                improvedCount++;
                return false;
            }

            return false;
        }

        public int GetTraitCountByRarity(ItemRarity rarity, int maxTraits)
        {
            var (Min, Max) = rarityTraitRanges[rarity];
            int minTraits = Math.Max(0, (int)Math.Floor(Min * maxTraits));
            int maxTraitsInclusive = (int)Math.Ceiling(Max * maxTraits);

            // For fixed values, min and max traits will be the same
            return Helpers._random.Next(minTraits, maxTraitsInclusive + 1);
        }

        // Traits
        [Obsolete]
        private void ApplyTraits(ref BasePickupItem item, ItemRarity itemRarity, ItemTraitType itemTraitType, CompositeItemRecord compositeItemRecord)
        {
            var traitsForItemType = GetAddeableTraits(itemTraitType);
            var traitsForItemTypeShuffled = ShuffleDictionary(traitsForItemType);

            // Determine if the item is a melee weapon
            bool isMelee = false;

            var record = item.Record<WeaponRecord>();
            _logger.Log($"\t\t  WeaponRecord Exist: {record != null}");

            if (record != null)
            {
                isMelee = record.IsMelee;
            }
            _logger.Log($"\t\t  isMelee: {isMelee}");

            // Apply traits blacklist
            if (isMelee)
            {
                for (int i = traitsForItemTypeShuffled.Count - 1; i >= 0; i--)
                {
                    var key = traitsForItemTypeShuffled.ElementAt(i).Value.Id;
                    if (meleeTraitsBlacklist.Contains(key))
                    {
                        _logger.Log($"[RaritySystem] Removing key '{key}' from traitsForItemTypeShuffled as it's in the meleeTraitsBlacklist.");
                        traitsForItemTypeShuffled.Remove(key);
                    }
                }
            }
            else
            {
                for (int i = traitsForItemTypeShuffled.Count - 1; i >= 0; i--)
                {
                    var key = traitsForItemTypeShuffled.ElementAt(i).Value.Id;
                    if (rangedTraitsBlacklist.Contains(key))
                    {
                        _logger.Log($"[RaritySystem] Removing key '{key}' from traitsForItemTypeShuffled as it's in the rangedTraitsBlacklist.");
                        traitsForItemTypeShuffled.Remove(key);
                    }
                }
            }

            var traitCount = GetTraitCountByRarity(itemRarity, traitsForItemType.Count);

            // Calculate the number of traits to adjust based on the percentage
            int numParamsToAdjust = Mathf.Max(1, traitCount); // Ensure at least 1 parameter is adjusted

            var canAddUnbreakableTrait = false;

            // Only 20% of all items are eligible for unbreakable trait
            if (Helpers._random.NextDouble() <= UNBREAKABLE_ENTRY_CHANCE &&
                unbreakableTraitPercent.TryGetValue(itemRarity, out float weight) &&
                weight > 0)
            {
                // Get the list of eligible rarities and their weights
                var eligibleRarities = unbreakableTraitPercent
                    .Where(kv => kv.Value > 0)
                    .ToDictionary(kv => kv.Key, kv => kv.Value);

                // Calculate total weight among eligible rarities
                float totalWeight = eligibleRarities.Values.Sum();

                // Check if this specific item wins based on its weight
                if (Helpers._random.NextDouble() * totalWeight <= weight)
                {
                    canAddUnbreakableTrait = true;
                }
            }

            _logger.Log($"\t\t  Unbreakable: {canAddUnbreakableTrait}");

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

        [Obsolete]
        internal void ApplyTraits(ref BasePickupItem item)
        {
            var metadata = RecordCollection.MetadataWrapperRecords.GetOrAdd(item.Id, MetadataWrapper.SplitItemUid);

            // We have that item in list so we need to process it and remove later on.
            CompositeItemRecord compositeItemRecord = Data.Items.GetRecord(item.Id, true) as CompositeItemRecord;

            foreach (BasePickupItemRecord basePickupItemRecord in compositeItemRecord.Records)
            {
                Type recordType = basePickupItemRecord.GetType();

                switch (recordType.Name)
                {
                    case nameof(WeaponRecord):
                        ApplyTraits(ref item, metadata.RarityClass, ItemTraitType.WeaponTrait, compositeItemRecord);
                        break;
                    case nameof(ArmorRecord):
                    case nameof(HelmetRecord):
                    case nameof(LeggingsRecord):
                    case nameof(BootsRecord):
                        ApplyTraits(ref item, metadata.RarityClass, ItemTraitType.ArmorTrait, compositeItemRecord);
                        break;
                        //case nameof(AmmoRecord):
                        //    ApplyTraits(ref item, metadata.RarityClass, ItemTraitType.ArmorTrait, compositeItemRecord);
                        break;
                    default:
                        break;
                }
            }
        }

        [Obsolete]
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

        [Obsolete]
        private static Dictionary<string, MagnumProjectParameter> GetEditableParameters(MagnumProjectType projectType)
        {
            // Get magnum_projects_params that we can edit for that projectType
            Dictionary<string, MagnumProjectParameter> editable_magnum_projects_params = new Dictionary<string, MagnumProjectParameter>();

            // Iterate whole list of record to get what we need.
            foreach (var param in Data.MagnumProjectParameters._records)
            {
                // _logger.Log($"\t\t record: {param.Key}"); // record: rangeweapon_damage
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

        public void ShuffleList<T>(IList<T> list)
        {
            // Fisher-Yates shuffle
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Helpers._random.Next(i + 1);
                T temp = list[i];
                list[i] = list[j];
                list[j] = temp;
            }
        }

        public Dictionary<TKey, TValue> ShuffleDictionary<TKey, TValue>(Dictionary<TKey, TValue> dictionary)
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

        internal static void AddAffixes(string itemId)
        {
            if (!RecordCollection.MetadataWrapperRecords.TryGetValue(itemId, out MetadataWrapper metadata))
            {
                if (MetadataWrapper.IsPoqItemUid(itemId))
                {
                    Plugin.Logger.LogWarning($"AddAffixes: trying to get poq item but record is missing.");
                }
            }
            else
            {
                metadata = RecordCollection.MetadataWrapperRecords.GetOrAdd(itemId, MetadataWrapper.SplitItemUid);

                if (metadata.RarityClass == ItemRarity.Standard)
                {
                    return;
                }

                var affix = AffixManager.GetAffix(metadata.RarityClass, itemId);

                if (affix == null)
                {
                    return;
                }

                UpdateKey("item." + metadata.ReturnItemUid() + ".name",
                    affix[0].Text, "",
                    metadata.ReturnItemUid(true));

                UpdateKey("item." + metadata.ReturnItemUid() + ".shortdesc",
                    "", affix[1].Text,
                    metadata.ReturnItemUid(true));
            }
        }

        [Obsolete]
        internal static void AddAffixes(MagnumProject magnumProject)
        {
            // Add affixes for localization data.
            // English as of time being.

            var wrapper = new MetadataWrapper(magnumProject);

            if (wrapper.PoqItem)
            {
                _logger.LogWarning($"AddAffixes for {wrapper.ReturnItemUid()}, PoqItem {wrapper.PoqItem}.");

                var digitInfo = DigitInfo.GetDigits(magnumProject.FinishTime.Ticks);
                var affix = AffixManager.GetAffix(wrapper.RarityClass, magnumProject, digitInfo.BoostedParam);

                // We got id.name now.
                if (affix == null || affix.Count != 2)
                {
                    _logger.LogWarning($"AddAffixes failed. Nothing was found.");
                    _logger.LogWarning($"\t\t affix == null {affix == null}");
                    _logger.LogWarning($"\t\t affix.Count {affix?.Count}");

                    return;
                }

                // Add our item language keys.
                // We do it here because this method fires earlier than we actually inject item record.
                //// Since Localization.DuplicateKey just copies key and nothing else, it will do same in inject item record method.

                //Localization.DuplicateKey("item." + metadata.Id + ".name", "item." + metadata.ReturnItemUid() + ".name");
                //Localization.DuplicateKey("item." + metadata.Id + ".shortdesc", "item." + metadata.ReturnItemUid() + ".shortdesc");

                //_logger.LogWarning($"Updating {affix[0].Text} and {affix[1].Text} for {metadata.ReturnItemUid()}");

                // Problem, on game load it doesn't have effect.
                UpdateKey("item." + wrapper.ReturnItemUid() + ".name",
                    affix[0].Text, "",
                    wrapper.ReturnItemUid(true));
                UpdateKey("item." + wrapper.ReturnItemUid() + ".shortdesc",
                    "", affix[1].Text,
                    wrapper.ReturnItemUid(true));
            }
        }

        private static void UpdateKey(string lookupItemId, string prefix, string suffix, string originalUid)
        {
            foreach (KeyValuePair<Localization.Lang, Dictionary<string, string>> languageToDict in Singleton<Localization>.Instance.db)
            {
                Localization.Lang lang = languageToDict.Key;

                var originalName = Localization.Get("item." + originalUid + ".name", lang);
                var originalShortdesc = Localization.Get("item." + originalUid + ".shortdesc", lang);

                // Get prefix and suffix based on lang
                string _prefix = Localization.Get(prefix, lang);
                string _suffix = Localization.Get(suffix, lang);

                if (languageToDict.Value.ContainsKey(lookupItemId))
                {
                    // Update prefix
                    if (lookupItemId.Contains(".name"))
                    {
                        languageToDict.Value[lookupItemId] = $"{_prefix} {originalName}";
                    }

                    // Update suffix
                    if (lookupItemId.Contains(".shortdesc"))
                    {
                        languageToDict.Value[lookupItemId] = $"{originalShortdesc} {_suffix}";
                    }
                }
                else
                {
                    _logger.LogWarning($"UpdateKey issue. No key {lookupItemId}");
                }
            }
        }

        public void Apply<T>(Action<T> setter, Func<T> getter, float modifier, bool increase, out float oldValue, out float newValue) where T : struct // ensure T is a non-nullable value type
        {
            T value = getter();
            oldValue = Convert.ToSingle(value); // Safe cast to float
            ApplyModifier(ref value, modifier, increase, out _, out newValue);
            setter(value);
        }

        public void ApplyModifier<T>(ref T value, float finalModifier, bool increase, out float outOldValue, out float outNewValue) where T : struct // ensure T is a non-nullable value type
        {
            outOldValue = (float)Convert.ChangeType(value, typeof(float));

            float tempValue = outOldValue;
            tempValue = increase ? tempValue * finalModifier : tempValue / finalModifier;

            if (typeof(T) == typeof(int))
            {
                //tempValue = (float)Math.Ceiling(tempValue);
                // While is most cases it'ok to have 1.01 become two but i need custom rounding
                // Custom rounding: 1.3 or higher rounds up, below stays down
                tempValue = Helpers.CustomRound(tempValue, 0.7f);
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

        internal void ApplyColors()
        {
            Colors[ItemRarity.Standard] = Helpers.AlphaAwareColorToHex(Plugin.Config.RarityColor_Standard);
            Colors[ItemRarity.Enhanced] = Helpers.AlphaAwareColorToHex(Plugin.Config.RarityColor_Enhanced);
            Colors[ItemRarity.Advanced] = Helpers.AlphaAwareColorToHex(Plugin.Config.RarityColor_Advanced);
            Colors[ItemRarity.Premium] = Helpers.AlphaAwareColorToHex(Plugin.Config.RarityColor_Premium);
            Colors[ItemRarity.Prototype] = Helpers.AlphaAwareColorToHex(Plugin.Config.RarityColor_Prototype);
            Colors[ItemRarity.Quantum] = Helpers.AlphaAwareColorToHex(Plugin.Config.RarityColor_Quantum);
        }
    }
}
