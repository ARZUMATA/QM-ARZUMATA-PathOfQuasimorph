using MGSC;
using Newtonsoft.Json;
using QM_PathOfQuasimorph.Core.Processors;
using QM_PathOfQuasimorph.Core.Records;
using QM_PathOfQuasimorph.PoQHelpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static QM_PathOfQuasimorph.Core.MagnumPoQProjectsController;
using static UnityEngine.Rendering.Universal.TemporalAA;
using static UnityEngine.UI.Image;
using Type = System.Type;

namespace QM_PathOfQuasimorph.Core
{
    internal class ItemRecordsControllerPoq
    {
        internal AugmentationRecordProcessorPoq augmentationRecordProcessorPoq;
        internal ImplantRecordProcessorPoq implantRecordProcessorPoq;
        internal WeaponRecordProcessorPoq weaponRecordProcessorPoq;
        internal HelmetRecordProcessorPoq helmetRecordProcessorPoq;
        internal ArmorRecordProcessorPoq armorRecordProcessorPoq;
        internal LeggingsRecordProcessorPoq leggingsRecordProcessorPoq;
        internal BootsRecordProcessorPoq bootsRecordProcessorPoq;
        internal WoundSlotRecordProcessorPoq woundSlotRecordProcessorPoq;
        public ItemProduceReceipt itemProduceReceiptPlaceHolder = null;

        private Logger _logger = new Logger(null, typeof(ItemRecordsControllerPoq));

        internal ItemRecordsControllerPoq()
        {
            augmentationRecordProcessorPoq = new AugmentationRecordProcessorPoq(this);
            implantRecordProcessorPoq = new ImplantRecordProcessorPoq(this);
            weaponRecordProcessorPoq = new WeaponRecordProcessorPoq(this);
            helmetRecordProcessorPoq = new HelmetRecordProcessorPoq(this);
            armorRecordProcessorPoq = new ArmorRecordProcessorPoq(this);
            leggingsRecordProcessorPoq = new LeggingsRecordProcessorPoq(this);
            bootsRecordProcessorPoq = new BootsRecordProcessorPoq(this);
            woundSlotRecordProcessorPoq = new WoundSlotRecordProcessorPoq(this);
        }

        internal string CreateNew(string itemIdOrigin, bool mobRarityBoost)
        {
            _logger.Log($"CreateNew");

            var itemRarity = PathOfQuasimorph.raritySystem.SelectRarity();
            _logger.Log($"\t itemRarity {itemRarity}");

            /* 
             * We can't drop processing even if rarity standard and we do nothing.            
             * Reason for it is simple: if we put non-rarity augment on the mercenary and remove augment back,
             * it triggers item creation and we can't decide if we need to pass it as is or apply rarity as item is still non-rarity.
             * Tracking such items in some controller class is weird and is prone to even more errors so it's better to just create a new item completely
             * and apply Standard rarity and do nothing.
            */

            //if (itemRarity == ItemRarity.Standard)
            //{
            //    return itemIdOrigin;
            //}

            if (!PathOfQuasimorph.itemRecordsControllerPoq.CanProcessItemRecord(itemIdOrigin))
            {
                _logger.Log($"\t CanProcessItemRecord FALSE, returning generic");
                return itemIdOrigin;
            }

            // Generate a new UID
            var randomUid = Helpers.UniqueIDGenerator.GenerateRandomIDWith16Characters();
            DigitInfo digits = DigitInfo.GetDigits(randomUid);
            digits.FillZeroes();
            digits.Rarity = (int)itemRarity;
            var randomUidInjected = digits.ReturnUID();

            _logger.Log($"\t randomUidInjected: {randomUidInjected}");


            return CreateNew(itemIdOrigin, mobRarityBoost, itemRarity, randomUidInjected);
        }

