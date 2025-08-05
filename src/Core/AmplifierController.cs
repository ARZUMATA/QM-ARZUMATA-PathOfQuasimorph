using MGSC;
using QM_PathOfQuasimorph.Core.Records;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace QM_PathOfQuasimorph.Core
{
    internal class AmplifierController
    {
        public static float DROP_CHANCE = 100;
        public static string nameBase = "amplifier_poq";

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
            if (item == null || item.Id == null)
            {
                return false;
            }

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
            var itemId = $"{nameBase}_{rarity.ToString().ToLower()}";

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

            Data.Items._records.Remove(itemId);
            Data.Items._records.Add(itemId, compositeItemRecord);

            Localization.DuplicateKey($"item.{nameBase}.shortdesc", "item." + itemId + ".shortdesc");
        }

        internal string GetAmplifierNameFromRarity(ItemRarity itemRarity)
        {
            return $"{nameBase}_{itemRarity.ToString().ToLower()}";
        }
        public bool ApplyAmplifier(ref BasePickupItem target, BasePickupItem repair, AmplifierRecord ampRec, ref bool __result)
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

            if (true) //PathOfQuasimorph.itemRecordsControllerPoq.ChangeRecordFromAmplifier(oldItem, repair))
            {
                var baseId = MetadataWrapper.GetBaseId(target.Id);
                var newId = PathOfQuasimorph.itemRecordsControllerPoq.InterceptAndReplaceItemId(baseId, false, ampRec.Rarity);
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
                    newItem.
                }
            }
        }
    }
}
