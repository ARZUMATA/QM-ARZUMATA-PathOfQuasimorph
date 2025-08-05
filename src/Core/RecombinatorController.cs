using MGSC;
using QM_PathOfQuasimorph.Core.Processors;
using QM_PathOfQuasimorph.Core.Records;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using static QM_PathOfQuasimorph.Core.CreaturesControllerPoq;

namespace QM_PathOfQuasimorph.Core
{
    public class RecombinatorController
    {
        public static float DROP_CHANCE = 100;
        private static Logger _logger = new Logger(null, typeof(RecombinatorController));
        public enum RecombinatorType
        {
            WeaponTraits,
            WeaponRequiredAmmo,
            AmmoTraits,
            WeaponRandoms,
            Indestructible
        }

        public static string nameBase = "recombinator_poq";
        private static Sprite[] sprites = Helpers.LoadSpritesFromEmbeddedBundle("QM_PathOfQuasimorph.Files.AssetBundles.pathofquasimorph");
        public static void AddRecombinators()
        {
            // Add our own custom items.
            foreach (RecombinatorType type in Enum.GetValues(typeof(RecombinatorType)))
            {
                CreateRecombinator(type, (int)type);
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

        private static void CreateRecombinator(RecombinatorType type, int index)
        {
            var itemId = $"{nameBase}_{index}";

            CompositeItemRecord compositeItemRecord = new CompositeItemRecord(itemId);

            var record = new RecombinatorRecord();
            record.Id = itemId;
            record.Categories = new List<string>();
            record.TechLevel = 1;
            record.Price = 0;
            record.Weight = 0;
            record.InventoryWidthSize = 1;
            record.ItemClass = ItemClass.Parts;

            record.RepairSpecialRule = RepairSpecialRule.None;

            switch (type)
            {
                case RecombinatorType.WeaponTraits:
                    record.RepairSpecialRule = RepairSpecialRule.AllWeapons;
                    break;
                case RecombinatorType.WeaponRequiredAmmo:
                    record.RepairSpecialRule = RepairSpecialRule.AllWeapons;
                    break;
            }

            record.MaxStack = 100;
            record.UsageCost = 1;
            record.MaxUsage = 1;
            record.RecombinatorType = type;

            RepairDescriptor descriptor = ScriptableObject.CreateInstance("RepairDescriptor") as RepairDescriptor;
            Sprite icon = System.Array.Find(sprites, s => s.name == $"recombinator_icons_{index}");
            descriptor._icon = icon;
            descriptor._smallIcon = icon;
            descriptor.name = itemId;
            record.ContentDescriptor = descriptor;

            compositeItemRecord.Records.Add(record);

            Data.Items._records.Remove(itemId);
            Data.Items._records.Add(itemId, compositeItemRecord);

            //Localization.DuplicateKey($"item.{nameBase}_{index}.shortdesc", "item." + itemId + ".shortdesc");
            Localization.DuplicateKey($"item.{nameBase}.shortdesc", "item." + itemId + ".shortdesc");
        }
        public bool ApplyRecombinator(BasePickupItem target, BasePickupItem repair, ref bool __result)
        {
            RepairRecord repairRecord = repair.Record<RepairRecord>();
            Plugin.Logger.Log($"target.Is<RepairRecord>() {repair.Is<RepairRecord>()}");

            if (!repairRecord.IsValidCategory(target))
            {
                Plugin.Logger.Log($"Invalid category");

                // Do original method
                return true;
            }

            RecombinatorRecord recombinatorRecord = repair.Record<RecombinatorRecord>();

            Plugin.Logger.Log($"recombinatorRecord null {recombinatorRecord == null}");

            Plugin.Logger.Log($"RecombinatorType  {recombinatorRecord.RecombinatorType}");

            if (recombinatorRecord.RecombinatorType == RecombinatorType.WeaponTraits)
            {
                Plugin.Logger.Log($"ReplaceWeaponTraits Do");

                if (PathOfQuasimorph.recombinatorController.ProcessRecombinatorAction(target, repair, (recomb, meta, comp) => PathOfQuasimorph.itemRecordsControllerPoq.weaponRecordProcessorPoq.ReplaceWeaponTraits(recomb, meta, comp)))
                {
                    ItemInteractionSystem.ConsumeItem(repair);
                    __result = true;
                }
            }

            if (recombinatorRecord.RecombinatorType == RecombinatorType.WeaponRequiredAmmo)
            {
                Plugin.Logger.Log($"ReplaceRequiredAmmo Do");

                if (PathOfQuasimorph.recombinatorController.ProcessRecombinatorAction(target, repair, (recomb, meta, comp) => PathOfQuasimorph.itemRecordsControllerPoq.weaponRecordProcessorPoq.ReplaceRequiredAmmo(recomb, meta)))
                {
                    ItemInteractionSystem.ConsumeItem(repair);
                    __result = true;
                }
            }

            return false;
        }

        internal bool ProcessRecombinatorAction(BasePickupItem target, BasePickupItem repair,
            Action<RecombinatorRecord, MetadataWrapper, WeaponComponent> processorAction)
        {
            _logger.Log($"ProcessRecombinatorAction: Invoked");

            // Validate target & metadata
            if (!RecordCollection.MetadataWrapperRecords.TryGetValue(target.Id, out MetadataWrapper metadata) ||
                !metadata.PoqItem)
            {
                _logger.Log($"ProcessRecombinatorAction: Failed - Missing metadata or not PoqItem");
                return false;
            }

            // Get item record
            var obj = Data.Items.GetRecord(target.Id) as CompositeItemRecord;
            if (obj == null)
            {
                _logger.Log($"ProcessRecombinatorAction: Failed - No CompositeItemRecord found");
                return false;
            }

            var recombRecord = repair.Record<RecombinatorRecord>();

            foreach (var record in obj.Records)
            {
                var weaponRecord = record as WeaponRecord;
                var weaponComponent = target.Comp<WeaponComponent>();

                if (weaponRecord != null && weaponComponent != null)
                {
                    _logger.Log($"Processing weapon record with {processorAction.Method.Name}");

                    // Initialize processor (assumed common setup)
                    PathOfQuasimorph.itemRecordsControllerPoq.weaponRecordProcessorPoq.Init(weaponRecord, metadata.RarityClass, false, false, metadata.Id, metadata.ReturnItemUid());

                    // Execute the provided action (e.g., ReplaceWeaponTraits, ReplaceRequiredAmmo, etc.)
                    processorAction(recombRecord, metadata, weaponComponent);

                    return true;
                }
            }

            _logger.Log($"ProcessWeaponRecord: No valid weapon record/component found");
            return false;
        }
    }
}
