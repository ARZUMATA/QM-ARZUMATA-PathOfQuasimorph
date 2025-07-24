using MGSC;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using QM_PathOfQuasimorph.Core.Processors;
using QM_PathOfQuasimorph.PoQHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static QM_PathOfQuasimorph.Core.MagnumPoQProjectsController;
using static UnityEngine.Rendering.Universal.TemporalAA;

namespace QM_PathOfQuasimorph.Core
{
    internal class ItemRecordsControllerPoq
    {
        private readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            NullValueHandling = NullValueHandling.Include,
            Formatting = Formatting.Indented,
            ObjectCreationHandling = ObjectCreationHandling.Replace,
            TypeNameHandling = TypeNameHandling.Objects,
            MissingMemberHandling = MissingMemberHandling.Error,
            ContractResolver = new DataSerializerHelper.CompositeItemRecordResolver(),
            MaxDepth = 10,
        };

        internal AugmentationRecordProcessorPoq augmentationRecordProcessorPoq;
        internal ImplantRecordProcessorPoq implantRecordProcessorPoq;
        internal WeaponRecordProcessorPoq weaponRecordProcessorPoq;
        internal HelmetRecordProcessorPoq helmetRecordProcessorPoq;
        internal ArmorRecordProcessorPoq armorRecordProcessorPoq;
        internal LeggingsRecordProcessorPoq leggingsRecordProcessorPoq;
        internal BootsRecordProcessorPoq bootsRecordProcessorPoq;
        public ItemProduceReceipt itemProduceReceiptPlaceHolder = null;

        private Logger _logger = new Logger(null, typeof(ItemRecordsControllerPoq));

        Dictionary<string, string> itemRecords = new Dictionary<string, string>();

        internal ItemRecordsControllerPoq()
        {
            augmentationRecordProcessorPoq = new AugmentationRecordProcessorPoq(this);
            implantRecordProcessorPoq = new ImplantRecordProcessorPoq(this);
            weaponRecordProcessorPoq = new WeaponRecordProcessorPoq(this);
            helmetRecordProcessorPoq = new HelmetRecordProcessorPoq(this);
            armorRecordProcessorPoq = new ArmorRecordProcessorPoq(this);
            leggingsRecordProcessorPoq = new LeggingsRecordProcessorPoq(this);
            bootsRecordProcessorPoq = new BootsRecordProcessorPoq(this);
        }

        public void AddItemRecords()
        {
            PathOfQuasimorph.magnumProjectsController.AddItemRecords(_jsonSettings);
        }

