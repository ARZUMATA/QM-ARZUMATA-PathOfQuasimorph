using MGSC;
using QM_PathOfQuasimorph.Core.Records;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using UnityEngine;

namespace QM_PathOfQuasimorph.Core
{
    public class SynthraformerController
    {
        public static float DROP_CHANCE = 80;
        public static string nameBase = "synthraformer_poq";
        private static Sprite[] sprites = Helpers.LoadSpritesFromEmbeddedBundle("QM_PathOfQuasimorph.Files.AssetBundles.pathofquasimorph");
        private static Logger _logger = new Logger(null, typeof(SynthraformerController));

        private static readonly Dictionary<Type, Action<ItemRecord, MetadataWrapper>> _initializers = new Dictionary<Type, Action<ItemRecord, MetadataWrapper>>();

        public enum SynthraformerType
        {
            Amplifier,
            Traits,
            RandomRarity,
            Indestructible,
        }

        static SynthraformerController()
        {
            RegisterInitializer<WeaponRecord>((r, m) =>
                PathOfQuasimorph.itemRecordsControllerPoq.weaponRecordProcessorPoq.Init(r, m.RarityClass, false, false, m.Id, m.ReturnItemUid()));

            RegisterInitializer<ArmorRecord>((r, m) =>
                PathOfQuasimorph.itemRecordsControllerPoq.armorRecordProcessorPoq.Init(r, m.RarityClass, false, false, m.Id, m.ReturnItemUid()));

            RegisterInitializer<HelmetRecord>((r, m) =>
                PathOfQuasimorph.itemRecordsControllerPoq.helmetRecordProcessorPoq.Init(r, m.RarityClass, false, false, m.Id, m.ReturnItemUid()));

            RegisterInitializer<LeggingsRecord>((r, m) =>
                PathOfQuasimorph.itemRecordsControllerPoq.leggingsRecordProcessorPoq.Init(r, m.RarityClass, false, false, m.Id, m.ReturnItemUid()));

            RegisterInitializer<BootsRecord>((r, m) =>
                PathOfQuasimorph.itemRecordsControllerPoq.bootsRecordProcessorPoq.Init(r, m.RarityClass, false, false, m.Id, m.ReturnItemUid()));

            RegisterInitializer<AmmoRecord>((r, m) =>
                PathOfQuasimorph.itemRecordsControllerPoq.ammoRecordProcessorPoq.Init(r, m.RarityClass, false, false, m.Id, m.ReturnItemUid()));

            foreach (var s in sprites)
            {
                _logger.Log($"SynthraformerController sprites available: {s.name}");
            }

        }

