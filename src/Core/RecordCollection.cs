using MGSC;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using static QM_PathOfQuasimorph.Core.MagnumPoQProjectsController;
using static UnityEngine.EventSystems.EventTrigger;

namespace QM_PathOfQuasimorph.Core
{
    public class RecordCollection
    {
        public static Dictionary<string, CompositeItemRecord> ItemRecords { get; private set; } = new Dictionary<string, CompositeItemRecord>();

        public static Dictionary<string, WoundSlotRecord> WoundSlotRecords { get; private set; } = new Dictionary<string, WoundSlotRecord>();

        public static Dictionary<string, MetadataWrapper> MetadataWrapperRecords { get; private set; } = new Dictionary<string, MetadataWrapper>();
        private static Logger _logger = new Logger(null, typeof(RecordCollection));

        public void Init()
        {
        }
        public static void SerializeCollection()
        {
            _logger.Log($"SerializeCollection");

            PathOfQuasimorph.magnumProjectsController.CreateDataHolderProject();
            var itemRecords = DataSerializerHelper.SerializeData<Dictionary<string, CompositeItemRecord>>(ItemRecords, DataSerializerHelper._jsonSettingsPoq);
            var woundSlotRecords = DataSerializerHelper.SerializeData<Dictionary<string, WoundSlotRecord>>(WoundSlotRecords, DataSerializerHelper._jsonSettingsPoq);
            var metadataWrapperRecords = DataSerializerHelper.SerializeData<Dictionary<string, MetadataWrapper>>(MetadataWrapperRecords, DataSerializerHelper._jsonSettingsPoq);

            PathOfQuasimorph.magnumProjectsController.dataPlaceholderProject.UpcomingModifications.Clear();
            PathOfQuasimorph.magnumProjectsController.dataPlaceholderProject.UpcomingModifications.Add("ItemRecords", itemRecords);
            PathOfQuasimorph.magnumProjectsController.dataPlaceholderProject.UpcomingModifications.Add("WoundSlotRecords", woundSlotRecords);
            PathOfQuasimorph.magnumProjectsController.dataPlaceholderProject.UpcomingModifications.Add("MetadataWrapperRecords", metadataWrapperRecords);

            _logger.Log($"List_ItemRecords: {ItemRecords.Count}");
            _logger.Log($"List_WoundSlotRecords: {WoundSlotRecords.Count}");
            _logger.Log($"List_MetadataWrapperRecords: {MetadataWrapperRecords.Count}");
            //_logger.Log($"itemRecords: {itemRecords}");

            foreach (var id in ItemRecords.Keys.ToList())
            {
                _logger.Log($"logging serialziation: {id}");
            }

            _logger.Log($"SerializeCollection Done");

        }

