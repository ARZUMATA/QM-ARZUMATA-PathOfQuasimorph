using MGSC;
using QM_PathOfQuasimorph.Core.Records;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Contexts;
using UnityEngine;

namespace QM_PathOfQuasimorph.Core
{
    public class SynthraformerController
    {
        public static float DROP_CHANCE = 80;
        public static string nameBase = "synthraformer_poq";
        private static Sprite[] sprites = Helpers.LoadSpritesFromEmbeddedBundle("QM_PathOfQuasimorph.Files.AssetBundles.pathofquasimorph");
        private static Logger _logger = new Logger(null, typeof(SynthraformerController));
        public static List<string> recipesOutputItems = new List<string>();

        public enum SynthraformerType
        {
            Amplifier,
            Traits,
            RandomRarity,
            Indestructible,
            Catalyst,
        }

        public static void AddItems()
        {
            RemoveExistingSynthraformerRecipes();
            recipesOutputItems.Clear();
            CreateItem(SynthraformerType.Amplifier, ItemRarity.Standard);
            CreateItem(SynthraformerType.Amplifier, ItemRarity.Enhanced);
            CreateItem(SynthraformerType.Amplifier, ItemRarity.Advanced);
            CreateItem(SynthraformerType.Amplifier, ItemRarity.Premium);
            CreateItem(SynthraformerType.Amplifier, ItemRarity.Prototype);
            CreateItem(SynthraformerType.Amplifier, ItemRarity.Quantum);
            CreateItem(SynthraformerType.Traits, ItemRarity.Standard);
            CreateItem(SynthraformerType.RandomRarity, ItemRarity.Standard);
            CreateItem(SynthraformerType.Indestructible, ItemRarity.Standard);
            //CreateItem(SynthraformerType.Catalyst, ItemRarity.Standard);
            CreateAmplifierRecipes();
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

        private static void CreateItem(SynthraformerType type, ItemRarity rarity)
        {
            var itemId = $"{nameBase}_{(int)type}_{rarity.ToString().ToLower()}";

            CompositeItemRecord compositeItemRecord = new CompositeItemRecord(itemId);
            ItemProduceReceipt itemRecipe = new ItemProduceReceipt();
            itemRecipe.RequiredItems = new List<ItemQuantity>();

            var record = new SynthraformerRecord
            {
                BaseId = nameBase,
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
                Type = type,
                Rarity = rarity

            };

            switch (type)
            {
                case SynthraformerType.Amplifier:
                    //record.RepairSpecialRule = RepairSpecialRule.AllWeapons;
                    break;

                case SynthraformerType.Traits:
                    //record.RepairSpecialRule = RepairSpecialRule.AllWeapons;
                    itemRecipe.RequiredItems.Add(new ItemQuantity($"synthraformer_poq_{(int)SynthraformerType.Amplifier}_{ItemRarity.Standard.ToString().ToLower()}", 5));
                    itemRecipe.OutputItem = itemId;
                    itemRecipe.ProduceTimeInHours = 2;
                    AddRecipe(itemRecipe);
                    break;

                case SynthraformerType.RandomRarity:
                    //record.RepairSpecialRule = RepairSpecialRule.AllWeapons;
                    itemRecipe.RequiredItems.Add(new ItemQuantity($"synthraformer_poq_{(int)SynthraformerType.Traits}_{ItemRarity.Standard.ToString().ToLower()}", 2));
                    itemRecipe.OutputItem = itemId;
                    itemRecipe.ProduceTimeInHours = 2;
                    AddRecipe(itemRecipe);
                    break;

                case SynthraformerType.Indestructible:
                    //record.RepairSpecialRule = RepairSpecialRule.AllWeapons;
                    itemRecipe.RequiredItems.Add(new ItemQuantity($"synthraformer_poq_{(int)SynthraformerType.Traits}_{ItemRarity.Standard.ToString().ToLower()}", 1));
                    itemRecipe.RequiredItems.Add(new ItemQuantity($"synthraformer_poq_{(int)SynthraformerType.Indestructible}_{ItemRarity.Standard.ToString().ToLower()}", 1));
                    itemRecipe.OutputItem = itemId;
                    itemRecipe.ProduceTimeInHours = 2;
                    AddRecipe(itemRecipe);
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

            Localization.DuplicateKey($"item.{nameBase}_{(int)type}.name", "item." + itemId + ".name");
            Localization.DuplicateKey($"item.{nameBase}_{(int)type}.desc", "item." + itemId + ".desc");

            Localization.DuplicateKey($"item.{nameBase}.shortdesc", "item." + itemId + ".shortdesc");
        }

        private static void RemoveExistingSynthraformerRecipes()
        {
            int removedCount = Data.ProduceReceipts.RemoveAll(recipe =>
                recipe.OutputItem != null &&
                recipe.OutputItem.StartsWith(nameBase, StringComparison.OrdinalIgnoreCase)
            );

            Plugin.Logger.Log($"Removed {removedCount} existing recipe(s) with OutputItem starting with '{nameBase}'");
        }

        private static void CreateAmplifierRecipes()
        {
            var type = (int)SynthraformerType.Amplifier;
            var rarities = Enum.GetValues(typeof(ItemRarity)) as ItemRarity[];

            int needAmnt = 5;

            for (int i = 0; i < rarities.Length - 1; i++)
            {
                var currentRarity = rarities[i];
                var nextRarity = rarities[i + 1];

                ItemProduceReceipt itemRecipe = new ItemProduceReceipt();
                itemRecipe.RequiredItems = new List<ItemQuantity>
                {
                    new ItemQuantity($"synthraformer_poq_{type}_{currentRarity.ToString().ToLower()}", needAmnt)
                };
                itemRecipe.OutputItem = $"synthraformer_poq_{type}_{nextRarity.ToString().ToLower()}";
                itemRecipe.ProduceTimeInHours = 2;

                AddRecipe(itemRecipe);
            }
        }

        private static void AddRecipe(ItemProduceReceipt itemRecipe)
        {
            Plugin.Logger.Log($"AddRecipe");
            Plugin.Logger.Log($"Data.ProduceReceipts.Add: {itemRecipe.Id} - {itemRecipe.OutputItem}");
            Data.ProduceReceipts.Add(itemRecipe);

            if (!recipesOutputItems.Contains(itemRecipe.OutputItem))
            {
                recipesOutputItems.Add(itemRecipe.OutputItem);
            }
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
                __result = CreateNewItem(targetItem, repair, true);
                return;
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

            __result = CreateNewItem(targetItem, repair, false);
        }

        private static bool CreateNewItem(BasePickupItem target, BasePickupItem repair, bool selectRarity)
        {
            Plugin.Logger.Log($"Synthraformer CreateNewItem");

            var baseId = MetadataWrapper.GetBaseId(target.Id);
            var newId = PathOfQuasimorph.itemRecordsControllerPoq.InterceptAndReplaceItemId(baseId, false, ItemRarity.Standard, selectRarity, true);
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
            var pickupItemOld = oldItem as PickupItem;

            if (pickupItemNew == null || pickupItemOld == null)
            {
                return;
            }

            foreach (PickupItemComponent newComp in pickupItemNew.Components)
            {
                // Find old component with same concrete type
                var oldComp = pickupItemOld.Components
                    .FirstOrDefault(c => c?.GetType() == newComp.GetType());

                if (oldComp == null) continue;

                // Copy all [Save]-marked properties
                CopySaveFieldsFrom(newComp, oldComp);
            }
        }

        private static void CopySaveFieldsFrom(PickupItemComponent target, PickupItemComponent source)
        {
            if (source == null || target == null) return;

            Type type = target.GetType();
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo prop in properties)
            {
                // Skip if not marked with [Save]
                if (!Attribute.IsDefined(prop, typeof(Save))) continue;
                if (!prop.CanRead || !prop.CanWrite) continue;

                Type propType = prop.PropertyType;

                // Ignore string
                //if (propType == typeof(string))
                //    continue;

                // Ignore List<T>
                if (propType.IsGenericType && propType.GetGenericTypeDefinition() == typeof(List<>))
                    continue;

                // Ignore Dictionary<TKey, TValue>
                if (propType.IsGenericType && propType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                    continue;

                // Optionally: ignore other collections? e.g. HashSet<T>, arrays?
                // Add more if needed.

                try
                {
                    object value = prop.GetValue(source);
                    prop.SetValue(target, value);
                }
                catch (Exception ex)
                {
                    Plugin.Logger.Log($"Failed to copy {prop.Name}: {ex.Message}");
                }
            }
        }
    }
}
