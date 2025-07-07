using MGSC;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static MGSC.Localization;
using static QM_PathOfQuasimorph.Core.MagnumPoQProjectsController;

namespace QM_PathOfQuasimorph.Core
{
    public enum AffixType
    {
        Prefix,
        Suffix
    }

    public enum AffixCategory
    {
        Weapon,
        Armor,
        None
    }
    public enum AffixRarityType
    {
        Standard,
        Enhanced,
        Advanced,
        Premium,
        Prototype,
        Quantum,
    }

    internal class AffixManager
    {
        public static Affix[] affixes;

        public AffixManager()
        {
            affixes = LoadAffixesFromEmbeddedCSV();
        }

        Affix[] CreateAffixes()
        {
            return new Affix[]
            {
            };
        }

        // Might need a good CSV reader lib, but while it works...
        Affix[] LoadAffixesFromCSV(string filePath)
        {
            List<Affix> affixes = new List<Affix>();
            string[] lines = File.ReadAllLines(filePath);

            foreach (string line in lines)
            {
                string[] parts = line.Split(',');
                if (parts.Length == 5)
                {
                    affixes.Add(new Affix(parts[0], parts[1], parts[2], parts[3], parts[4], parts[5]));
                }
            }

            return affixes.ToArray();
        }

        Affix[] LoadAffixesFromEmbeddedCSV(string resourceName = "QM_PathOfQuasimorph.Files.Affixes.csv")
        {
            List<Affix> affixes = new List<Affix>();

            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] parts = line.Split(',');
                    if (parts.Length == 6) // This is important not to overlook
                    {
                        affixes.Add(new Affix(parts[0], parts[1], parts[2], parts[3], parts[4], parts[5]));
                    }
                }
            }
            return affixes.ToArray();
        }

        internal static List<Affix> GetAffix(ItemRarity rarityClass, MagnumProject magnumProject, int BoostedParam)
        {
            var affixRarityTypeToLookFor = (AffixRarityType)rarityClass;
            AffixCategory affixCategoryLookFor = AffixCategory.None;

            var affixesList = new List<Affix>();

            string itemClass = string.Empty;

            Plugin.Logger.Log($"ProjectType: {magnumProject.ProjectType}, RarityClass: {rarityClass}");

            try
            {
                switch (magnumProject.ProjectType)
                {
                    case MagnumProjectType.RangeWeapon:
                        affixCategoryLookFor = AffixCategory.Weapon;
                        itemClass = Data.Items.GetSimpleRecord<WeaponRecord>(magnumProject.DevelopId, true).WeaponSubClass.ToString();
                        break;
                    case MagnumProjectType.MeleeWeapon:
                        affixCategoryLookFor = AffixCategory.Weapon;
                        itemClass = Data.Items.GetSimpleRecord<WeaponRecord>(magnumProject.DevelopId, true).WeaponClass.ToString();
                        break;
                    case MagnumProjectType.Armor:
                        affixCategoryLookFor = AffixCategory.Armor;
                        itemClass = Data.Items.GetSimpleRecord<ArmorRecord>(magnumProject.DevelopId, true).ArmorClass.ToString();
                        break;
                    case MagnumProjectType.Helmet:
                        affixCategoryLookFor = AffixCategory.Armor;
                        itemClass = Data.Items.GetSimpleRecord<HelmetRecord>(magnumProject.DevelopId, true).ArmorClass.ToString();
                        break;
                    case MagnumProjectType.Boots:
                        affixCategoryLookFor = AffixCategory.Armor;
                        itemClass = Data.Items.GetSimpleRecord<BootsRecord>(magnumProject.DevelopId, true).ArmorClass.ToString();
                        break;
                    case MagnumProjectType.Leggings:
                        affixCategoryLookFor = AffixCategory.Armor;
                        itemClass = Data.Items.GetSimpleRecord<LeggingsRecord>(magnumProject.DevelopId, true).ArmorClass.ToString();
                        break;
                    default:
                        Plugin.Logger.Log($"Unknown ProjectType: {magnumProject.ProjectType}, returning null.");
                        return null;
                }
            }
            catch
            {
                Plugin.Logger.Log($"Unknown null record, returning null.");
                return null;
            }

            if (affixCategoryLookFor == AffixCategory.None)
            {
                Plugin.Logger.Log("AffixCategory is None, returning null.");
                return null;
            }

            Plugin.Logger.Log($"Selected AffixCategory: {affixCategoryLookFor}, AffixRarity: {affixRarityTypeToLookFor}, Class: {itemClass}");
            Plugin.Logger.Log($"affixes.Length {affixes.Length}");

            if (affixes.Length > 0)
            {
                // Filter affixes by rarity and project type and prefixes, suffixes
                var matchingPrefixes = affixes
                    .Where(affix => affix.AffixCategory == affixCategoryLookFor &&
                           affix.AffixType == AffixType.Prefix &&
                           affix.AffixRarityType == affixRarityTypeToLookFor &&
                           affix.Class == itemClass)
                    .ToList();

                var matchingSuffixes = affixes
                    .Where(affix => affix.AffixRarityType == affixRarityTypeToLookFor &&
                                    affix.AffixCategory == affixCategoryLookFor &&
                                    affix.AffixType == AffixType.Suffix &&
                                    affix.Parameter == RaritySystem.ParamIdentifiers[BoostedParam]
                                    )
                    .ToList();

                Plugin.Logger.Log($"MatchingPrefixes count: {matchingPrefixes.Count}, MatchingSuffixes count: {matchingSuffixes.Count}");

                if (matchingPrefixes.Count >= 1 && matchingSuffixes.Count >= 1)
                {
                    affixesList.Add(matchingPrefixes[0]);
                    Plugin.Logger.Log($"Added Prefix: {matchingPrefixes[0]}");
                    affixesList.Add(matchingSuffixes[0]); // There will be only one suffix anyway.
                    Plugin.Logger.Log($"Added Suffix: {matchingSuffixes[0]}");

                    return affixesList;
                }

                Plugin.Logger.Log("No matching affixes found.");
            }

            return null; // Return null if no matching affix is found
        }
    }

    internal class Affix
    {
        public AffixType AffixType { get; set; }
        public AffixCategory AffixCategory { get; set; }
        public AffixRarityType AffixRarityType { get; set; }
        public string Class { get; set; }
        public string Text { get; set; }
        public string Parameter { get; set; }

        public Affix(string affixCategory, string affixType, string affixRarityType, string _class, string parameter, string text)
        {
            this.AffixCategory = (AffixCategory)Enum.Parse(typeof(AffixCategory), affixCategory, true);
            this.AffixType = (AffixType)Enum.Parse(typeof(AffixType), affixType, true);
            this.AffixRarityType = (AffixRarityType)Int32.Parse(affixRarityType);
            this.Class = _class;
            this.Text = text;
            this.Parameter = parameter;
        }
    }
}
