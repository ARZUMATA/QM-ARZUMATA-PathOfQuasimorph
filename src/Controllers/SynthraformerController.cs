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
        public static string nameBase = "synthraformer_poq";
        private static Sprite[] sprites = Helpers.LoadSpritesFromEmbeddedBundle("QM_PathOfQuasimorph.Files.AssetBundles.pathofquasimorph");
        private static Logger _logger = new Logger(null, typeof(SynthraformerController));
        public static List<string> recipesOutputItems = new List<string>();

        // Base drop chances by type
        public Dictionary<SynthraformerType, float> DropChances = new()
        {
            { SynthraformerType.PrimalCore,         0.70f },
            { SynthraformerType.Rarity,             0.05f },
            { SynthraformerType.Infuser,            0.05f },
            { SynthraformerType.Traits,             0.05f },
            { SynthraformerType.Indestructible,     0.05f },
            { SynthraformerType.Amplifier,          0.35f },
            { SynthraformerType.Transmuter,         0.05f },
            { SynthraformerType.Catalyst,           0.05f },
            { SynthraformerType.Azure,              0.05f },
        };

        public Dictionary<SynthraformerType, float> ProduceTimeMap = new Dictionary<SynthraformerType, float>
        {
            { SynthraformerType.Rarity, 1.5f },
            { SynthraformerType.Infuser, 2.0f },
            { SynthraformerType.Traits, 1.0f },
            { SynthraformerType.Indestructible, 2.0f },
            { SynthraformerType.Amplifier, 0.5f },
            { SynthraformerType.Transmuter, 2.5f },
            { SynthraformerType.Catalyst, 3.0f },
            { SynthraformerType.Azure, 4.0f }
            // PrimalCore - not craftable
        };


        /*
         * Synthraformer types
         * As rainbow colors:
         * 
         * Red - rolls random rarity on a item (blackjack game based on rarities and mimic 21 card system)
         *      Applies to all items where rarity can be applied.
         *      
         * Orange - rolls a random ballistic type on ammo
         * 
         * Yellow - rerolls a list of traits on item removing existing ones
         *      Weapons and Ammo
         *      
         * Green - tries to roll indestructible flag (rare to drop, 50% chance)
         *      All items that can break
         *      
         * Blue - rerolls a random chosed parameter/stat on item
         *      All items that can have that roll
         * 
         * Indigo - TBA
         * 
         * Violet - Catalyst, turns any weapon into augment so its now an integrated augment with random augment stats too.
         *      Blue can apply and roll weapon stats.
         *      
         * Azure - TBA
         * 
         * White - basic crafting material that drops from item dissasembly unless there is a different color to drop
         */


        public enum SynthraformerType
        {
            PrimalCore, // White - basic crafting material             // In Use
            Rarity, // Red - applies random rarity onitem              // In Use
            Infuser, // Orange - nothing for now                       // Nothing
            Traits, // Yellow - rerolls traits on ite                  // In Use
            Indestructible, // Green - roll indestructible flag        // In Use
            Amplifier, // Blue - rerolls single random stat on item    // In Use
            Transmuter,  // Indigo - nothing for now                   // Nothing
            Catalyst, // Violet - turn weapon into augment             // In Use
            Azure, // - nothing for now                                // Nothing
        }

        public void AddItems()
        {
            RemoveExistingSynthraformerRecipes();
            recipesOutputItems.Clear();

            var types = Enum.GetValues(typeof(SynthraformerType)).OfType<SynthraformerType>().ToArray();

            foreach (var type in types)
                CreateItem(type);

            // var stats = BlackJackRollerSimulator.RunSimulation(maxLevel: 5, rollCount: 100000);
            // BlackJackRollerSimulator.PrintReport(stats);

        }

        public static string MakeId(SynthraformerType type) => $"{nameBase}_{(int)type}";
        public static bool Is(BasePickupItem item) => item?.Id != null && Is(item.Id);
        public static bool Is(string item) => item?.Contains(nameBase) == true;
        public string GetBaseDrop() => MakeId(SynthraformerType.Amplifier);

        private void CreateItem(SynthraformerType type)
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
                RequiredItems = new List<ItemQuantity>()
            };

            SetupSynthraformerRecipe(type, itemRecipe);

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

        private void SetupSynthraformerRecipe(SynthraformerType type, ItemProduceReceipt recipe)
        {
            /*
                Revised Recipe Tree (Total items per recipe ≤ 7)

                PrimalCore (drop)
                │
                └─▶ Amplifier (7 Core) 
                     │
                     ├─▶ Traits (3 Amp + 4 Core = 7)
                     │    │
                     │    ├─▶ Rarity (3 Traits + 4 Amp = 7)
                     │    │    │
                     │    │    ├─▶ Transmuter (2 Rarity + 1 Amp + 4 Core = 7)
                     │    │    │
                     │    │    └─▶ Catalyst (2 Rarity + 1 Transmuter + 4 Core = 7)
                     │    │
                     │    └─▶ Indestructible (2 Traits + 2 Amp + 3 Core = 7)
                     │
                     └─▶ Infuser (3 Traits + 4 Rarity = 7) → future
            */

            // Clear any existing requirements (defensive)
            recipe.RequiredItems.Clear();

            // float produceTime = type switch
            // {
            //     SynthraformerType.Amplifier => 0.5f,
            //     SynthraformerType.Traits => 1.0f,
            //     SynthraformerType.Rarity => 1.5f,
            //     SynthraformerType.Indestructible => 2.0f,
            //     SynthraformerType.Infuser => 2.0f,
            //     SynthraformerType.Transmuter => 2.5f,
            //     SynthraformerType.Catalyst => 3.0f,
            //     SynthraformerType.Azure => 4.0f,
            //     _ => 0.0f // PrimalCore = not craftable
            // };

            float produceTime = ProduceTimeMap.TryGetValue(type, out var time) ? time : 0.0f;

            recipe.ProduceTimeInHours = produceTime;

            // Define recipe per type
            switch (type)
            {
                case SynthraformerType.PrimalCore:
                    // Not craftable — base material (drop-only)
                    break;

                case SynthraformerType.Amplifier:
                    recipe.RequiredItems.Add(new ItemQuantity(MakeId(SynthraformerType.PrimalCore), 7));
                    break;

                case SynthraformerType.Traits:
                    recipe.RequiredItems.Add(new ItemQuantity(MakeId(SynthraformerType.Amplifier), 3));
                    recipe.RequiredItems.Add(new ItemQuantity(MakeId(SynthraformerType.PrimalCore), 4));
                    break;

                case SynthraformerType.Rarity:
                    recipe.RequiredItems.Add(new ItemQuantity(MakeId(SynthraformerType.Traits), 3));
                    recipe.RequiredItems.Add(new ItemQuantity(MakeId(SynthraformerType.Amplifier), 4));
                    break;

                case SynthraformerType.Indestructible:
                    recipe.RequiredItems.Add(new ItemQuantity(MakeId(SynthraformerType.Traits), 2));
                    recipe.RequiredItems.Add(new ItemQuantity(MakeId(SynthraformerType.Amplifier), 2));
                    recipe.RequiredItems.Add(new ItemQuantity(MakeId(SynthraformerType.PrimalCore), 3));
                    break;

                case SynthraformerType.Infuser:
                    //recipe.RequiredItems.Add(new ItemQuantity(MakeId(SynthraformerType.Traits), 3));
                    //recipe.RequiredItems.Add(new ItemQuantity(MakeId(SynthraformerType.Rarity), 4));
                    break;

                case SynthraformerType.Transmuter:
                    //recipe.RequiredItems.Add(new ItemQuantity(MakeId(SynthraformerType.Rarity), 2));
                    //recipe.RequiredItems.Add(new ItemQuantity(MakeId(SynthraformerType.Amplifier), 1));
                    //recipe.RequiredItems.Add(new ItemQuantity(MakeId(SynthraformerType.PrimalCore), 4));
                    break;

                case SynthraformerType.Catalyst:
                    recipe.RequiredItems.Add(new ItemQuantity(MakeId(SynthraformerType.Traits), 1));
                    recipe.RequiredItems.Add(new ItemQuantity(MakeId(SynthraformerType.Rarity), 2));
                    recipe.RequiredItems.Add(new ItemQuantity(MakeId(SynthraformerType.PrimalCore), 4));
                    break;

                case SynthraformerType.Azure:
                    //recipe.RequiredItems.Add(new ItemQuantity(MakeId(SynthraformerType.Catalyst), 1));
                    //recipe.RequiredItems.Add(new ItemQuantity(MakeId(SynthraformerType.PrimalCore), 6));
                    break;

                default:
                    Plugin.Logger.Log($"Unknown SynthraformerType: {type}");
                    break;
            }

            if (recipe.RequiredItems.Count > 0)
            {
                AddRecipe(recipe);
            }
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

            var metadata = RecordCollection.MetadataWrapperRecords.GetOrAdd(targetItem.Id, MetadataWrapper.SplitItemUid);

            Plugin.Logger.Log($"metadata.RarityClass {metadata.RarityClass}");
            Plugin.Logger.Log($"metadata.Id {metadata.Id}");
            Plugin.Logger.Log($"metadata.ReturnItemUid {metadata.ReturnItemUid()}");

            CompositeItemRecord obj = Data.Items.GetRecord(targetItem.Id) as CompositeItemRecord;

            switch (record.Type)
            {
                case SynthraformerType.Rarity:
                    HandleRarity(targetItem, repair, record, metadata, obj, ref __result);
                    break;

                case SynthraformerType.Infuser:
                    //HandleInfuser(targetItem, repair, record, metadata, obj, ref __result);
                    break;

                case SynthraformerType.Traits:
                    HandleTraits(targetItem, repair, record, metadata, obj, ref __result);
                    break;

                case SynthraformerType.Indestructible:
                    HandleIndestructible(targetItem, repair, record, metadata, obj, ref __result);

                    break;
                case SynthraformerType.Amplifier:
                    HandleAmplifier(targetItem, repair, record, metadata, obj, ref __result);
                    break;

                case SynthraformerType.Transmuter:
                    //HandleTransmuter(targetItem, repair, record, metadata, obj, ref __result);
                    break;

                case SynthraformerType.Catalyst:
                    HandleCatalyst(targetItem, repair, record, metadata, obj, ref __result);
                    break;

                case SynthraformerType.Azure:
                    HandleAzure(targetItem, repair, record, metadata, obj, ref __result);
                    break;

                default:
                    __result = false;
                    break;
            }
        }

        private void HandleAzure(PickupItem targetItem, BasePickupItem repair, SynthraformerRecord record, MetadataWrapper metadata, CompositeItemRecord obj, ref bool __result)
        {
            foreach (var basePickupItemRecord in obj.Records)
            {
                switch (basePickupItemRecord)
                {
                    case AmmoRecord ammoRecord:
                        PathOfQuasimorph.itemRecordsControllerPoq.ammoRecordProcessorPoq.Init(ammoRecord, metadata.RarityClass, false, false, metadata.ReturnItemUid(), metadata.Id);
                        PathOfQuasimorph.itemRecordsControllerPoq.ammoRecordProcessorPoq.RerollBallisticType(record, metadata);
                        __result = true;
                        return;
                }
            }
        }

        private void HandleTransmuter(PickupItem targetItem, BasePickupItem repair, SynthraformerRecord record, MetadataWrapper metadata, CompositeItemRecord obj, ref bool __result)
        {
            foreach (var basePickupItemRecord in obj.Records)
            {
                switch (basePickupItemRecord)
                {
                    case AmmoRecord ammoRecord:
                        PathOfQuasimorph.itemRecordsControllerPoq.ammoRecordProcessorPoq.Init(ammoRecord, metadata.RarityClass, false, false, metadata.ReturnItemUid(), metadata.Id);
                        PathOfQuasimorph.itemRecordsControllerPoq.ammoRecordProcessorPoq.RerollDamageType(record, metadata);
                        __result = true;
                        return;
                }
            }
        }

        private void HandleInfuser(PickupItem targetItem, BasePickupItem repair, SynthraformerRecord record, MetadataWrapper metadata, CompositeItemRecord obj, ref bool __result)
        {
            foreach (var basePickupItemRecord in obj.Records)
            {
                switch (basePickupItemRecord)
                {
                    case AmmoRecord ammoRecord:
                        PathOfQuasimorph.itemRecordsControllerPoq.ammoRecordProcessorPoq.Init(ammoRecord, metadata.RarityClass, false, false, metadata.ReturnItemUid(), metadata.Id);
                        PathOfQuasimorph.itemRecordsControllerPoq.ammoRecordProcessorPoq.RerollAmmoType(record, metadata);
                        __result = true;
                        return;
                }
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
                        PathOfQuasimorph.itemRecordsControllerPoq.ammoRecordProcessorPoq.Init(ammoRecord, metadata.RarityClass, false, false, metadata.ReturnItemUid(), metadata.Id);
                        PathOfQuasimorph.itemRecordsControllerPoq.ammoRecordProcessorPoq.RerollRandomStat(record, metadata);
                        break;

                    case WeaponRecord weaponRecord when !isStandard:
                        _logger.Log($"weaponRecord processing");
                        PathOfQuasimorph.itemRecordsControllerPoq.weaponRecordProcessorPoq.Init(weaponRecord, metadata.RarityClass, false, false, metadata.ReturnItemUid(), metadata.Id);
                        PathOfQuasimorph.itemRecordsControllerPoq.weaponRecordProcessorPoq.RerollRandomStat(record, metadata);
                        break;

                    case HelmetRecord helmetRecord when !isStandard:
                        _logger.Log($"helmetRecord processing");
                        PathOfQuasimorph.itemRecordsControllerPoq.helmetRecordProcessorPoq.Init(helmetRecord, metadata.RarityClass, false, false, metadata.ReturnItemUid(), metadata.Id);
                        PathOfQuasimorph.itemRecordsControllerPoq.helmetRecordProcessorPoq.RerollRandomStat(record, metadata);
                        break;

                    case ArmorRecord armorRecord when !isStandard:
                        _logger.Log($"armorRecord processing");
                        PathOfQuasimorph.itemRecordsControllerPoq.armorRecordProcessorPoq.Init(armorRecord, metadata.RarityClass, false, false, metadata.ReturnItemUid(), metadata.Id);
                        PathOfQuasimorph.itemRecordsControllerPoq.armorRecordProcessorPoq.RerollRandomStat(record, metadata);
                        break;

                    case LeggingsRecord leggingsRecord when !isStandard:
                        _logger.Log($"leggingsRecord processing");
                        PathOfQuasimorph.itemRecordsControllerPoq.leggingsRecordProcessorPoq.Init(leggingsRecord, metadata.RarityClass, false, false, metadata.ReturnItemUid(), metadata.Id);
                        PathOfQuasimorph.itemRecordsControllerPoq.leggingsRecordProcessorPoq.RerollRandomStat(record, metadata);
                        break;

                    case BootsRecord bootsRecord when !isStandard:
                        _logger.Log($"bootsRecord processing");
                        PathOfQuasimorph.itemRecordsControllerPoq.bootsRecordProcessorPoq.Init(bootsRecord, metadata.RarityClass, false, false, metadata.ReturnItemUid(), metadata.Id);
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
                        PathOfQuasimorph.itemRecordsControllerPoq.weaponRecordProcessorPoq.Init(weaponRecord, metadata.RarityClass, false, false, metadata.ReturnItemUid(), metadata.Id);
                        PathOfQuasimorph.itemRecordsControllerPoq.weaponRecordProcessorPoq.ReplaceWeaponTraits(record, metadata);
                        break;

                    case AmmoRecord ammoRecord:
                        PathOfQuasimorph.itemRecordsControllerPoq.ammoRecordProcessorPoq.Init(ammoRecord, metadata.RarityClass, false, false, metadata.ReturnItemUid(), metadata.Id);
                        PathOfQuasimorph.itemRecordsControllerPoq.ammoRecordProcessorPoq.ReplaceAmmoTraits(record, metadata);
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


        private void HandleRarity(PickupItem targetItem, BasePickupItem repair, SynthraformerRecord record, MetadataWrapper metadata, CompositeItemRecord obj, ref bool __result)
        {
            Plugin.Logger.Log($"HandleRarity");

            // Get current rarity from item 
            var currentRarity = (int)metadata.RarityClass;

            // Roll the new rarity using blackjack logic
            // Draw from blackjack deck
            var (rolledRarity, wasCritical, didBust) = PathOfQuasimorph.raritySystem.blackJackRoller.Draw();

            // We simply roll rarity i.e. just generatin new item rarity and new item for it.
            __result = CreateNewItem(targetItem, repair, (ItemRarity)rolledRarity, false, true);
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
                        PathOfQuasimorph.itemRecordsControllerPoq.breakableItemProcessorPoq.Init(breakableItemRecord, metadata.RarityClass, false, false, metadata.ReturnItemUid(), metadata.Id);
                        PathOfQuasimorph.itemRecordsControllerPoq.breakableItemProcessorPoq.AddUnbreakableTrait(record, metadata, 0.5f);
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
            _logger.Log($"hasWeaponRecord: {hasWeaponRecord}, hasAugmentationRecord: {hasAugmentationRecord}");

            foreach (var basePickupItemRecord in obj.Records)
            {
                switch (basePickupItemRecord)
                {
                    case WeaponRecord weaponRecord when !hasAugmentationRecord:
                        PathOfQuasimorph.itemRecordsControllerPoq.weaponRecordProcessorPoq.Init(weaponRecord, metadata.RarityClass, false, false, metadata.ReturnItemUid(), metadata.Id);
                        PathOfQuasimorph.itemRecordsControllerPoq.weaponRecordProcessorPoq.CreateAugmentation(record, metadata, obj);
                        break;

                    case AugmentationRecord augmentationRecord:
                        PathOfQuasimorph.itemRecordsControllerPoq.augmentationRecordProcessorPoq.Init(augmentationRecord, metadata.RarityClass, false, false, metadata.ReturnItemUid(), metadata.Id);
                        PathOfQuasimorph.itemRecordsControllerPoq.augmentationRecordProcessorPoq.AddRandomEffect(record, metadata);
                        break;

                    case ImplantRecord implantRecord:
                        PathOfQuasimorph.itemRecordsControllerPoq.implantRecordProcessorPoq.Init(implantRecord, metadata.RarityClass, false, false, metadata.ReturnItemUid(), metadata.Id);
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

            string newId = applyRarity
                ? PathOfQuasimorph.itemRecordsControllerPoq.InterceptAndReplaceItemId(
                    Id: MetadataWrapper.GetBaseId(target.Id),
                    mobRarityBoost: false,
                    itemRarity: itemRarity,
                    selectRarity: selectRarity,
                    ignoreBlacklist: true,
                    randomUidInjected: null,
                    applyRarity: applyRarity)
                : target.Id;

            // Verify rarity class in metadata
            var metadata = RecordCollection.MetadataWrapperRecords.GetOrAdd(newId, MetadataWrapper.SplitItemUid);

            if (metadata.RarityClass != itemRarity)
            {
                Plugin.Logger.Log($"Updating metadata rarity: {metadata.RarityClass} > {itemRarity}");

                metadata.UpdateRarityClass(itemRarity);
                newId = metadata.ReturnItemUid();
            }

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
                if (newComp.GetType().Name == "BreakableItemComponent")
                {
                    _logger.Log($"BreakableItemComponent skip");
                    continue;
                }
           
                // Find old component with same concrete type
                var oldComp = pickupItemOld.Components.FirstOrDefault(c => c?.GetType() == newComp.GetType());

                _logger.Log($"newComp: {newComp.GetType().Name}, old is null: {oldComp == null}");

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

                    _logger.Log($"prop: {prop.Name}, value: {value}");

                }
                catch (Exception ex)
                {
                    Plugin.Logger.Log($"Failed to copy {prop.Name}: {ex.Message}");
                }
            }
        }

        public static string FixOldId(string id)
        {
            var array = id.Split('_');
            // synthraformer_poq_1 = count 3
            // synthraformer_poq_1_standard = count 4

            if (array.Length == 4)
            {
                return string.Join("_", array[0], array[1], array[2]);
            }

            return id;
        }

        internal List<string> GetAdditionalDroptems(BasePickupItem item, MetadataWrapper metadata)
        {
            _logger.Log($"GetAdditionalDroptems");

            var itemsList = new List<string>();

            var roll = Helpers._random.NextDouble();

            foreach (var kvp in DropChances)
            {
                SynthraformerType type = kvp.Key;
                float baseChance = kvp.Value;
                float finalChance = baseChance;

                // Adjust chance based on item properties
                switch (type)
                {
                    case SynthraformerType.PrimalCore:
                        break; // Use base chance

                    case SynthraformerType.Rarity:
                        finalChance *= (int)metadata.RarityClass;
                        break;

                    case SynthraformerType.Traits:
                        if (item.Is<WeaponRecord>())
                        {
                            finalChance *= item.Record<WeaponRecord>().Traits.Count;
                        }
                        else
                            continue; // Skip if no traits
                        break;

                    case SynthraformerType.Indestructible:
                        if (item.Is<BreakableItemRecord>())
                        {
                            if (item.Record<BreakableItemRecord>().Unbreakable)
                            {
                                finalChance *= 1.5f;
                            }
                        }

                        else
                            continue; // Skip if not unbreakable
                        break;

                    case SynthraformerType.Amplifier:
                        if (item.Is<WeaponRecord>() || item.Is<ResistRecord>() || item.Is<ImplantRecord>() || item.Is<AugmentationRecord>())
                        {
                            // Use base chance
                            break;
                        }
                        else
                            continue; // Skip if not unbreakable

                    case SynthraformerType.Catalyst:
                        if (item.Is<AugmentationRecord>() || item.Is<ImplantRecord>())
                        {
                            // Use base chance
                            break;
                        }
                        else
                            continue; // Skip if not unbreakable

                    default:
                        continue; // Skip types that don't drop items (Infuser, Transmuter, Azure, etc.)
                }

                // Clamp and check roll
                finalChance = Mathf.Clamp01(finalChance);

                Plugin.Logger.Log($"roll: {roll}, finalChance: {finalChance}, type: {type}");

                if (roll < finalChance)
                {
                    Plugin.Logger.Log($"adding {type}");

                    itemsList.Add(MakeId(type));
                }
            }

            return itemsList;
        }
    }
}