        internal string CreateNew(string itemIdOrigin, bool mobRarityBoost, ItemRarity itemRarity, string randomUidInjected)
        {
            // Resulting UID
            var wrapper = new MetadataWrapper(
                 id: itemIdOrigin,
                 poqItem: true,
                 startTime: new DateTime(MagnumPoQProjectsController.MAGNUM_PROJECT_START_TIME),
                 finishTime: new DateTime(Int64.Parse(randomUidInjected))
                 );

            var newId = wrapper.ReturnItemUid();

            _logger.Log($"\t newId: {newId}");

            CompositeItemRecord obj = Data.Items.GetRecord(itemIdOrigin) as CompositeItemRecord;
            CompositeItemRecord newObj = new CompositeItemRecord(newId);

            _logger.Log($"Checking ItemTransformationRecord");

            ItemTransformationRecord itemTransformationRecord = Data.ItemTransformation.GetRecord(itemIdOrigin);

            if (itemTransformationRecord == null)
            {
                _logger.Log($"ItemTransformationRecord is missing for itemIdOrigin: {itemIdOrigin}.");
                _logger.Log($"Need a placeholder");

                // Item breaks into this, unless it has it's own itemTransformationRecord.
                itemTransformationRecord = Data.ItemTransformation.GetRecord("prison_tshirt_1", true);
            }

            _logger.Log($" Cloning and adding record record.");

            var itemTransformationRecordNew = itemTransformationRecord.Clone(newId);

            // Check amplifier drop chance
            var canApplyAmplifierDrop = Helpers._random.Next(0, 100 + 1) < AmplifierController.DROP_CHANCE;

            if (canApplyAmplifierDrop)
            {
                var amplifierName = PathOfQuasimorph.amplifierController.GetAmplifierNameFromRarity(itemRarity);
                itemTransformationRecordNew.OutputItems.Add(new ItemQuantity(amplifierName, 1));
            }

            Data.ItemTransformation.AddRecord(newId, itemTransformationRecordNew);

            _logger.Log($"ItemTransformationRecord: result will be item count {itemTransformationRecord.OutputItems.Count}");

            string boostedParamString = string.Empty;
            ApplyRarityStats(obj.Records, newObj.Records, itemRarity, mobRarityBoost, newId, itemIdOrigin, ref boostedParamString);
            wrapper.BoostedString = boostedParamString;

            _logger.Log($"");
            foreach (var recordEntry in newObj.Records)
            {
                // Replace Id since we have new one now
                //recordEntry.Id = newId;
                //Data.Items.AddRecord(itemId, recordEntry);
                _logger.Log($"\t recordEntry newObject {recordEntry.Id}");
                _logger.Log($"\t\t recordEntry Name {recordEntry.GetType().Name}");
                _logger.Log($"\t\t recordEntry id {recordEntry.Id}");

                _logger.Log($"\t\t recordEntry PrimaryRecord Name {newObj.PrimaryRecord.GetType().Name}");
                _logger.Log($"\t\t recordEntry newObject id {newObj.PrimaryRecord.Id}");

                // We can add records one by one which is OK if we one day start creating weaponized-augmentations
                // or we can just do
                // Data.Items._records.Add(newId, newObj);
                // for now we use in-game methodsm, and directly add _records during loading saved data before
                Data.Items.AddRecord(newId, recordEntry);
            }

            // newId = ParseHelper.ParseId(newId);
            _logger.Log($"");

            foreach (var recordEntry in obj.Records)
            {
                _logger.Log($"\t recordEntry oldObject {recordEntry.Id}");
                _logger.Log($"\t\t recordEntry Name {recordEntry.GetType().Name}");
                _logger.Log($"\t\t recordEntry id {recordEntry.Id}");

                _logger.Log($"\t\t recordEntry PrimaryRecord Name {obj.PrimaryRecord.GetType().Name}");
                _logger.Log($"\t\t recordEntry newObject id {obj.PrimaryRecord.Id}");
            }

            _logger.Log($"recordEntry.GetType().Name {obj.PrimaryRecord.GetType().Name}");

            //Data.Items.AddRecord(newId, newObj);
            RecordCollection.ItemRecords.Add(newId, newObj);
            RecordCollection.MetadataWrapperRecords.Add(newId, wrapper);

            _logger.Log($"itemId {Data.Items._records.Keys.Contains(newId)}");
            _logger.Log($"oldId {Data.Items._records.Keys.Contains(itemIdOrigin)}");

            Localization.DuplicateKey("item." + itemIdOrigin + ".name", "item." + newId + ".name");
            Localization.DuplicateKey("item." + itemIdOrigin + ".shortdesc", "item." + newId + ".shortdesc");
            RaritySystem.AddAffixes(newId);

            return newId;
        }

