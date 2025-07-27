using MGSC;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using static QM_PathOfQuasimorph.Core.MagnumPoQProjectsController;

namespace QM_PathOfQuasimorph.Core
{
    public class RecordCollection
    {
        public static ConfigRecordCollection<CompositeItemRecord> ItemRecords { get; private set; } = new ConfigRecordCollection<CompositeItemRecord>();

        public static ConfigRecordCollection<WoundSlotRecord> WoundSlotRecords { get; private set; } = new ConfigRecordCollection<WoundSlotRecord>();

        public static ConfigRecordCollection<MetadataWrapper> MetadataWrapperRecords { get; private set; } = new ConfigRecordCollection<MetadataWrapper>();

        public void Init()
        {
        }
        public static void SerializeCollection()
        {
            Plugin.Logger.Log($"SerializeCollection");

            PathOfQuasimorph.magnumProjectsController.CreateDataHolderProject();
            var itemRecords = DataSerializerHelper.SerializeData<ConfigRecordCollection<CompositeItemRecord>>(ItemRecords, DataSerializerHelper._jsonSettingsPoq);
            var woundSlotRecords = DataSerializerHelper.SerializeData<ConfigRecordCollection<WoundSlotRecord>>(WoundSlotRecords, DataSerializerHelper._jsonSettingsPoq);
            var metadataWrapperRecords = DataSerializerHelper.SerializeData<ConfigRecordCollection<MetadataWrapper>>(MetadataWrapperRecords, DataSerializerHelper._jsonSettingsPoq);

            PathOfQuasimorph.magnumProjectsController.dataPlaceholderProject.UpcomingModifications.Clear();
            PathOfQuasimorph.magnumProjectsController.dataPlaceholderProject.UpcomingModifications.Add("ItemRecords", itemRecords);
            PathOfQuasimorph.magnumProjectsController.dataPlaceholderProject.UpcomingModifications.Add("WoundSlotRecords", woundSlotRecords);
            PathOfQuasimorph.magnumProjectsController.dataPlaceholderProject.UpcomingModifications.Add("MetadataWrapperRecords", metadataWrapperRecords);

            Plugin.Logger.Log($"itemRecords: {itemRecords}");

        }

        public static void DeserializeCollection(string itemRecords, string woundSlotRecords, string magnumProjectWrapperRecords)
        {
            ItemRecords = DataSerializerHelper.DeserializeData<ConfigRecordCollection<CompositeItemRecord>>(itemRecords, DataSerializerHelper._jsonSettingsPoq);
            WoundSlotRecords = DataSerializerHelper.DeserializeData<ConfigRecordCollection<WoundSlotRecord>>(woundSlotRecords, DataSerializerHelper._jsonSettingsPoq);
            MetadataWrapperRecords = DataSerializerHelper.DeserializeData<ConfigRecordCollection<MetadataWrapper>>(magnumProjectWrapperRecords, DataSerializerHelper._jsonSettingsPoq);

            Plugin.Logger.Log($"List_ItemRecords: {ItemRecords.Count}");
            Plugin.Logger.Log($"List_WoundSlotRecords: {WoundSlotRecords.Count}");
            Plugin.Logger.Log($"List_MetadataWrapperRecords: {MetadataWrapperRecords.Count}");

            foreach (var itemRecord in ItemRecords.Records)
            {
                Data.Items.AddRecord(itemRecord.Id, itemRecord);
            }

            foreach (var woundSlotRecord in WoundSlotRecords.Records)
            {
                Data.WoundSlots.AddRecord(woundSlotRecord.Id, woundSlotRecord);
            }

        }

        public static bool HasRecord(string itemId)
        {
            return ItemRecords.Ids.Contains(itemId);
        }

        internal static string GetBoostedString(string itemId)
        {
            var metaData = MetadataWrapperRecords.GetRecord(itemId);

            if (metaData != null)
            {
                return metaData.BoostedString;
            }

            return string.Empty;
        }

    }


    //TypeNameHandling.Auto if dictionaries contain derived types
    // string json = System.IO.File.ReadAllText("record_collection.json");

    // RecordCollection loadedRecords = JsonConvert.DeserializeObject<RecordCollection>(json, new JsonSerializerSettings
    // {
    //     TypeNameHandling = TypeNameHandling.Auto
    // });



}
