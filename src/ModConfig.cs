using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using MGSC;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace QM_PathOfQuasimorph
{
    public class ModConfig
    {
        [JsonIgnore]
        public bool DebugLog { get; set; } = false;

        [JsonIgnore]
        public bool Enable { get; set; } = true;

        [JsonIgnore]
        public bool EnableMobs { get; set; } = true;

        [JsonIgnore]
        public bool ApplyRarityToMagnumItems { get; set; } = true;

        [JsonIgnore]
        public Color RarityColor_Standard { get; set; } = Helpers.HexStringToUnityColor("#FFFFFF");

        [JsonIgnore]
        public Color RarityColor_Enhanced { get; set; } = Helpers.HexStringToUnityColor("#8888FF");

        [JsonIgnore]
        public Color RarityColor_Advanced { get; set; } = Helpers.HexStringToUnityColor("#FFFF77");

        [JsonIgnore]
        public Color RarityColor_Premium { get; set; } = Helpers.HexStringToUnityColor("#AF6025");

        [JsonIgnore]
        public Color RarityColor_Prototype { get; set; } = Helpers.HexStringToUnityColor("#800080");

        [JsonIgnore]
        public Color RarityColor_Quantum { get; set; } = Helpers.HexStringToUnityColor("#FF0000");

        [JsonIgnore]
        public Color DifferenceColor_Positive { get; set; } = Helpers.HexStringToUnityColor("#2196F3");

        [JsonIgnore]
        public Color DifferenceColor_Negative { get; set; } = Helpers.HexStringToUnityColor("#F44336");

        [JsonIgnore]
        public Color DifferenceColor_Equal { get; set; } = Helpers.HexStringToUnityColor("#444444");

        [JsonIgnore]
        public Color MonsterMasteryColors_Novice { get; set; } = Helpers.HexStringToUnityColor("#8888FF");

        [JsonIgnore]
        public Color MonsterMasteryColors_Skilled { get; set; } = Helpers.HexStringToUnityColor("#FFFF77");

        [JsonIgnore]
        public Color MonsterMasteryColors_Expert { get; set; } = Helpers.HexStringToUnityColor("#800080");

        [JsonIgnore]
        public Color MonsterMasteryColors_Grandmaster { get; set; } = Helpers.HexStringToUnityColor("#FF0000");





        [JsonIgnore]
        public bool CleanupMode { get; set; } = true;

        [JsonIgnore]
        public bool CustomWeights { get; set; } = true;

        [JsonIgnore]
        public string CustomWeightsInfo1 { get; set; }

        [JsonIgnore]
        public string CustomWeightsInfo2 { get; set; }

        [JsonIgnore]
        public string CustomWeightsInfo3 { get; set; }

        [JsonIgnore]
        public string Date { get; set; }

        [JsonIgnore]
        public string Commit { get; set; }

        [JsonIgnore]
        public string About1 { get; set; }

        [JsonIgnore]
        public string About2 { get; set; }

        [JsonIgnore]
        //[JsonProperty("version")]
        public string Version { get; set; } = "1.9.3";

        // MCM Related Start

        public void LoadConfigMCM(string configPath)
        {
            if (File.Exists(configPath))
            {
                string[] array = File.ReadAllLines(configPath);
                for (int i = 0; i < array.Length; i++)
                {
                    string text = array[i].Trim();
                    if (text.Contains('='))
                    {
                        string[] array2 = text.Split(new char[]
                        {
                            '='
                        }, 2);
                        string name = array2[0].Trim();
                        string value = array2[1].Trim();
                        object value2 = this.ConvertValue(value);

                        Plugin.Logger.Log($"name: {name}, value: {value.ToString()}, value: {value2.ToString()}");


                        PropertyInfo property = base.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
                        if (property != null)
                        {
                            property.SetValue(this, value2, null);
                        }
                        else
                        {
                            throw new Exception("Property not found LoadConfigMCM configPath: " + name);
                        }
                    }
                }
            }
        }

        public void LoadConfigMCM(Dictionary<string, object> propertiesDictionary)
        {
            foreach (KeyValuePair<string, object> keyValuePair in propertiesDictionary)
            {
                PropertyInfo property = base.GetType().GetProperty(keyValuePair.Key, BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
                if (property != null)
                {
                    property.SetValue(this, Convert.ChangeType(keyValuePair.Value, property.PropertyType), null);
                }
                else
                {
                    throw new Exception("Property not found LoadConfigMCM propertiesDictionary: " + keyValuePair.Key);
                }
            }
        }

        private object ConvertValue(string value)
        {
            int num;
            if (int.TryParse(value, out num))
            {
                return num;
            }
            float num2;
            if (float.TryParse(value, out num2))
            {
                return num2;
            }
            bool flag;
            if (bool.TryParse(value, out flag))
            {
                return flag;
            }
            Color color;
            if (ColorUtility.TryParseHtmlString(value.Replace("\"", string.Empty), out color))
            {
                return color;
            }
            return value;
        }

        // MCM Related End

        // We don't use json config so for now we dont need this.
        public static ModConfig LoadConfig(string configPath)
        {
            ModConfig config;

            JsonSerializerSettings serializerSettings = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented,
            };

            Plugin.Logger.Log("ModConfig JSON Loading config from " + configPath);
            Plugin.Logger.Log("File.Exists " + File.Exists(configPath));

            if (File.Exists(configPath))
            {


                Plugin.Logger.Log("JSON Loading config from " + configPath);

                try
                {
                    string sourceJson = File.ReadAllText(configPath);

                    config = JsonConvert.DeserializeObject<ModConfig>(sourceJson, serializerSettings);

                    //Add any new elements that have been added since the last mod version the user had.
                    string upgradeConfig = JsonConvert.SerializeObject(config, serializerSettings);

                    if (upgradeConfig != sourceJson)
                    {
                        Plugin.Logger.Log("Updating config with missing elements");
                        //re-write
                        File.WriteAllText(configPath, upgradeConfig);
                    }

                    return config;
                }
                catch (Exception ex)
                {
                    Plugin.Logger.LogError("Error parsing configuration.  Ignoring config file and using defaults");
                    Plugin.Logger.LogException(ex);

                    //Not overwriting in case the user just made a typo.
                    config = new ModConfig();
                    return config;
                }
            }
            else
            {
                config = new ModConfig();

                string json = JsonConvert.SerializeObject(config, serializerSettings);
                File.WriteAllText(configPath, json);

                return config;
            }
        }
    }
}