using ModConfigMenu;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using QM_PathOfQuasimorph.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

public static class DataSerializerHelper
{
    /// <summary>
    /// Performs a deep copy of any serializable object using JSON serialization.
    /// Handles: Lists, Dictionaries, Tuples, nested objects, etc.
    /// </summary>
    public static T MakeDeepCopy<T>(T value)
    {
        if (value == null) return default(T);

        var settings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            TypeNameHandling = TypeNameHandling.Auto,
            NullValueHandling = NullValueHandling.Include,
            Formatting = Formatting.Indented,
        };

        string json = JsonConvert.SerializeObject(value, settings);
        Console.WriteLine(json);

        return JsonConvert.DeserializeObject<T>(json, settings);
    }

    public static readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings();

    public static readonly JsonSerializerSettings _jsonSettingsPoq = new JsonSerializerSettings
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


    public static string SerializeData<T>(T _data) where T : class
    {
        return SerializeData<T>(_data, _jsonSettings);
    }

    public static string SerializeData<T>(T _data, JsonSerializerSettings _jsonSettings) where T : class
    {
        var _dataString = JsonConvert.SerializeObject(_data, _jsonSettings);
        return _dataString;
    }

    public static string SerializeDataBase64<T>(T _data) where T : class
    {
        return SerializeDataBase64<T>(_data, _jsonSettings);
    }

    public static string SerializeDataBase64<T>(T _data, JsonSerializerSettings _jsonSettings) where T : class
    {
        var _dataString = Convert.ToBase64String(
                Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(_data, _jsonSettings)));
        return _dataString;
    }

    public static T DeserializeData<T>(string _dataString) where T : class
    {
        return DeserializeData<T>(_dataString, _jsonSettings);

    }

    public static T DeserializeData<T>(string _dataString, JsonSerializerSettings _jsonSettings) where T : class
    {
        var deserializedData = JsonConvert.DeserializeObject<T>(_dataString, _jsonSettings);
        return deserializedData;
    }

    public static T DeserializeDataBase64<T>(string _dataString) where T : class
    {
        return DeserializeDataBase64<T>(_dataString, _jsonSettings);

    }
    public static T DeserializeDataBase64<T>(string _dataString, JsonSerializerSettings _jsonSettings) where T : class
    {
        try
        {
            var base64 = _dataString.Trim();
            if (base64.Length % 4 != 0)
            {
                return null;
            }

            try
            {
                var jsonBytes = Convert.FromBase64String(base64);
                var deserializedData = DeserializeData<T>(Encoding.UTF8.GetString(jsonBytes), _jsonSettings);
                //var deserializedData = JsonConvert.DeserializeObject<T>(
                    //Encoding.UTF8.GetString(jsonBytes), _jsonSettings);
                return deserializedData;
            }
            catch (FormatException)
            {
                // Invalid Base64 string
                return null;
            }
        }
        catch (Exception)
        {
            // Any other decoding or deserialization error
            return null;
        }
    }

    internal class CompositeItemRecordResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);

            // Enable writing to properties with protected/private setters
            if (property.Writable == false)
            {
                var propertyInfo = member as PropertyInfo;
                if (propertyInfo != null)
                {
                    // Check if there's a non-public setter
                    if (propertyInfo.GetSetMethod(nonPublic: true) != null)
                    {
                        property.Writable = true;
                    }
                }
            }

            // Explicitly include the Records property
            if (property.PropertyName == "Records")
            {
                property.ShouldSerialize = _ => true;
            }

            // Explicitly include ResistSheet
            else if (property.PropertyName == "ResistSheet")
            {
                property.ShouldSerialize = _ => true;
                property.Ignored = false;
                property.Readable = true;
                property.Writable = true; // Ensure it can be set
            }
            // Explicitly exclude problematic properties
            else if (property.PropertyName == "ItemDesc" ||
                     property.PropertyName == "PrimaryRecord")
            {
                property.ShouldSerialize = _ => false;
            }

            // Optionally exclude all Unity types like GameObject
            if (property.PropertyType.Namespace == "UnityEngine")
            {
                property.ShouldSerialize = _ => false;
            }

            return property;
        }


    }
}