        internal string CreateNew(string itemId, bool mobRarityBoost)
        {
            var itemRarity = PathOfQuasimorph.raritySystem.SelectRarity();

            if (itemRarity == ItemRarity.Standard)
            {
                //return itemId;
            }

            //debug
            itemRarity = ItemRarity.Standard;

            if (!PathOfQuasimorph.itemRecordsControllerPoq.CanProcessItemRecord(itemId))
            {
                return itemId;
            }

            CompositeItemRecord obj = Data.Items.GetRecord(itemId) as CompositeItemRecord;
            var recordsList = new List<BasePickupItemRecord>();

            var boostedParamIndex = ApplyRarityStats(recordsList, obj.Records, itemRarity, mobRarityBoost, itemId);

            // Generate a new UID
            var randomUid = Helpers.UniqueIDGenerator.GenerateRandomIDWith16Characters();
            DigitInfo digits = DigitInfo.GetDigits(randomUid);
            digits.FillZeroes();
            digits.Rarity = (int)itemRarity;
            digits.BoostedParam = boostedParamIndex;
            var randomUidInjected = digits.ReturnUID();

            // Resulting Uid
            var wrapper = new MagnumProjectWrapper(
                 id: itemId,
                 poqItem: true,
                 startTime: new DateTime(MagnumPoQProjectsController.MAGNUM_PROJECT_START_TIME),
                 finishTime: new DateTime(Int64.Parse(randomUidInjected))
                 );

            string oldId = itemId;
            itemId = wrapper.ReturnItemUid();

            Localization.DuplicateKey("item." + oldId + ".name", "item." + itemId + ".name");
            Localization.DuplicateKey("item." + oldId + ".shortdesc", "item." + itemId + ".shortdesc");

            ItemTransformationRecord record = Data.ItemTransformation.GetRecord(oldId);

            if (record == null || record.Id == string.Empty)
            {
                // Item breaks into this, unless it has it's own record.
                record = Data.ItemTransformation.GetRecord("prison_tshirt_1", true);
            }

            if (record != null)
            {
                Data.ItemTransformation.AddRecord(itemId, record.Clone(itemId));
            }

            foreach (var recordEntry in recordsList)
            {
                // Replace Id since we have new one now
                //recordEntry.Id = itemId;
                _logger.Log($"record test {recordEntry.Id}");
                Data.Items.AddRecord(itemId, recordEntry);
            }

            foreach (var recordEntry in obj.Records)
            {
                _logger.Log($"old record test {recordEntry.Id}");
            }

            Data.Items._records.Add(itemId, obj);

            _logger.Log($"itemId {Data.Items._records.Keys.Contains(itemId)}");
            _logger.Log($"oldId {Data.Items._records.Keys.Contains(oldId)}");

            RaritySystem.AddAffixes(itemId);

            // Also add records entry in our dummy magnum project placeholder
            PathOfQuasimorph.magnumProjectsController.AddItemRecord(itemId, obj, _jsonSettings);

            return itemId;
        }

        public string GetItemBoostedStat(string itemId, int statId)
        {
            CompositeItemRecord compositeItemRecord = Data.Items.GetRecord(itemId, true) as CompositeItemRecord;
            Type recordType = compositeItemRecord.PrimaryRecord.GetType();

            switch (recordType.Name)
            {
                case nameof(WeaponRecord):
                    return weaponRecordProcessorPoq.parameters.ElementAt(statId);
                case nameof(ArmorRecord):
                    return armorRecordProcessorPoq.parameters.ElementAt(statId);
                case nameof(HelmetRecord):
                    return helmetRecordProcessorPoq.parameters.ElementAt(statId);
                case nameof(LeggingsRecord):
                    return leggingsRecordProcessorPoq.parameters.ElementAt(statId);
                case nameof(BootsRecord):
                    return bootsRecordProcessorPoq.parameters.ElementAt(statId);
                default:
                    return string.Empty;
                    break;
            }
        }