        public string GetItemBoostedString(string itemId)
        {
            return RecordCollection.GetBoostedString(itemId);
        }

        private void ApplyRarityStats(
            List<BasePickupItemRecord> oldRecords,
            List<BasePickupItemRecord> records,
            ItemRarity itemRarity, bool mobRarityBoost, string itemId, string oldId, ref string boostedParamString)
        {
            _logger.Log($"ApplyRarityStats");

            // Iterate over item records, apply parameters, return ready to go item itemTransformationRecord.

            _logger.Log($"Iterating records list with count of {oldRecords.Count}");

            foreach (BasePickupItemRecord basePickupItemRecord in oldRecords)
            {
                _logger.Log($"basePickupItemRecord: {basePickupItemRecord.Id}");

                WeaponRecord weaponRecord = basePickupItemRecord as WeaponRecord;

                if (weaponRecord != null)
                {
                    _logger.Log($"weaponRecord processing");

                    WeaponRecord weaponRecordNew = ItemRecordHelpers.CloneWeaponRecord(weaponRecord, itemId);
                    //WeaponRecord weaponRecordNew = weaponRecord.Clone(itemId);
                    // WeaponRecord weaponRecordNew = weaponRecord.Clone($"*{itemId}");
                    weaponRecordProcessorPoq.Init(weaponRecordNew, itemRarity, mobRarityBoost, false, itemId, oldId);
                    weaponRecordProcessorPoq.ProcessRecord(ref boostedParamString);
                    records.Add(weaponRecordNew);
                }

                HelmetRecord helmetRecord = basePickupItemRecord as HelmetRecord;

                if (helmetRecord != null)
                {
                    _logger.Log($"helmetRecord processing");

                    HelmetRecord helmetRecordNew = helmetRecord.Clone(itemId);
                    helmetRecordProcessorPoq.Init(helmetRecordNew, itemRarity, mobRarityBoost, false, itemId, oldId);
                    helmetRecordProcessorPoq.ProcessRecord(ref boostedParamString);
                    records.Add(helmetRecordNew);
                }

                ArmorRecord armorRecord = basePickupItemRecord as ArmorRecord;

                if (armorRecord != null)
                {
                    _logger.Log($"armorRecord processing");

                    ArmorRecord armorRecordNew = armorRecord.Clone(itemId);
                    armorRecordProcessorPoq.Init(armorRecordNew, itemRarity, mobRarityBoost, false, itemId, oldId);
                    armorRecordProcessorPoq.ProcessRecord(ref boostedParamString);
                    records.Add(armorRecordNew);
                }

                LeggingsRecord leggingsRecord = basePickupItemRecord as LeggingsRecord;

                if (leggingsRecord != null)
                {
                    _logger.Log($"leggingsRecord processing");

                    LeggingsRecord leggingsRecordNew = leggingsRecord.Clone(itemId);
                    leggingsRecordProcessorPoq.Init(leggingsRecordNew, itemRarity, mobRarityBoost, false, itemId, oldId);
                    leggingsRecordProcessorPoq.ProcessRecord(ref boostedParamString);
                    records.Add(leggingsRecordNew);
                }

                BootsRecord bootsRecord = basePickupItemRecord as BootsRecord;

                if (bootsRecord != null)
                {
                    _logger.Log($"bootsRecord processing");

                    BootsRecord bootsRecordNew = bootsRecord.Clone(itemId);
                    bootsRecordProcessorPoq.Init(bootsRecordNew, itemRarity, mobRarityBoost, false, itemId, oldId);
                    bootsRecordProcessorPoq.ProcessRecord(ref boostedParamString);
                    records.Add(bootsRecordNew);
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

                    ImplantRecord implantRecordNew = ItemRecordHelpers.CloneImplantRecord(implantRecord, itemId);
                    implantRecordProcessorPoq.Init(implantRecordNew, itemRarity, mobRarityBoost, false, itemId, oldId);
                    implantRecordProcessorPoq.ProcessRecord(ref boostedParamString);
                    records.Add(implantRecordNew);
                }

                AugmentationRecord augmentationRecord = basePickupItemRecord as AugmentationRecord;

                if (augmentationRecord != null)
                {
                    _logger.Log($"augmentationRecord processing");

                    AugmentationRecord augmentationRecordNew = ItemRecordHelpers.CloneAugmentationRecord(augmentationRecord, itemId);
                    augmentationRecordProcessorPoq.Init(augmentationRecordNew, itemRarity, mobRarityBoost, false, itemId, oldId);
                    augmentationRecordProcessorPoq.ProcessRecord(ref boostedParamString);
                    records.Add(augmentationRecordNew);
                }
            }
        }

