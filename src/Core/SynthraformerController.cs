using MGSC;
using QM_PathOfQuasimorph.Core.Records;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using UnityEngine;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

namespace QM_PathOfQuasimorph.Core
{
    public class SynthraformerController
    {
        public static float DROP_CHANCE = 80;
        public static string nameBase = "synthraformer_poq";
        private static Sprite[] sprites = Helpers.LoadSpritesFromEmbeddedBundle("QM_PathOfQuasimorph.Files.AssetBundles.pathofquasimorph");
        private static Logger _logger = new Logger(null, typeof(SynthraformerController));

        public enum SynthraformerType
        {
            Amplifier,
            Traits,
            RandomRarity,
            Indestructible,
        }

        public static void AddItems()
        {
            CreateItem(SynthraformerType.Amplifier);
            CreateItem(SynthraformerType.Traits);
            CreateItem(SynthraformerType.RandomRarity);
            CreateItem(SynthraformerType.Indestructible);
        }

        public static bool Is(BasePickupItem item)
        {
            if (item == null || item.Id == null)
            {
                return false;
            }

            return Is(item.Id);
        }

        public static bool Is(string item)
        {
            if (item.Contains(nameBase))
            {
                return true;
            }

            return false;
        }

        public string GetBaseDrop()
        {
            return $"{nameBase}_0";
        }

        private static void CreateItem(SynthraformerType type)
        {
            var itemId = $"{nameBase}_{(int)type}";

            CompositeItemRecord compositeItemRecord = new CompositeItemRecord(itemId);
            ItemProduceReceipt itemRecipe = new ItemProduceReceipt();
            itemRecipe.RequiredItems = new List<ItemQuantity>();

            var record = new SynthraformerRecord
            {
                Id = itemId,
                Categories = new List<string>(),
                TechLevel = 1,
                Price = 0,
                Weight = 0,
                InventoryWidthSize = 1,
                ItemClass = ItemClass.Parts,
                RepairSpecialRule = RepairSpecialRule.All,
                MaxStack = 100,
                UsageCost = 1,
                MaxUsage = 1,
                Type = type

            };
            switch (type)
            {
                case SynthraformerType.Amplifier:
                    //record.RepairSpecialRule = RepairSpecialRule.AllWeapons;
                    break;

                case SynthraformerType.Traits:
                    //record.RepairSpecialRule = RepairSpecialRule.AllWeapons;
                    itemRecipe.RequiredItems.Add(new ItemQuantity($"synthraformer_poq_{(int)SynthraformerType.Amplifier}", 5));
                    itemRecipe.OutputItem = $"synthraformer_poq_{(int)SynthraformerType.Traits}";
                    itemRecipe.ProduceTimeInHours = 2;
                    Data.ProduceReceipts.Add(itemRecipe);
                    break;

                case SynthraformerType.RandomRarity:
                    //record.RepairSpecialRule = RepairSpecialRule.AllWeapons;
                    itemRecipe.RequiredItems.Add(new ItemQuantity($"synthraformer_poq_{(int)SynthraformerType.Amplifier}", 10));
                    itemRecipe.OutputItem = $"synthraformer_poq_{(int)SynthraformerType.RandomRarity}";
                    itemRecipe.ProduceTimeInHours = 2;
                    Data.ProduceReceipts.Add(itemRecipe);
                    break;

                case SynthraformerType.Indestructible:
                    //record.RepairSpecialRule = RepairSpecialRule.AllWeapons;
                    itemRecipe.RequiredItems.Add(new ItemQuantity($"synthraformer_poq_{(int)SynthraformerType.Amplifier}", 10));
                    itemRecipe.OutputItem = $"synthraformer_poq_{(int)SynthraformerType.RandomRarity}";
                    itemRecipe.ProduceTimeInHours = 2;
                    Data.ProduceReceipts.Add(itemRecipe);
                    break;
            }

            RepairDescriptor descriptor = ScriptableObject.CreateInstance("RepairDescriptor") as RepairDescriptor;
            Sprite icon = System.Array.Find(sprites, s => s.name == $"synthraformer_poq_icons_{((int)type)}");

            descriptor._icon = icon;
            descriptor._smallIcon = icon;
            descriptor.name = itemId;
            record.ContentDescriptor = descriptor;

            compositeItemRecord.Records.Add(record);

            Data.Items._records.Remove(itemId);
            Data.Items._records.Add(itemId, compositeItemRecord);

            Localization.DuplicateKey($"item.{nameBase}.shortdesc", "item." + itemId + ".shortdesc");
        }

