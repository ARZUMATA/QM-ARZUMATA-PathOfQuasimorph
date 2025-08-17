using MGSC;
using QM_PathOfQuasimorph.Core;
using QM_PathOfQuasimorph.Records;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace QM_PathOfQuasimorph.Controllers
{
    public class SynthraformerController
    {
        public static float DROP_CHANCE = 70;
        public static string nameBase = "synthraformer_poq";
        private static Sprite[] sprites = Helpers.LoadSpritesFromEmbeddedBundle("QM_PathOfQuasimorph.Files.AssetBundles.pathofquasimorph");
        private static Logger _logger = new Logger(null, typeof(SynthraformerController));
        public static List<string> recipesOutputItems = new List<string>();
        const int craftUpgradeCostAmount = 4;

        public enum SynthraformerType
        {
            Amplifier,
            Traits,
            Rarity,
            Indestructible,
            Catalyst,
        }

        public static void AddItems()
        {
            RemoveExistingSynthraformerRecipes();
            recipesOutputItems.Clear();

            // We don't need all rarities here but why not make them.

            CreateItem(SynthraformerType.Rarity);
            CreateItem(SynthraformerType.Amplifier);
            CreateItem(SynthraformerType.Traits);
            CreateItem(SynthraformerType.Indestructible);
            CreateItem(SynthraformerType.Catalyst);
        }

        public static string MakeId(SynthraformerType type) => $"{nameBase}_{(int)type}";
        public static bool Is(BasePickupItem item) => item?.Id != null && Is(item.Id);
        public static bool Is(string item) => item?.Contains(nameBase) == true;
        public string GetBaseDrop() => MakeId(SynthraformerType.Amplifier);

        private static void CreateItem(SynthraformerType type)
        {
            string itemId = MakeId(type);

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
            };

            var itemRecipe = new ItemProduceReceipt
            {
                OutputItem = itemId,
                ProduceTimeInHours = 2,
                RequiredItems = new List<ItemQuantity>()
            };

            // Setup recipe per type
            switch (type)
            {
                case SynthraformerType.Traits:
                    itemRecipe.RequiredItems.Add(new ItemQuantity(MakeId(SynthraformerType.Amplifier), 5));
                    break;

                case SynthraformerType.Rarity:
                    itemRecipe.RequiredItems.Add(new ItemQuantity(MakeId(SynthraformerType.Traits), 2));
                    break;

                case SynthraformerType.Indestructible:
                    itemRecipe.RequiredItems.Add(new ItemQuantity(MakeId(SynthraformerType.Traits), 1));
                    itemRecipe.RequiredItems.Add(new ItemQuantity(MakeId(SynthraformerType.Indestructible), 1));
                    break;
            }

            if (itemRecipe.RequiredItems.Count > 0)
            {
                AddRecipe(itemRecipe);
            }

            // Descriptor & Icon
            RepairDescriptor descriptor = ScriptableObject.CreateInstance("RepairDescriptor") as RepairDescriptor;
            Sprite icon = Array.Find(sprites, s => s.name == $"synthraformer_poq_icons_{(int)type}");
            descriptor._icon = icon;
            descriptor._smallIcon = icon;
            descriptor.name = itemId;
            record.ContentDescriptor = descriptor;

            // Save record
            CompositeItemRecord compositeItemRecord = new CompositeItemRecord(itemId);
            compositeItemRecord.Records.Add(record);
            Data.Items._records[itemId] = compositeItemRecord;

            // Localization
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

        private static void AddRecipe(ItemProduceReceipt itemRecipe)
        {
            Plugin.Logger.Log($"AddRecipe: {itemRecipe.OutputItem}");
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

            Plugin.Logger.Log($"metadata.RarityClass {metadata.RarityClass}");
            Plugin.Logger.Log($"metadata.Id {metadata.Id}");
            Plugin.Logger.Log($"metadata.ReturnItemUid {metadata.ReturnItemUid()}");

            CompositeItemRecord obj = Data.Items.GetRecord(targetItem.Id) as CompositeItemRecord;

            switch (record.Type)
            {
                case SynthraformerType.Amplifier:
                    HandleAmplifier(targetItem, repair, record, metadata, obj, ref __result);
                    break;

                case SynthraformerType.Traits:
                    HandleTraits(targetItem, repair, record, metadata, obj, ref __result);
                    break;

                case SynthraformerType.Rarity:
                    HandleRandomRarity(targetItem, repair, record, metadata, obj, ref __result);
                    break;

                case SynthraformerType.Indestructible:
                    HandleIndestructible(targetItem, repair, record, metadata, obj, ref __result);
                    break;

                case SynthraformerType.Catalyst:
                    HandleCatalyst(targetItem, repair, record, metadata, obj, ref __result);
                    break;

                default:
                    __result = false;
                    break;
            }
        }

        private void HandleAmplifier(PickupItem targetItem, BasePickupItem repair, SynthraformerRecord record, MetadataWrapper metadata, CompositeItemRecord obj, ref bool __result)
        {
            bool isStandard = metadata.RarityClass == ItemRarity.Standard;

            foreach (var basePickupItemRecord in obj.Records)
            {
                switch (basePickupItemRecord)
                {
                    case AmmoRecord ammoRecord when isStandard:
                        // There is only one exception for Standard Rarity - Ammmo.
                        __result = CreateNewItem(targetItem, repair, metadata.RarityClass, true, true);
                        return;

                    case AmmoRecord ammoRecord when !isStandard:
                        _logger.Log($"ammoRecord processing");
                        PathOfQuasimorph.itemRecordsControllerPoq.ammoRecordProcessorPoq.Init(ammoRecord, metadata.RarityClass, false, false, metadata.Id, metadata.ReturnItemUid());
                        PathOfQuasimorph.itemRecordsControllerPoq.ammoRecordProcessorPoq.RerollRandomStat(record, metadata);
                        break;

                    case WeaponRecord weaponRecord when !isStandard:
                        _logger.Log($"weaponRecord processing");
                        PathOfQuasimorph.itemRecordsControllerPoq.weaponRecordProcessorPoq.Init(weaponRecord, metadata.RarityClass, false, false, metadata.Id, metadata.ReturnItemUid());
                        PathOfQuasimorph.itemRecordsControllerPoq.weaponRecordProcessorPoq.RerollRandomStat(record, metadata);
                        break;

                    case HelmetRecord helmetRecord when !isStandard:
                        _logger.Log($"helmetRecord processing");
                        PathOfQuasimorph.itemRecordsControllerPoq.helmetRecordProcessorPoq.Init(helmetRecord, metadata.RarityClass, false, false, metadata.Id, metadata.ReturnItemUid());
                        PathOfQuasimorph.itemRecordsControllerPoq.helmetRecordProcessorPoq.RerollRandomStat(record, metadata);
                        break;

                    case ArmorRecord armorRecord when !isStandard:
                        _logger.Log($"armorRecord processing");
                        PathOfQuasimorph.itemRecordsControllerPoq.armorRecordProcessorPoq.Init(armorRecord, metadata.RarityClass, false, false, metadata.Id, metadata.ReturnItemUid());
                        PathOfQuasimorph.itemRecordsControllerPoq.armorRecordProcessorPoq.RerollRandomStat(record, metadata);
                        break;

                    case LeggingsRecord leggingsRecord when !isStandard:
                        _logger.Log($"leggingsRecord processing");
                        PathOfQuasimorph.itemRecordsControllerPoq.leggingsRecordProcessorPoq.Init(leggingsRecord, metadata.RarityClass, false, false, metadata.Id, metadata.ReturnItemUid());
                        PathOfQuasimorph.itemRecordsControllerPoq.leggingsRecordProcessorPoq.RerollRandomStat(record, metadata);
                        break;

                    case BootsRecord bootsRecord when !isStandard:
                        _logger.Log($"bootsRecord processing");
                        PathOfQuasimorph.itemRecordsControllerPoq.bootsRecordProcessorPoq.Init(bootsRecord, metadata.RarityClass, false, false, metadata.Id, metadata.ReturnItemUid());
                        PathOfQuasimorph.itemRecordsControllerPoq.bootsRecordProcessorPoq.RerollRandomStat(record, metadata);
                        break;

                    case ImplantRecord implantRecord when !isStandard:
                        _logger.Log($"implantRecord processing");
                        break;

                    case AugmentationRecord augmentationRecord when !isStandard:
                        _logger.Log($"augmentationRecord processing");
                        break;
                }

                __result = CreateNewItem(targetItem, repair, metadata.RarityClass, false, false);

                if (__result)
                {
                    RecordCollection.ItemRecords.Remove(targetItem.Id);
                    RecordCollection.ItemRecords.Add(targetItem.Id, obj);
                }

                return;
            }

            __result = false;
        }

        private void HandleTraits(PickupItem targetItem, BasePickupItem repair, SynthraformerRecord record, MetadataWrapper metadata, CompositeItemRecord obj, ref bool __result)
        {
            if (metadata.RarityClass == ItemRarity.Standard)
            {
                __result = false;
                return;
            }

            foreach (var basePickupItemRecord in obj.Records)
            {
                switch (basePickupItemRecord)
                {
                    case WeaponRecord weaponRecord:
                        PathOfQuasimorph.itemRecordsControllerPoq.weaponRecordProcessorPoq.Init(weaponRecord, metadata.RarityClass, false, false, metadata.Id, metadata.ReturnItemUid());
                        PathOfQuasimorph.itemRecordsControllerPoq.weaponRecordProcessorPoq.ReplaceWeaponTraits(record, metadata);
                        break;

                    case AmmoRecord ammoRecord:
                        PathOfQuasimorph.itemRecordsControllerPoq.ammoRecordProcessorPoq.Init(ammoRecord, metadata.RarityClass, false, false, metadata.Id, metadata.ReturnItemUid());
                        PathOfQuasimorph.itemRecordsControllerPoq.ammoRecordProcessorPoq.ReplaceWeaponTraits(record, metadata);
                        break;

                    default:
                        continue;
                }

                __result = CreateNewItem(targetItem, repair, metadata.RarityClass, false, false);

                if (__result)
                {
                    RecordCollection.ItemRecords.Remove(targetItem.Id);
                    RecordCollection.ItemRecords.Add(targetItem.Id, obj);
                }

                return;
            }

            __result = false;
        }


        private void HandleRandomRarity(PickupItem targetItem, BasePickupItem repair, SynthraformerRecord record, MetadataWrapper metadata, CompositeItemRecord obj, ref bool __result)
        {
            // We simply roll rarity i.e. just generatin new item rarity and new item for it.
            __result = CreateNewItem(targetItem, repair, metadata.RarityClass, true, true);
            return;
        }

        private void HandleIndestructible(PickupItem targetItem, BasePickupItem repair, SynthraformerRecord record, MetadataWrapper metadata, CompositeItemRecord obj, ref bool __result)
        {
            _logger.Log($"HandleIndestructible")
                ;
            if (metadata.RarityClass == ItemRarity.Standard)
            {
                __result = false;
                return;
            }

            foreach (var basePickupItemRecord in obj.Records)
            {
                switch (basePickupItemRecord)
                {
                    case BreakableItemRecord breakableItemRecord:
                        PathOfQuasimorph.itemRecordsControllerPoq.breakableItemProcessorPoq.Init(breakableItemRecord, metadata.RarityClass, false, false, metadata.Id, metadata.ReturnItemUid());
                        PathOfQuasimorph.itemRecordsControllerPoq.breakableItemProcessorPoq.AddUnbreakableTrait(record, metadata);
                        break;

                    default:
                        continue;
                }

                __result = CreateNewItem(targetItem, repair, metadata.RarityClass, false, false);

                if (__result)
                {
                    RecordCollection.ItemRecords.Remove(targetItem.Id);
                    RecordCollection.ItemRecords.Add(targetItem.Id, obj);
                }

                return;
            }

            __result = false;
        }

        private void HandleCatalyst(PickupItem targetItem, BasePickupItem repair, SynthraformerRecord record, MetadataWrapper metadata, CompositeItemRecord obj, ref bool __result)
        {
            _logger.Log($"HandleCatalyst");

            if (metadata.RarityClass == ItemRarity.Standard)
            {
                __result = false;
                return;
            }

            bool hasWeaponRecord = targetItem.Is<WeaponRecord>();
            bool hasAugmentationRecord = targetItem.Is<AugmentationRecord>();

            foreach (var basePickupItemRecord in obj.Records)
            {
                switch (basePickupItemRecord)
                {
                    case WeaponRecord weaponRecord when !hasAugmentationRecord:
                        PathOfQuasimorph.itemRecordsControllerPoq.weaponRecordProcessorPoq.Init(weaponRecord, metadata.RarityClass, false, false, metadata.Id, metadata.ReturnItemUid());
                        PathOfQuasimorph.itemRecordsControllerPoq.weaponRecordProcessorPoq.CreateAugmentation(record, metadata, obj);
                        break;

                    case AugmentationRecord augmentationRecord when !hasWeaponRecord:
                        PathOfQuasimorph.itemRecordsControllerPoq.augmentationRecordProcessorPoq.Init(augmentationRecord, metadata.RarityClass, false, false, metadata.Id, metadata.ReturnItemUid());
                        PathOfQuasimorph.itemRecordsControllerPoq.augmentationRecordProcessorPoq.AddRandomEffect(record, metadata);
                        break;

                    case ImplantRecord implantRecord when !hasWeaponRecord:
                        PathOfQuasimorph.itemRecordsControllerPoq.implantRecordProcessorPoq.Init(implantRecord, metadata.RarityClass, false, false, metadata.Id, metadata.ReturnItemUid());
                        PathOfQuasimorph.itemRecordsControllerPoq.implantRecordProcessorPoq.AddRandomEffect(record, metadata);
                        break;

                    default:
                        continue;
                }

                __result = CreateNewItem(targetItem, repair, metadata.RarityClass, false, false);

                if (__result)
                {
                    RecordCollection.ItemRecords.Remove(targetItem.Id);
                    RecordCollection.ItemRecords.Add(targetItem.Id, obj);
                }

                return;
            }

            __result = false;
        }

        private static bool CreateNewItem(BasePickupItem target, BasePickupItem repair, ItemRarity itemRarity, bool selectRarity, bool applyRarity)
        {
            Plugin.Logger.Log($"Synthraformer CreateNewItem: {target.Id}, Rarity={itemRarity}, SelectRarity={selectRarity}, ApplyRarity={applyRarity}");

            string newId = selectRarity
                ? PathOfQuasimorph.itemRecordsControllerPoq.InterceptAndReplaceItemId(
                    Id: MetadataWrapper.GetBaseId(target.Id),
                    mobRarityBoost: false,
                    itemRarity: itemRarity,
                    selectRarity: selectRarity,
                    ignoreBlacklist: true,
                    randomUidInjected: null,
                    applyRarity: applyRarity)
                : target.Id;

            var newItem = ItemFactoryPoq.CreateNewItem(newId);

            Plugin.Logger.Log($"newId: {newId}");
            Plugin.Logger.Log($"oldItem: {target.Id}");
            Plugin.Logger.Log($"newItem: {newItem?.Id ?? "NULL"}");

            if (newItem == null)
            {
                return false;
            }

            if (UI.Drag.SlotUnderCursor._itemStorage.SwitchItems(target, newItem))
            {
                CopyFromOld(target, newItem);
                return true;
            }

            return false;
        }

        private static void CopyFromOld(BasePickupItem oldItem, BasePickupItem newItem)
        {
            var pickupItemNew = newItem as PickupItem;
            var pickupItemOld = oldItem as PickupItem;

            if (pickupItemNew == null || pickupItemOld == null)
            {
                return;
            }

            foreach (PickupItemComponent newComp in pickupItemNew.Components)
            {
                // Find old component with same concrete type
                var oldComp = pickupItemOld.Components.FirstOrDefault(c => c?.GetType() == newComp.GetType());

                if (oldComp == null)
                {
                    continue;
                }

                // Copy all [Save]-marked properties
                CopySaveFieldsFrom(newComp, oldComp);
            }
        }

        private static void CopySaveFieldsFrom(PickupItemComponent target, PickupItemComponent source)
        {
            if (source == null || target == null)
            {
                return;
            }

            var props = target.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => Attribute.IsDefined(p, typeof(Save)) && p.CanRead && p.CanWrite)
                .ToList();

            foreach (var prop in props)
            {
                Type propType = prop.PropertyType;

                // Ignore List<T>
                if (propType.IsGenericType && propType.GetGenericTypeDefinition() == typeof(List<>))
                    continue;

                // Ignore Dictionary<TKey, TValue>
                if (propType.IsGenericType && propType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                    continue;

                try
                {
                    var value = prop.GetValue(source);
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