        internal List<string> GetAddeableTraits(ItemTraitType itemTraitType)
        {
            List<string> addeableTraits = new List<string>();

            foreach (var param in Data.ItemTraits._records)
            {
                if (param.Value.ItemTraitType == itemTraitType)
                {
                    addeableTraits.Add(param.Value.Id);
                }
            }

            return addeableTraits;
        }

        public bool ShouldHinderParameter(ref int hinderedCount, ref int improvedCount, int numParamsToHinder, int numParamsToImprove)
        {
            // 20% chance to hinder first, regardless of improvement status
            if (Helpers._random.Next(0, 100 + 1) < 20)
            {
                if (hinderedCount < numParamsToHinder)
                {
                    hinderedCount++;
                    return true;
                }
                else
                {
                    // Can't hinder anymore, so improve
                    improvedCount++;
                    return false;
                }
            }

            // After 20% chance, follow the original logic
            if (hinderedCount < numParamsToHinder && improvedCount >= numParamsToImprove)
            {
                // 50/50 chance to hinder a parameter (after improvement threshold is met)
                if (Helpers._random.Next(0, 100 + 1) < PathOfQuasimorph.raritySystem.PARAMETER_HINDER_CHANCE)
                {
                    hinderedCount++;
                    return true;
                }
                else
                {
                    improvedCount++;
                    return false;
                }
            }
            else if (improvedCount < numParamsToImprove)
            {
                // Otherwise improve as usual
                improvedCount++;
                return false;
            }

            return false;
        }

        public ItemProduceReceipt GetPlaceHolderItemProduceReceipt()
        {
            // If we already found it, reuse it as iteration and calls are very intensive for the hook.
            if (itemProduceReceiptPlaceHolder != null && itemProduceReceiptPlaceHolder.Id != string.Empty)
            {
                return itemProduceReceiptPlaceHolder;
            }

            // Iterate whole receipts do find our placeholder.
            for (int i = 0; i < Data.ProduceReceipts.Count; i++)
            {
                // Item has no recipe, let's add placeholder as we won't see it in recipe's list anyway.
                if (Data.ProduceReceipts[i].OutputItem == "pills_sorbent")
                {
                    itemProduceReceiptPlaceHolder = Data.ProduceReceipts[i];
                    return itemProduceReceiptPlaceHolder;
                }
            }

            return itemProduceReceiptPlaceHolder;
        }

        public bool CanProcessItemRecord(string id)
        {
            // Don't process by default anything
            bool canProcess = false;

            // Blacklist some items
            List<string> blacklistedCategories = new List<string>
                {
                    //"Possessed",
                    //"CyberAug",
                    //"PossessedAug",
                    //"QuasiAug",
                    "none"
                };

            CompositeItemRecord compositeItemRecord = Data.Items.GetRecord(id, true) as CompositeItemRecord;

            foreach (var rec in compositeItemRecord.Records)
            {
                Type recordType = rec.GetType();
                bool checkWeaponRecord = false;
                bool checkAugmentationRecord = false;
                bool checkImplantRecord = false;

                switch (recordType.Name)
                {
                    case nameof(WeaponRecord):
                        // Check weapon record first
                        checkWeaponRecord = true;
                        break;
                    case nameof(ArmorRecord):
                    case nameof(HelmetRecord):
                    case nameof(LeggingsRecord):
                    case nameof(BootsRecord):
                        canProcess = true;
                        break;
                    case nameof(AugmentationRecord):
                        checkAugmentationRecord = true;
                        //canProcess = false;
                        break;
                    case nameof(ImplantRecord):
                        checkImplantRecord = true;
                        //canProcess = false;
                        break;
                    default:
                        _logger.Log($"canProcess = false : {recordType.Name}");
                        canProcess = false;
                        break;
                }

                if (checkWeaponRecord)
                {
                    // We can process weapons except some cases that will be checked later.
                    canProcess = true;
                    var weaponRecord = rec as WeaponRecord;

                    if (weaponRecord != null)
                    {
                        //_logger.Log($"\t\t\t IsImplicit {weaponRecord.IsImplicit}");
                        if (weaponRecord.IsImplicit)
                        {
                            _logger.Log($"canProcess = false : {weaponRecord.Id} - weaponRecord.IsImplicit");
                            canProcess = false;
                            break;
                        }

                        foreach (var mod in weaponRecord.Categories)
                        {
                            if (blacklistedCategories.Contains(mod))
                            {
                                _logger.Log($"canProcess = false : {weaponRecord.Id}, {mod} - blacklistedCategories");

                                canProcess = false;
                                break;
                            }

                            //_logger.Log($"\t\t\t Category  {mod}");
                        }

                        //_logger.Log($"\t\t\t ItemClass {weaponRecord.ItemClass}");
                        //_logger.Log($"\t\t\t WeaponClass {weaponRecord.WeaponClass}");
                    }
                }

                if (checkAugmentationRecord)
                {
                    var augmentationRecord = rec as AugmentationRecord;
                    if (augmentationRecord != null)
                    {
                    }
                }
            }

            return canProcess;
        }

