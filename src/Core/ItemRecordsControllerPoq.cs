using MGSC;
using Newtonsoft.Json;
using QM_PathOfQuasimorph.Core.Processors;
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
            Plugin.Logger.Log($"CreateNew");

            var itemRarity = PathOfQuasimorph.raritySystem.SelectRarity();
            Plugin.Logger.Log($"\t itemRarity {itemRarity}");

            if (itemRarity == ItemRarity.Standard)
            {
                return itemIdOrigin;
            }

            if (!PathOfQuasimorph.itemRecordsControllerPoq.CanProcessItemRecord(itemIdOrigin))
            {
                Plugin.Logger.Log($"\t CanProcessItemRecord FALSE, returning generic");
                return itemIdOrigin;
            }

            // Generate a new UID
            var randomUid = Helpers.UniqueIDGenerator.GenerateRandomIDWith16Characters();
            DigitInfo digits = DigitInfo.GetDigits(randomUid);
            digits.FillZeroes();
            digits.Rarity = (int)itemRarity;
            var randomUidInjected = digits.ReturnUID();

            Plugin.Logger.Log($"\t randomUidInjected: {randomUidInjected}");

            // Resulting UID
            var wrapper = new MetadataWrapper(
                 id: itemIdOrigin,
                 poqItem: true,
                 startTime: new DateTime(MagnumPoQProjectsController.MAGNUM_PROJECT_START_TIME),
                 finishTime: new DateTime(Int64.Parse(randomUidInjected))
                 );

            var newId = wrapper.ReturnItemUid();

            Plugin.Logger.Log($"\t newId: {newId}");

            CompositeItemRecord obj = Data.Items.GetRecord(itemIdOrigin) as CompositeItemRecord;
            CompositeItemRecord newObj = new CompositeItemRecord(newId);

            Localization.DuplicateKey("item." + itemIdOrigin + ".name", "item." + newId + ".name");
            Localization.DuplicateKey("item." + itemIdOrigin + ".shortdesc", "item." + newId + ".shortdesc");

            ItemTransformationRecord itemTransformationRecord = Data.ItemTransformation.GetRecord(itemIdOrigin);

            if (itemTransformationRecord == null || itemTransformationRecord.Id == string.Empty)
            {
                // Item breaks into this, unless it has it's own itemTransformationRecord.
                itemTransformationRecord = Data.ItemTransformation.GetRecord("prison_tshirt_1", true);
                Data.ItemTransformation.AddRecord(newId, itemTransformationRecord.Clone(newId));
            }

            ApplyRarityStats(obj.Records, newObj.Records, itemRarity, mobRarityBoost, newId);
          
            foreach (var recordEntry in newObj.Records)
            {
                // Replace Id since we have new one now
                //recordEntry.Id = newId;
                _logger.Log($"\t new recordEntry test {recordEntry.Id}");
                //Data.Items.AddRecord(itemId, recordEntry);
                Plugin.Logger.Log($"recordEntry.GetType().Name {recordEntry.GetType().Name}");

                Plugin.Logger.Log($"recordEntry.GetType().Name {newObj.PrimaryRecord.GetType().Name}");

                // We can add records one by one which is OK if we one day start creating weaponized-augmentations
                // or we can just do
                // Data.Items._records.Add(newId, newObj);
                // for now we use in-game methodsm, and directly add _records during loading saved data before
                Data.Items.AddRecord(newId, recordEntry);
            }

            foreach (var recordEntry in obj.Records)
            {
                _logger.Log($"\t old recordEntry test {recordEntry.Id}");
                Plugin.Logger.Log($"recordEntry.GetType().Name {recordEntry.GetType().Name}");
            }

            Plugin.Logger.Log($"recordEntry.GetType().Name {obj.PrimaryRecord.GetType().Name}");

            //Data.Items.AddRecord(newId, newObj);
            RecordCollection.ItemRecords.Add(newId, newObj);
            RecordCollection.MetadataWrapperRecords.Add(newId, wrapper);

            _logger.Log($"itemId {Data.Items._records.Keys.Contains(newId)}");
            _logger.Log($"oldId {Data.Items._records.Keys.Contains(itemIdOrigin)}");

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
            ItemRarity itemRarity, bool mobRarityBoost, string itemId)
        {
            _logger.Log($"ApplyRarityStats");

            // Iterate over item records, apply parameters, return ready to go item itemTransformationRecord.

            _logger.Log($"Iterating records list with count of {records.Count}");

            foreach (BasePickupItemRecord basePickupItemRecord in oldRecords)
            {
                _logger.Log($"itemTransformationRecord: {basePickupItemRecord.Id}");

                WeaponRecord weaponRecord = basePickupItemRecord as WeaponRecord;

                if (weaponRecord != null)
                {
                    _logger.Log($"weaponRecord processing");

                    WeaponRecord weaponRecordNew = weaponRecord.Clone(itemId);
                    weaponRecordProcessorPoq.Init(weaponRecordNew, itemRarity, mobRarityBoost, itemId);
                    weaponRecordProcessorPoq.ProcessRecord();
                    records.Add(weaponRecordNew);
                }

                HelmetRecord helmetRecord = basePickupItemRecord as HelmetRecord;

                if (helmetRecord != null)
                {
                    _logger.Log($"helmetRecord processing");

                    HelmetRecord helmetRecordNew = helmetRecord.Clone(itemId);
                    helmetRecordProcessorPoq.Init(helmetRecordNew, itemRarity, mobRarityBoost, itemId);
                    helmetRecordProcessorPoq.ProcessRecord();
                    records.Add(helmetRecordNew);
                }

                ArmorRecord armorRecord = basePickupItemRecord as ArmorRecord;

                if (armorRecord != null)
                {
                    _logger.Log($"armorRecord processing");

                    ArmorRecord armorRecordNew = armorRecord.Clone(itemId);
                    armorRecordProcessorPoq.Init(armorRecordNew, itemRarity, mobRarityBoost, itemId);
                    armorRecordProcessorPoq.ProcessRecord();
                    records.Add(armorRecordNew);
                }

                LeggingsRecord leggingsRecord = basePickupItemRecord as LeggingsRecord;

                if (leggingsRecord != null)
                {
                    _logger.Log($"leggingsRecord processing");

                    LeggingsRecord leggingsRecordNew = leggingsRecord.Clone(itemId);
                    leggingsRecordProcessorPoq.Init(leggingsRecordNew, itemRarity, mobRarityBoost, itemId);
                    leggingsRecordProcessorPoq.ProcessRecord();
                    records.Add(leggingsRecordNew);
                }

                BootsRecord bootsRecord = basePickupItemRecord as BootsRecord;

                if (bootsRecord != null)
                {
                    _logger.Log($"bootsRecord processing");

                    BootsRecord bootsRecordNew = bootsRecord.Clone(itemId);
                    bootsRecordProcessorPoq.Init(bootsRecordNew, itemRarity, mobRarityBoost, itemId);
                    bootsRecordProcessorPoq.ProcessRecord();
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
                    if (implantRecord.IsActive == false)
                    {
                        _logger.Log($"implantRecord processing");

                        ImplantRecord implantRecordNew = ItemRecordHelpers.CloneImplantRecord(implantRecord, itemId);
                        implantRecordProcessorPoq.Init(implantRecordNew, itemRarity, mobRarityBoost, itemId);
                        implantRecordProcessorPoq.ProcessRecord();
                        records.Add(implantRecordNew);
                    }
                }

                AugmentationRecord augmentationRecord = basePickupItemRecord as AugmentationRecord;

                if (augmentationRecord != null)
                {
                    _logger.Log($"augmentationRecord processing");

                    AugmentationRecord augmentationRecordNew = ItemRecordHelpers.CloneAugmentationRecord(augmentationRecord, itemId);
                    augmentationRecordProcessorPoq.Init(augmentationRecordNew, itemRarity, mobRarityBoost, itemId);
                    augmentationRecordProcessorPoq.ProcessRecord();
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
            bool canProcess = true;

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

                switch (recordType.Name)
                {
                    case nameof(WeaponRecord):
                        checkWeaponRecord = true;
                        break;
                    case nameof(ArmorRecord):
                    case nameof(HelmetRecord):
                    case nameof(LeggingsRecord):
                    case nameof(BootsRecord):
                        break;
                    case nameof(AugmentationRecord):
                        checkAugmentationRecord = true;
                        //canProcess = false;
                        break;
                    default:
                        _logger.Log($"canProcess = false : {recordType.Name}");
                        canProcess = false;
                        break;
                }

                if (checkWeaponRecord)
                {
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
    }
}