        public void Apply(BasePickupItem target, BasePickupItem repair, SynthraformerRecord record, ref bool __result)
        {
            Plugin.Logger.Log($"Apply");

            var targetItem = target as PickupItem;

            if (!RecordCollection.MetadataWrapperRecords.TryGetValue(targetItem.Id, out var metadata))
            {
                metadata = MetadataWrapper.SplitItemUid(targetItem.Id);
            }

            if (metadata.RarityClass == ItemRarity.Standard)
            {
                Plugin.Logger.Log($"Synthraformer Apply : metadata.RarityClass == ItemRarity.Standard");
                __result = CreateNewItem(targetItem, repair);
            }

            if (record.Type == SynthraformerType.Amplifier)
            {
                foreach (var basePickupItemRecord in targetItem.Records)
                {
                    WeaponRecord weaponRecord = basePickupItemRecord as WeaponRecord;

                    if (weaponRecord != null)
                    {
                        _logger.Log($"weaponRecord processing");
                        PathOfQuasimorph.itemRecordsControllerPoq.weaponRecordProcessorPoq.Init(weaponRecord, metadata.RarityClass, false, false, metadata.Id, metadata.ReturnItemUid());
                        PathOfQuasimorph.itemRecordsControllerPoq.weaponRecordProcessorPoq.RerollRandomStat(record, metadata);
                        __result = true;
                    }

                    HelmetRecord helmetRecord = basePickupItemRecord as HelmetRecord;

                    if (helmetRecord != null)
                    {
                        _logger.Log($"helmetRecord processing");
                        PathOfQuasimorph.itemRecordsControllerPoq.helmetRecordProcessorPoq.Init(helmetRecord, metadata.RarityClass, false, false, metadata.Id, metadata.ReturnItemUid());
                        PathOfQuasimorph.itemRecordsControllerPoq.helmetRecordProcessorPoq.RerollRandomStat(record, metadata);
                    }

                    ArmorRecord armorRecord = basePickupItemRecord as ArmorRecord;

                    if (armorRecord != null)
                    {
                        _logger.Log($"armorRecord processing");
                        PathOfQuasimorph.itemRecordsControllerPoq.armorRecordProcessorPoq.Init(armorRecord, metadata.RarityClass, false, false, metadata.Id, metadata.ReturnItemUid());
                        PathOfQuasimorph.itemRecordsControllerPoq.armorRecordProcessorPoq.RerollRandomStat(record, metadata);
                    }

                    LeggingsRecord leggingsRecord = basePickupItemRecord as LeggingsRecord;

                    if (leggingsRecord != null)
                    {
                        _logger.Log($"leggingsRecord processing");
                        PathOfQuasimorph.itemRecordsControllerPoq.leggingsRecordProcessorPoq.Init(leggingsRecord, metadata.RarityClass, false, false, metadata.Id, metadata.ReturnItemUid());
                        PathOfQuasimorph.itemRecordsControllerPoq.leggingsRecordProcessorPoq.RerollRandomStat(record, metadata);
                    }

                    BootsRecord bootsRecord = basePickupItemRecord as BootsRecord;

                    if (bootsRecord != null)
                    {
                        _logger.Log($"bootsRecord processing");
                        PathOfQuasimorph.itemRecordsControllerPoq.bootsRecordProcessorPoq.Init(bootsRecord, metadata.RarityClass, false, false, metadata.Id, metadata.ReturnItemUid());
                        PathOfQuasimorph.itemRecordsControllerPoq.bootsRecordProcessorPoq.RerollRandomStat(record, metadata);
                    }

                    BreakableItemRecord breakableItemRecord = basePickupItemRecord as BreakableItemRecord;

                    if (breakableItemRecord != null)
                    {
                        //breakableItemRecord.Unbreakable;
                    }

                    ImplantRecord implantRecord = basePickupItemRecord as ImplantRecord;

                    if (implantRecord != null)
                    {
                        _logger.Log($"implantRecord processing");
                    }

                    AugmentationRecord augmentationRecord = basePickupItemRecord as AugmentationRecord;

                    if (augmentationRecord != null)
                    {
                        _logger.Log($"augmentationRecord processing");
                    }

                    AmmoRecord ammoRecord = basePickupItemRecord as AmmoRecord;

                    if (ammoRecord != null)
                    {
                        _logger.Log($"augmentationRecord processing");
                        PathOfQuasimorph.itemRecordsControllerPoq.ammoRecordProcessorPoq.Init(ammoRecord, metadata.RarityClass, false, false, metadata.Id, metadata.ReturnItemUid());
                        PathOfQuasimorph.itemRecordsControllerPoq.ammoRecordProcessorPoq.RerollRandomStat(record, metadata);
                    }
                }
            }

            if (record.Type == SynthraformerType.Traits)
            {
                foreach (var basePickupItemRecord in targetItem.Records)
                {
                    WeaponRecord weaponRecord2 = basePickupItemRecord as WeaponRecord;

                    if (weaponRecord2 != null)
                    {
                        _logger.Log($"weaponRecord processing");
                        PathOfQuasimorph.itemRecordsControllerPoq.weaponRecordProcessorPoq.Init(weaponRecord2, metadata.RarityClass, false, false, metadata.Id, metadata.ReturnItemUid());
                            PathOfQuasimorph.itemRecordsControllerPoq.weaponRecordProcessorPoq.ReplaceWeaponTraits(record, metadata);
                        __result = true;
                    }
                }
            }
        }



        private static bool CreateNewItem(BasePickupItem target, BasePickupItem repair)
        {
            Plugin.Logger.Log($"Synthraformer CreateNewItem");

            var baseId = MetadataWrapper.GetBaseId(target.Id);
            var newId = PathOfQuasimorph.itemRecordsControllerPoq.InterceptAndReplaceItemId(baseId, false, true);
            var newItem = ItemFactoryPoq.CreateNewItem(newId);

            Plugin.Logger.Log($"oldItem {target.Id}");
            Plugin.Logger.Log($"newItem {newItem.Id}");

            if (UI.Drag.SlotUnderCursor._itemStorage.SwitchItems(target, newItem))
            {
                Plugin.Logger.Log($"SwitchItems OK");

                CopyFromOld(target, newItem);
                return true;
            }

            return false;
        }

        private static void CopyFromOld(BasePickupItem oldItem, BasePickupItem newItem)
        {
            Plugin.Logger.Log($"CopyFromOld");
            var pickupItemNew = newItem as PickupItem;

            foreach (PickupItemComponent comp in pickupItemNew.Components)
            {
                StackableItemComponent stackableItemComponent = comp as StackableItemComponent;
                var oldStackableItemComponent = oldItem.Comp<StackableItemComponent>();

                if (stackableItemComponent != null)
                {
                    stackableItemComponent.Count = oldStackableItemComponent.Count;
                    stackableItemComponent.Max = oldStackableItemComponent.Max;
                }
            }
        }
    }
}
