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
    public class RecombinatorController
    {
        public static float DROP_CHANCE = 100;
        public enum RecombinatorType
        {
            WeaponTraits,
            WeaponRandoms,
            Ammo,
            Indestructible
        }

        public static string nameBase = "recombinator_poq_";

        public static void AddRecombinators()
        {
            // Add our own custom items.
            foreach (RecombinatorType type in Enum.GetValues(typeof(RecombinatorType)))
            {
                CreateRecombinator(type);
            }
        }

        public static bool IsRecombinator(BasePickupItem item)
        {
            return IsRecombinator(item.Id);
        }

        public static bool IsRecombinator(string item)
        {
            if (item.Contains(nameBase))
            {
                return true;
            }

            return false;
        }

        private static void CreateRecombinator(RecombinatorType type)
        {
            var itemId = $"{nameBase}{type.ToString().ToLower()}";

            CompositeItemRecord compositeItemRecord = new CompositeItemRecord(itemId);

            var record = new RecombinatorRecord();
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
            record.RecombinatorType = type;

            RepairDescriptor descriptor = ScriptableObject.CreateInstance("RepairDescriptor") as RepairDescriptor;
            descriptor._icon = Helpers.LoadSpriteFromEmbeddedBundle("QM_PathOfQuasimorph.Files.AssetBundles.pathofquasimorph", "amplifier_icon");
            descriptor._smallIcon = Helpers.LoadSpriteFromEmbeddedBundle("QM_PathOfQuasimorph.Files.AssetBundles.pathofquasimorph", "amplifier_icon");
            descriptor.name = itemId;

            record.ContentDescriptor = descriptor;

            compositeItemRecord.Records.Add(record);

            Data.Items._records.Add(itemId, compositeItemRecord);

            Localization.DuplicateKey($"item.{nameBase}.shortdesc", "item." + itemId + ".shortdesc");
        }

        internal string GetAmplifierNameFromRarity(ItemRarity itemRarity)
        {
            return $"{nameBase}{itemRarity.ToString().ToLower()}";
        }
    }
}
