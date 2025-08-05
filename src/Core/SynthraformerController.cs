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

        public enum RecombinatorType
        {
            Amplifier,
            Traits,
            RandomRarity,
            Indestructible,
        }

        public static void AddItems()
        {
            // Add our own custom items.
            //foreach (ItemRarity rarity in Enum.GetValues(typeof(ItemRarity)))
            //{
            //    CreateItem(rarity, (int)type);
            //}
            //// Add our own custom items.
            //foreach (RecombinatorType type in Enum.GetValues(typeof(RecombinatorType)))
            //{
            //    CreateRecombinator(type, (int)type);
            //}


            CreateItem(RecombinatorType.Amplifier);
            CreateItem(RecombinatorType.Traits);
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

        private static void CreateItem(RecombinatorType type)
        {
            var itemId = $"{nameBase}_{type.ToString().ToLower()}";

            CompositeItemRecord compositeItemRecord = new CompositeItemRecord(itemId);
            ItemProduceReceipt itemRecipe = new ItemProduceReceipt();

            var record = new SynthraformerRecord();
            record.Id = itemId;
            record.Categories = new List<string>();
            record.TechLevel = 1;
            record.Price = 0;
            record.Weight = 0;
            record.InventoryWidthSize = 1;
            record.ItemClass = ItemClass.Parts;
            record.Rarity = ItemRarity.Standard;
            record.RepairSpecialRule = RepairSpecialRule.None;

            switch (type)
            {
                case RecombinatorType.Amplifier:
                    record.RepairSpecialRule = RepairSpecialRule.AllWeapons;
                    break;

                case RecombinatorType.Traits:
                    record.RepairSpecialRule = RepairSpecialRule.AllWeapons;
                    itemRecipe.RequiredItems.Add(new ItemQuantity($"synthraformer_poq_{(int)RecombinatorType.Amplifier}", 5));
                    itemRecipe.OutputItem = $"synthraformer_poq_{(int)RecombinatorType.Traits}";
                    itemRecipe.ProduceTimeInHours = 2;
                    Data.ProduceReceipts.Add(itemRecipe);
                    break;

                case RecombinatorType.RandomRarity:
                    record.RepairSpecialRule = RepairSpecialRule.AllWeapons;
                    itemRecipe.RequiredItems.Add(new ItemQuantity($"synthraformer_poq_{(int)RecombinatorType.Amplifier}", 5));
                    itemRecipe.OutputItem = $"synthraformer_poq_{(int)RecombinatorType.RandomRarity}";
                    itemRecipe.ProduceTimeInHours = 2;
                    Data.ProduceReceipts.Add(itemRecipe);
                    break;
            }

            record.MaxStack = 100;
            record.UsageCost = 1;
            record.MaxUsage = 1;
            record.RecombinatorType = type;

            RepairDescriptor descriptor = ScriptableObject.CreateInstance("RepairDescriptor") as RepairDescriptor;
            Sprite icon = System.Array.Find(sprites, s => s.name == $"synthraformer_poq_icons_{((int)type).ToString().ToLower()}");
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

        public bool Apply(ref BasePickupItem target, BasePickupItem repair, SynthraformerRecord record, ref bool __result)
        {
            RepairRecord repairRecord = repair.Record<RepairRecord>();
            Plugin.Logger.Log($"oldItem.Is<RepairRecord>() {repair.Is<RepairRecord>()}");

            if (!repairRecord.IsValidCategory(target))
            {
                Plugin.Logger.Log($"Invalid category");

                // Do original method
                return true;
            }

            // We replace item instead changing records and copy some item data to keep stuff like loaded ammo / durability etc.
            DragController drag = UI.Drag;

            if (record.RecombinatorType == RecombinatorType.Amplifier)
            {
                //if (PathOfQuasimorph.recombinatorController.ProcessRecombinatorAction(target, repair, (recomb, meta, comp) => PathOfQuasimorph.itemRecordsControllerPoq.weaponRecordProcessorPoq.ReplaceWeaponTraits(recomb, meta, comp)))
                //{
                //    ItemInteractionSystem.ConsumeItem(repair);
                //    __result = true;
                //}
            }

            if (record.RecombinatorType == RecombinatorType.Traits)
            {

                //if (PathOfQuasimorph.recombinatorController.ProcessRecombinatorAction(target, repair, (recomb, meta, comp) => PathOfQuasimorph.itemRecordsControllerPoq.weaponRecordProcessorPoq.ReplaceRequiredAmmo(recomb, meta)))
                //{
                //    ItemInteractionSystem.ConsumeItem(repair);
                //    __result = true;
                //}
            }

            var metadata = RecordCollection.MetadataWrapperRecords.TryGetValue(target.Id, out var metadataWrapper);

            if (!metadata)
            {
                metadataWrapper = MetadataWrapper.SplitItemUid(target.Id);
            }

            if (true) //PathOfQuasimorph.itemRecordsControllerPoq.ChangeRecordFromAmplifier(oldItem, repair))
            {
                var baseId = MetadataWrapper.GetBaseId(target.Id);
                var newId = PathOfQuasimorph.itemRecordsControllerPoq.InterceptAndReplaceItemId(baseId, false, metadataWrapper.RarityClass);
                var newItem = ItemFactoryPoq.CreateNewItem(newId);

                Plugin.Logger.Log($"oldItem {target.Id}");
                Plugin.Logger.Log($"newItem {newItem.Id}");

                if (drag.SlotUnderCursor._itemStorage.SwitchItems(target, newItem))
                {
                    ProcessItem(target, newItem);
                    //ItemInteractionSystem.ConsumeItem(repair);
                    __result = true;

                }

                Plugin.Logger.Log($"ChangeRecordFromAmplifier Do");

                // oldItem.Id = newItem.Id; // ?

                //Plugin.Logger.Log($"drag.SlotUnderCursor.Item before {drag.SlotUnderCursor.Item.Id}");
                ////drag.SlotUnderCursor.Item = newItem;
                //Plugin.Logger.Log($"drag.SlotUnderCursor.Item after {drag.SlotUnderCursor.Item.Id}");

                //drag.SlotUnderCursor.Initialize(newItem, drag.SlotUnderCursor._itemStorage);



            }

            return false;
        }



        private static void ProcessItem(BasePickupItem oldItem, BasePickupItem newItem)
        {
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

            foreach (BasePickupItemRecord basePickupItemRecord in pickupItemNew.Records)
            {
                AmmoRecord ammoRecord = basePickupItemRecord as AmmoRecord;

                if (ammoRecord != null)
                {
                    //newItem.
                }
            }
        }


        internal bool ProcessRecombinatorAction(BasePickupItem target, BasePickupItem repair,
            Action<SynthraformerRecord, MetadataWrapper, WeaponComponent> processorAction)
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

            var recombRecord = repair.Record<SynthraformerRecord>();

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

        internal string GetBaseDrop()
        {
            return $"{nameBase}_0";
        }
    }
}
