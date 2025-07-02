using MGSC;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
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
                    affixes.Add(new Affix(parts[0], parts[1], parts[2], parts[3], parts[4]));
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
                    if (parts.Length == 5)
                    {
                        affixes.Add(new Affix(parts[0], parts[1], parts[2], parts[3], parts[4]));
                    }
                }
            }
            return affixes.ToArray();
        }

        internal static List<Affix> GetAffix(ItemRarity rarityClass, MagnumProjectType projectType, int BoostedParam, int RandomizedPrefix)
        {
            var affixRarityTypeToLookFor = (AffixRarityType)rarityClass;
            AffixCategory affixCategoryLookFor = AffixCategory.None;

            var affixesList = new List<Affix>();

            switch (projectType)
            {
                case MagnumProjectType.RangeWeapon:
                case MagnumProjectType.MeleeWeapon:
                    affixCategoryLookFor = AffixCategory.Weapon;
                    break;
                case MagnumProjectType.Armor:
                case MagnumProjectType.Helmet:
                case MagnumProjectType.Boots:
                case MagnumProjectType.Leggings:
                    affixCategoryLookFor = AffixCategory.Armor;
                    break;
                default:
                    break;
            }

            if (affixCategoryLookFor == AffixCategory.None)
            {
                return null;
            }

            if (affixes.Length > 0)
            {
                // Filter affixes by rarity and project type and prefixes, suffixes
                var matchingPrefixes = affixes
                    .Where(affix => affix.AffixRarityType == affixRarityTypeToLookFor &&
                                    affix.AffixCategory == affixCategoryLookFor &&
                                    affix.AffixType == AffixType.Prefix)
                    .ToList();

                var matchingSuffixes = affixes
                .Where(affix => affix.AffixRarityType == affixRarityTypeToLookFor &&
                                affix.AffixCategory == affixCategoryLookFor &&
                                affix.AffixType == AffixType.Suffix &&
                                affix.Parameter == RaritySystem.ParamIdentifiers[BoostedParam]
                                )
                .ToList();

                if (matchingPrefixes.Count > 1 && matchingSuffixes.Count >= 1)
                {
                    affixesList.Add(matchingPrefixes[RandomizedPrefix]);
                    affixesList.Add(matchingSuffixes[0]); // There will be only one suffix anyway.

                    return affixesList;
                }
            }

            return null; // Return null if no matching affix is found
        }
    }

    internal class Affix
    {
        public AffixType AffixType { get; set; }
        public AffixCategory AffixCategory { get; set; }
        public AffixRarityType AffixRarityType { get; set; }
        public string Text { get; set; }
        public string Parameter { get; set; }

        public Affix(string affixCategory, string affixType, string affixRarityType, string text, string parameter)
        {
            this.AffixCategory = (AffixCategory)Enum.Parse(typeof(AffixCategory), affixCategory, true);
            this.AffixType = (AffixType)Enum.Parse(typeof(AffixType), affixType, true);
            this.AffixRarityType = (AffixRarityType)Int32.Parse(affixRarityType);
            this.Text = text;
            this.Parameter = parameter;
        }
    }
}
