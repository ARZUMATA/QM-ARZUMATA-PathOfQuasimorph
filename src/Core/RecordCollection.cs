using MGSC;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using static QM_PathOfQuasimorph.Core.MagnumPoQProjectsController;

namespace QM_PathOfQuasimorph.Core
{
    [JsonObject(MemberSerialization.OptIn)]
    public class RecordCollection
    {
        [JsonProperty("items")]
        public static Dictionary<string, BasePickupItemRecord> ItemRecords { get; set; } = new Dictionary<string, BasePickupItemRecord>();

        [JsonProperty("woundSlots")]
        public static Dictionary<string, WoundSlotRecord> WoundSlotRecords { get; set; } = new Dictionary<string, WoundSlotRecord>();

        [JsonProperty("magnumProjects")]
        public static Dictionary<string, MagnumProjectWrapper> MagnumProjectWrapperRecords { get; set; } = new Dictionary<string, MagnumProjectWrapper>();

        public static bool HasRecord(string itemId)
        {
            return ItemRecords.ContainsKey(itemId);

        }

        public static bool HasWrapperRecord(string itemId)
        {
            return MagnumProjectWrapperRecords.ContainsKey(itemId);
        }

        internal static string GetBoostedString(string itemId)
        {
            if (HasWrapperRecord(itemId))
            {
                return MagnumProjectWrapperRecords[itemId].BoostedString;
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
