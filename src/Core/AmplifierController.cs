using MGSC;
using QM_PathOfQuasimorph.Core.Records;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace QM_PathOfQuasimorph.Core
{
    internal class AmplifierController
    {
        public static float DROP_CHANCE = 100;
        public static string nameBase = "amplifier_poq_";

        public static void AddAmplifiers()
        {
            // Add our own custom items.
            foreach (ItemRarity rarity in Enum.GetValues(typeof(ItemRarity)))
            {
                CreateAmplifier(rarity);
            }
        }

        public static bool IsAmplifier(BasePickupItem item)
        {
            return IsAmplifier(item.Id);
        }

        public static bool IsAmplifier(string item)
        {
            if (item.Contains(nameBase))
            {
                return true;
            }

            return false;
        }

        private static void CreateAmplifier(ItemRarity rarity)
        {
            var itemId = $"{nameBase}{rarity.ToString().ToLower()}";

            CompositeItemRecord compositeItemRecord = new CompositeItemRecord(itemId);

            var record = new AmplifierRecord();
            record.Id = itemId;
            record.Categories = new List<string>();
            record.TechLevel = 1;
            record.Price = 0;
            record.Weight = 0;
            record.InventoryWidthSize = 1;
            record.ItemClass = ItemClass.Parts;
            record.RepairSpecialRule = RepairSpecialRule.All;
            record.MaxStack = 100;
            record.UsageCost = 1;
            record.MaxUsage = 1;
            record.Rarity = rarity;

            RepairDescriptor descriptor = ScriptableObject.CreateInstance("RepairDescriptor") as RepairDescriptor;
            descriptor._icon = Helpers.LoadSpriteFromEmbeddedBundle("QM_PathOfQuasimorph.Files.AssetBundles.pathofquasimorph", "amplifier_icon");
            descriptor._smallIcon = Helpers.LoadSpriteFromEmbeddedBundle("QM_PathOfQuasimorph.Files.AssetBundles.pathofquasimorph", "amplifier_icon");
            descriptor.name = itemId;

            record.ContentDescriptor = descriptor;

            compositeItemRecord.Records.Add(record);

            Data.Items._records.Add(itemId, compositeItemRecord);

            Localization.DuplicateKey($"item.amplifier_poq.shortdesc", "item." + itemId + ".shortdesc");
        }

        internal string GetAmplifierNameFromRarity(ItemRarity itemRarity)
        {
            return $"{nameBase}{itemRarity.ToString().ToLower()}";
        }
    }
}