        private static void RegisterInitializer<T>(Action<T, MetadataWrapper> initAction)
            where T : ItemRecord
        {
            _initializers[typeof(T)] = (record, meta) => initAction((T)record, meta);
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

            var record = new SynthraformerRecord();
            record.Id = itemId;
            record.Categories = new List<string>();
            record.TechLevel = 1;
            record.Price = 0;
            record.Weight = 0;
            record.InventoryWidthSize = 1;
            record.ItemClass = ItemClass.Parts;
            record.RepairSpecialRule = RepairSpecialRule.All;

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

            record.MaxStack = 100;
            record.UsageCost = 1;
            record.MaxUsage = 1;
            record.Type = type;

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

        internal string GetNameFromRarity(ItemRarity itemRarity)
        {
            return $"{nameBase}_{itemRarity.ToString().ToLower()}";
        }

        public void Apply(ref BasePickupItem target, BasePickupItem repair, SynthraformerRecord record, ref bool __result)
        {
            Plugin.Logger.Log($"Apply");

            // We replace item instead changing records and copy some item data to keep stuff like loaded ammo / durability etc.
            DragController drag = UI.Drag;

            var targetItem = target as PickupItem;

            var hasMetadata = RecordCollection.MetadataWrapperRecords.TryGetValue(targetItem.Id, out var metadata);

            Plugin.Logger.Log($"hasMetadata: {hasMetadata}");

            if (!hasMetadata)
            {
                metadata = MetadataWrapper.SplitItemUid(targetItem.Id);
            }

            if (metadata.RarityClass == ItemRarity.Standard)
            {
                Plugin.Logger.Log($"Synthraformer Apply : metadata.RarityClass == ItemRarity.Standard");
                __result = CreateNewItem(targetItem, repair, ref __result, drag);
            }

            if (record.Type == SynthraformerType.Amplifier)
            {
                foreach (var basePickupItemRecord in targetItem.Records)
                {

                    WeaponRecord weaponRecord = basePickupItemRecord as WeaponRecord;

                    if (weaponRecord != null)
                    {
                        _logger.Log($"weaponRecord processing");
                        ApplyWeaponRecord(targetItem, repair, ref __result, drag, metadata);
                    }

                    HelmetRecord helmetRecord = basePickupItemRecord as HelmetRecord;

                    if (helmetRecord != null)
                    {
                        _logger.Log($"helmetRecord processing");
                        ApplyHelmetRecord(targetItem, repair, ref __result, drag, metadata);
                    }

                    ArmorRecord armorRecord = basePickupItemRecord as ArmorRecord;

                    if (armorRecord != null)
                    {
                        _logger.Log($"armorRecord processing");
                        ApplyArmorRecord(targetItem, repair, ref __result, drag, metadata);
                    }

                    LeggingsRecord leggingsRecord = basePickupItemRecord as LeggingsRecord;

                    if (leggingsRecord != null)
                    {
                        _logger.Log($"leggingsRecord processing");
                        ApplyLeggingsRecord(targetItem, repair, ref __result, drag, metadata);
                    }

                    BootsRecord bootsRecord = basePickupItemRecord as BootsRecord;

                    if (bootsRecord != null)
                    {
                        _logger.Log($"bootsRecord processing");
                        ApplyBootsRecord(targetItem, repair, ref __result, drag, metadata);
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
                        ApplyImplantRecord(targetItem, repair, ref __result, drag, metadata);
                    }

                    AugmentationRecord augmentationRecord = basePickupItemRecord as AugmentationRecord;

                    if (augmentationRecord != null)
                    {
                        _logger.Log($"augmentationRecord processing");
                        ApplyAugmentationRecord(targetItem, repair, ref __result, drag, metadata);
                    }

                    AmmoRecord ammoRecord = basePickupItemRecord as AmmoRecord;

                    if (ammoRecord != null)
                    {
                        _logger.Log($"augmentationRecord processing");
                        ApplyAmmoRecord(targetItem, repair, ref __result, drag, metadata);

                    }
                }

                return;
            }

            if (record.Type == SynthraformerType.Traits)
            {
                var weaponRecord = targetItem.Record<WeaponRecord>();

                if (weaponRecord != null)
                {
                    if (PathOfQuasimorph.synthraformerController.ProcessAction<WeaponRecord>(
                        targetItem,
                        repair,
                        (recomb, meta) =>
                        {
                            PathOfQuasimorph.itemRecordsControllerPoq.weaponRecordProcessorPoq.ReplaceWeaponTraits(recomb, meta);
                        }))
                    {
                        var weaponComponent = targetItem.Comp<WeaponComponent>();

                        ItemInteractionSystem.ConsumeItem(repair);

                        // Also replace traits on actual weapon

                        weaponComponent.Traits.Clear();

                        foreach (var trait in weaponRecord.Traits)
                        {
                            weaponComponent.Traits.Add(ItemTraitSystem.CreateItemTrait(trait));
                        }

                        __result = true;
                    }
                }

                return;
            }
        }

        private void ApplyImplantRecord(BasePickupItem target, BasePickupItem repair, ref bool __result, DragController drag, MetadataWrapper metadata)
        {
            Plugin.Logger.Log($"Synthraformer Apply : ImplantRecord");
        }

        private void ApplyAugmentationRecord(BasePickupItem target, BasePickupItem repair, ref bool __result, DragController drag, MetadataWrapper metadata)
        {
            Plugin.Logger.Log($"Synthraformer Apply : AugmentationRecord");

        }

        private void ApplyBootsRecord(BasePickupItem target, BasePickupItem repair, ref bool __result, DragController drag, MetadataWrapper metadata)
        {
            Plugin.Logger.Log($"Synthraformer Apply : BootsRecord");

            if (PathOfQuasimorph.synthraformerController.ProcessAction<BootsRecord>(
                target,
                repair,
                (recomb, meta) =>
                {
                    PathOfQuasimorph.itemRecordsControllerPoq.bootsRecordProcessorPoq.RerollRandomStat(recomb, meta);
                }))
            {
                ItemInteractionSystem.ConsumeItem(repair);
                __result = true;
            }
        }

        private void ApplyLeggingsRecord(BasePickupItem target, BasePickupItem repair, ref bool __result, DragController drag, MetadataWrapper metadata)
        {
            Plugin.Logger.Log($"Synthraformer Apply : LeggingsRecord");

            if (PathOfQuasimorph.synthraformerController.ProcessAction<LeggingsRecord>(
                target,
                repair,
                (recomb, meta) =>
                {
                    PathOfQuasimorph.itemRecordsControllerPoq.leggingsRecordProcessorPoq.RerollRandomStat(recomb, meta);
                }))
            {
                ItemInteractionSystem.ConsumeItem(repair);
                __result = true;
            }
        }

        private void ApplyArmorRecord(BasePickupItem target, BasePickupItem repair, ref bool __result, DragController drag, MetadataWrapper metadata)
        {
            Plugin.Logger.Log($"Synthraformer Apply : ArmorRecord");

            if (PathOfQuasimorph.synthraformerController.ProcessAction<ArmorRecord>(
                target,
                repair,
                (recomb, meta) =>
                {
                    PathOfQuasimorph.itemRecordsControllerPoq.armorRecordProcessorPoq.RerollRandomStat(recomb, meta);
                }))
            {
                ItemInteractionSystem.ConsumeItem(repair);
                __result = true;
            }
        }

        private void ApplyHelmetRecord(BasePickupItem target, BasePickupItem repair, ref bool __result, DragController drag, MetadataWrapper metadata)
        {
            Plugin.Logger.Log($"Synthraformer Apply : HelmetRecord");

            if (PathOfQuasimorph.synthraformerController.ProcessAction<HelmetRecord>(
              target,
              repair,
              (recomb, meta) =>
              {
                  PathOfQuasimorph.itemRecordsControllerPoq.helmetRecordProcessorPoq.RerollRandomStat(recomb, meta);
              }))
            {
                ItemInteractionSystem.ConsumeItem(repair);
                __result = true;
            }
        }

        private static void ApplyWeaponRecord(BasePickupItem target, BasePickupItem repair, ref bool __result, DragController drag, MetadataWrapper metadata)
        {
            Plugin.Logger.Log($"Synthraformer Apply : WeaponRecord");

            if (PathOfQuasimorph.synthraformerController.ProcessAction<WeaponRecord>(
                target,
                repair,
                (recomb, meta) =>
                {
                    PathOfQuasimorph.itemRecordsControllerPoq.weaponRecordProcessorPoq.RerollRandomStat(recomb, meta);
                }))
            {
                ItemInteractionSystem.ConsumeItem(repair);
                __result = true;
            }
        }

        private static void ApplyAmmoRecord(BasePickupItem target, BasePickupItem repair, ref bool __result, DragController drag, MetadataWrapper metadata)
        {
            Plugin.Logger.Log($"Synthraformer Apply : AmmoRecord");

            if (PathOfQuasimorph.synthraformerController.ProcessAction<AmmoRecord>(
                target,
                repair,
                (recomb, meta) =>
                {
                    PathOfQuasimorph.itemRecordsControllerPoq.ammoRecordProcessorPoq.RerollRandomStat(recomb, meta);
                }))
            {
                ItemInteractionSystem.ConsumeItem(repair);
                __result = true;
            }
        }

        private static bool CreateNewItem(BasePickupItem target, BasePickupItem repair, ref bool __result, DragController drag)
        {
            Plugin.Logger.Log($"Synthraformer CreateNewItem");

            var baseId = MetadataWrapper.GetBaseId(target.Id);
            var newId = PathOfQuasimorph.itemRecordsControllerPoq.InterceptAndReplaceItemId(baseId, false, true);
            var newItem = ItemFactoryPoq.CreateNewItem(newId);

            Plugin.Logger.Log($"oldItem {target.Id}");
            Plugin.Logger.Log($"newItem {newItem.Id}");

            if (drag.SlotUnderCursor._itemStorage.SwitchItems(target, newItem))
            {
                Plugin.Logger.Log($"SwitchItems OK");

                ProcessItem(target, newItem);
                ItemInteractionSystem.ConsumeItem(repair);
                __result = true;
            }

            return __result;
        }

        private static void ProcessItem(BasePickupItem oldItem, BasePickupItem newItem)
        {
            Plugin.Logger.Log($"ProcessItem");
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

        internal bool ProcessAction<TRecord>(
            BasePickupItem target,
            BasePickupItem repair,
            Action<SynthraformerRecord, MetadataWrapper> processorAction)
            where TRecord : ItemRecord
        {
            _logger.Log($"ProcessAction<{typeof(TRecord).Name}>: Invoked");

            // Validate metadata
            if (!RecordCollection.MetadataWrapperRecords.TryGetValue(target.Id, out MetadataWrapper metadata) ||
                !metadata.PoqItem)
            {
                _logger.Log("ProcessAction: Failed - Missing metadata or not PoqItem");
                return false;
            }

            // Get composite record
            var composite = Data.Items.GetRecord(target.Id) as CompositeItemRecord;
            if (composite == null)
            {
                _logger.Log("ProcessAction: Failed - No CompositeItemRecord found");
                return false;
            }

            var recombRecord = repair.Record<SynthraformerRecord>();

            foreach (var record in composite.Records)
            {
                if (record is TRecord targetRecord)
                {
                    _logger.Log($"Processing {typeof(TRecord).Name} with {processorAction.Method.Name}");

                    // Generic Init: Call appropriate processor based on TRecord
                    if (!InitializeRecordProcessor<TRecord>(targetRecord, metadata))
                    {
                        _logger.Log($"Init failed for {typeof(TRecord).Name}");
                        return false;
                    }

                    // Execute the action
                    processorAction(recombRecord, metadata);
                    return true;
                }
            }

            _logger.Log($"No valid {typeof(TRecord).Name} pair found");
            return false;
        }

        private bool InitializeRecordProcessor<TRecord>(TRecord record, MetadataWrapper metadata)
      where TRecord : ItemRecord
        {
            if (_initializers.TryGetValue(typeof(TRecord), out var initializer))
            {
                initializer(record, metadata);
                return true;
            }

            _logger.Log($"No initializer registered for record type: {typeof(TRecord).Name}");
            return false;
        }
    }
}