        internal bool ChangeRecordFromAmplifier(BasePickupItem target, BasePickupItem repair)
        {
            _logger.Log($"ChangeRecordFromAmplifier");

            var hasKey = RecordCollection.MetadataWrapperRecords.TryGetValue(target.Id, out MetadataWrapper metadata);
            var ampRecord = repair.Record<AmplifierRecord>();

            if (!hasKey || !metadata.PoqItem)
            {
                _logger.Log($"ChangeRecordFromAmplifier: Failure!");

                return false;
            }

            CompositeItemRecord obj = Data.Items.GetRecord(target.Id) as CompositeItemRecord;

            foreach (BasePickupItemRecord basePickupItemRecord in obj.Records)
            {
                WeaponRecord weaponRecord = basePickupItemRecord as WeaponRecord;

                if (weaponRecord != null)
                {
                    _logger.Log($"weaponRecord processing");

                    weaponRecordProcessorPoq.Init(weaponRecord, metadata.RarityClass, false, false, metadata.Id, metadata.ReturnItemUid());
                    weaponRecordProcessorPoq.Reroll(ampRecord, metadata);
                }
            }

            _logger.Log($"ChangeRecordFromAmplifier: Success!");

            return true;
        }

        internal bool ReplaceWeaponTraits(BasePickupItem target, BasePickupItem repair)
        {
            _logger.Log($"ReplaceWeaponTraits");

            var hasKey = RecordCollection.MetadataWrapperRecords.TryGetValue(target.Id, out MetadataWrapper metadata);
            var recombRecord = repair.Record<RecombinatorRecord>();

            if (!hasKey || !metadata.PoqItem)
            {
                _logger.Log($"ReplaceWeaponTraits: Failure! hasKey / PoqItem");
                return false;
            }

            CompositeItemRecord obj = Data.Items.GetRecord(target.Id) as CompositeItemRecord;

            foreach (BasePickupItemRecord basePickupItemRecord in obj.Records)
            {
                WeaponRecord weaponRecord = basePickupItemRecord as WeaponRecord;

                if (weaponRecord != null)
                {
                    _logger.Log($"weaponRecord processing");

                    weaponRecordProcessorPoq.Init(weaponRecord, metadata.RarityClass, false, false, metadata.Id, metadata.ReturnItemUid());
                    weaponRecordProcessorPoq.ReplaceWeaponTraits(recombRecord, metadata);

                    var weaponComponent = target.Comp<WeaponComponent>();

                    if (weaponComponent != null)
                    {
                        weaponComponent.Traits.Clear();
                        foreach (var trait in weaponRecord.Traits)
                        {
                            weaponComponent.Traits.Add(ItemTraitSystem.CreateItemTrait(trait));
                        }
                        _logger.Log($"ReplaceWeaponTraits: Success!");

                        return true;
                    }
                }
            }




            _logger.Log($"ReplaceWeaponTraits: Failure!");
            return false;
        }
    }
}