        public static void DeserializeCollection(string itemRecords, string woundSlotRecords, string magnumProjectWrapperRecords)
        {
            ItemRecords = DataSerializerHelper.DeserializeData<Dictionary<string, CompositeItemRecord>>(itemRecords, DataSerializerHelper._jsonSettingsPoq);
            WoundSlotRecords = DataSerializerHelper.DeserializeData<Dictionary<string, WoundSlotRecord>>(woundSlotRecords, DataSerializerHelper._jsonSettingsPoq);
            MetadataWrapperRecords = DataSerializerHelper.DeserializeData<Dictionary<string, MetadataWrapper>>(magnumProjectWrapperRecords, DataSerializerHelper._jsonSettingsPoq);

            _logger.Log($"List_ItemRecords: {ItemRecords.Count}");
            _logger.Log($"List_WoundSlotRecords: {WoundSlotRecords.Count}");
            _logger.Log($"List_MetadataWrapperRecords: {MetadataWrapperRecords.Count}");

            _logger.Log($"Verify records");

            // This may be time consuming as we need to get item descriptors as we don't serialize them (idk if we need it tho)
            // So we iterate all desciptors collections and find descriptor we need

            foreach (var itemRecord in ItemRecords)
            {
                _logger.Log($"\t itemRecord: {itemRecord}");

                var compositeRecord = itemRecord.Value;
                var baseIdExist = MetadataWrapper.TryGetBaseId(compositeRecord.Id, out string baseId);

                _logger.Log($"\t baseId: {baseId}");
                _logger.Log($"\t baseIdExist: {baseIdExist}");

                if (baseIdExist)
                {
                    _logger.Log($"\t Data.Descriptors.Count: {Data.Descriptors.Count}");

                    // Iterate every record to get proper descriptor
                    foreach (var record in itemRecord.Value.Records)
                    {
                        var foundDescriptor = TryFindDescriptor(baseId);
                        if (foundDescriptor != null)
                        {
                            record.ContentDescriptor = foundDescriptor;
                        }
                    }

                    if (Data.Items._records.ContainsKey(itemRecord.Key))
                    {
                        _logger.Log($"\t Data.Items._records removing key {itemRecord.Key}");

                        Data.Items._records.Remove(itemRecord.Key);
                    }

                    _logger.Log($"\t Data.Items._records adding key {itemRecord.Key} and compositeRecord {compositeRecord}");

                    Data.Items._records.Add(compositeRecord.Id, compositeRecord);

                    // Add item transformations
                    _logger.Log($"Checking ItemTransformationRecord");

                    ItemTransformationRecord itemTransformationRecord = Data.ItemTransformation.GetRecord(baseId);

                    if (itemTransformationRecord == null || itemTransformationRecord.Id == string.Empty)
                    {
                        _logger.Log($"ItemTransformationRecord - missing. Adding record.");

                        // Item breaks into this, unless it has it's own itemTransformationRecord.
                        itemTransformationRecord = Data.ItemTransformation.GetRecord("prison_tshirt_1", true);
                        Data.ItemTransformation.AddRecord(compositeRecord.Id, itemTransformationRecord.Clone(compositeRecord.Id));
                    }
                    else
                    {
                        _logger.Log($"ItemTransformationRecord - exists: result will be item count {itemTransformationRecord.OutputItems.Count}");
                        Data.ItemTransformation.AddRecord(compositeRecord.Id, itemTransformationRecord.Clone(compositeRecord.Id));
                    }

                    if (GetBoostedString(itemRecord.Key) == null)
                    {
                        _logger.Log($"\t {itemRecord.Key} is missing BoostedString");

                        if (MetadataWrapperRecords.TryGetValue(itemRecord.Key, out MetadataWrapper metaData))
                        {
                            _logger.Log($"\t metaData.SerializedStorage {metaData.SerializedStorage}");

                            if (!metaData.SerializedStorage)
                            {
                                // Since we no longer use boostedParam in ticks metadata and relay on MetaData, we need to migrate this as well.
                                var boostedParam = DigitInfo.GetBoostedParam(metaData.FinishTime.Ticks);
                                if (boostedParam != 99)
                                {
                                    _logger.Log($"\t boostedParam: {boostedParam} for {itemRecord.Key}");
                                    metaData.BoostedString = RaritySystem.ParamIdentifiers[boostedParam];
                                }
                            }
                        }
                    }

                    // Add localization
                    Localization.DuplicateKey("item." + baseId + ".name", "item." + compositeRecord.Id + ".name");
                    Localization.DuplicateKey("item." + baseId + ".shortdesc", "item." + compositeRecord.Id + ".shortdesc");
                    RaritySystem.AddAffixes(compositeRecord.Id);

                }
                //Data.Items.AddRecord(itemRecord.Id, itemRecord);
            }

            foreach (var woundSlotRecord in WoundSlotRecords)
            {
                Data.WoundSlots.RemoveRecord(woundSlotRecord.Key);
                Data.WoundSlots.AddRecord(woundSlotRecord.Key, woundSlotRecord.Value);
            }
        }

        private static UnityEngine.Object TryFindDescriptor(string baseId)
        {
            foreach (var desc in Data.Descriptors)
            {
                var value = desc.Value;
                if (value?.Ids == null) continue;

                for (int i = 0; i < value.Ids.Count(); i++)
                {
                    if (value._ids[i] == baseId)
                    {
                        _logger.Log($"\t\t\t MATCH FOUND! ID: {baseId}");

                        return value._descriptors[i];
                    }
                }
            }

            return null;
        }

        public static bool HasRecord(string itemId)
        {
            return ItemRecords.ContainsKey(itemId);
        }

        internal static string GetBoostedString(string itemId)
        {
            if (MetadataWrapperRecords.TryGetValue(itemId, out MetadataWrapper metaData))
            {
                return metaData.BoostedString;
            }
            return string.Empty;
        }

        internal static void CleanObsoleteItemRecords(List<string> idsToKeep)
        {
            _logger.Log($"CleanObsoleteItemRecords");

            foreach (var id in ItemRecords.Keys.ToList())
            {
                if (!idsToKeep.Contains(id))
                {
                    _logger.Log($"removing: {id} from ItemRecords");
                    ItemRecords.Remove(id);
                }
            }

            foreach (var id in MetadataWrapperRecords.Keys.ToList())
            {
                if (!idsToKeep.Contains(id))
                {
                    _logger.Log($"removing: {id} from MetadataWrapperRecords");
                    MetadataWrapperRecords.Remove(id);
                }
            }

            // Also process WoundSlots as they slightly different
            // Example: RecreationCyborgHead_recreationCyborg_head_custom_poq_1337_1580887887000002

            foreach (var id in WoundSlotRecords.Keys.ToList())
            {
                int firstUnderscoreIndex = id.IndexOf('_');
                if (firstUnderscoreIndex == -1)
                {
                    _logger.LogWarning($"not removing: {id} from WoundSlotRecords (no underscore)");
                    //WoundSlotRecords.Remove(id);
                    continue;
                }

                string suffix = id.Substring(firstUnderscoreIndex + 1); // everything after first '_'

                if (!idsToKeep.Contains(suffix))
                {
                    _logger.Log($"removing: {id} from WoundSlotRecords");
                    WoundSlotRecords.Remove(id);
                }
            }

            SerializeCollection();
        }
    }


    //TypeNameHandling.Auto if dictionaries contain derived types
    // string json = System.IO.File.ReadAllText("record_collection.json");

    // RecordCollection loadedRecords = JsonConvert.DeserializeObject<RecordCollection>(json, new JsonSerializerSettings
    // {
    //     TypeNameHandling = TypeNameHandling.Auto
    // });



}