        private int ApplyRarityStats(List<BasePickupItemRecord> recordsList, List<BasePickupItemRecord> records, ItemRarity itemRarity, bool mobRarityBoost, string itemId)
        {
            _logger.Log($"ApplyRarityStats");
            int boostedStat = 99;

            // Iterate over item records, apply parameters, return ready to go item record.

            _logger.Log($"Iterating records list with count of {records.Count}");

            /* itemId stays as original here as we need to determine new id later on */

            foreach (BasePickupItemRecord basePickupItemRecord in records)
            {
                _logger.Log($"record: {basePickupItemRecord.Id}");

                WeaponRecord weaponRecord = basePickupItemRecord as WeaponRecord;

                if (weaponRecord != null)
                {
                    _logger.Log($"weaponRecord processing");

                    WeaponRecord weaponRecordNew = weaponRecord.Clone($"*{itemId}");
                    weaponRecordProcessorPoq.Init(weaponRecordNew, itemRarity, mobRarityBoost);
                    boostedStat = weaponRecordProcessorPoq.ProcessRecord();
                    recordsList.Add(weaponRecordNew);
                }

                HelmetRecord helmetRecord = basePickupItemRecord as HelmetRecord;

                if (helmetRecord != null)
                {
                    _logger.Log($"helmetRecord processing");

                    HelmetRecord helmetRecordNew = helmetRecord.Clone(itemId);
                    helmetRecordProcessorPoq.Init(helmetRecordNew, itemRarity, mobRarityBoost);
                    boostedStat = helmetRecordProcessorPoq.ProcessRecord();
                    recordsList.Add(helmetRecordNew);
                }

                ArmorRecord armorRecord = basePickupItemRecord as ArmorRecord;

                if (armorRecord != null)
                {
                    _logger.Log($"armorRecord processing");

                    ArmorRecord armorRecordNew = armorRecord.Clone(itemId);
                    armorRecordProcessorPoq.Init(armorRecordNew, itemRarity, mobRarityBoost);
                    boostedStat = armorRecordProcessorPoq.ProcessRecord();
                    recordsList.Add(armorRecordNew);
                }

                LeggingsRecord leggingsRecord = basePickupItemRecord as LeggingsRecord;

                if (leggingsRecord != null)
                {
                    _logger.Log($"leggingsRecord processing");

                    LeggingsRecord leggingsRecordNew = leggingsRecord.Clone(itemId);
                    leggingsRecordProcessorPoq.Init(leggingsRecordNew, itemRarity, mobRarityBoost);
                    boostedStat = leggingsRecordProcessorPoq.ProcessRecord();
                    recordsList.Add(leggingsRecordNew);

                }

                BootsRecord bootsRecord = basePickupItemRecord as BootsRecord;

                if (bootsRecord != null)
                {
                    _logger.Log($"bootsRecord processing");

                    BootsRecord bootsRecordNew = bootsRecord.Clone(itemId);
                    bootsRecordProcessorPoq.Init(bootsRecordNew, itemRarity, mobRarityBoost);
                    boostedStat = bootsRecordProcessorPoq.ProcessRecord();
                    recordsList.Add(bootsRecordNew);
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
                        implantRecordProcessorPoq.Init(implantRecordNew, itemRarity, mobRarityBoost);
                        boostedStat = bootsRecordProcessorPoq.ProcessRecord();
                        recordsList.Add(implantRecordNew);
                    }
                }

                AugmentationRecord augmentationRecord = basePickupItemRecord as AugmentationRecord;

                if (augmentationRecord != null)
                {
                    _logger.Log($"augmentationRecord processing");

                    AugmentationRecord augmentationRecordNew = ItemRecordHelpers.CloneAugmentationRecord(augmentationRecord, itemId);
                    augmentationRecordProcessorPoq.Init(augmentationRecordNew, itemRarity, mobRarityBoost);
                    boostedStat = bootsRecordProcessorPoq.ProcessRecord();
                    recordsList.Add(augmentationRecordNew);
                }
            }

            return boostedStat;
        }


        internal bool HasRecord(string oldItemId)
        {
            return itemRecords.ContainsKey(oldItemId);
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
                            canProcess = false;
                            break;
                        }

                        foreach (var mod in weaponRecord.Categories)
                        {
                            if (blacklistedCategories.Contains(mod))
                            {
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

                        _logger.Log($"InventorySortOrder {augmentationRecord.InventorySortOrder}");
                        _logger.Log($"ContentDescriptor {augmentationRecord.ContentDescriptor.name}");
                        _logger.Log($"ContentDescriptor {augmentationRecord.ContentDescriptor.hideFlags}");
                        _logger.Log($"ItemClass {augmentationRecord.ItemClass}");
                        _logger.Log($"ItemDesc {augmentationRecord.ItemDesc.name}");
                        _logger.Log($"ItemDesc {augmentationRecord.ItemDesc.Icon}");
                        _logger.Log($"ItemDesc {augmentationRecord.ItemDesc.SmallIcon}");
                        _logger.Log($"ItemDesc {augmentationRecord.ItemDesc}");
                        _logger.Log($"ItemDesc {augmentationRecord.ItemDesc}");





                    }
                }
            }

            return canProcess;
        }
    }
}
