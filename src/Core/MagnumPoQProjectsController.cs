using MGSC;
using QM_PathOfQuasimorph.Core;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Security.AccessControl;
using System.Security.Cryptography;
using UnityEngine;
using static QM_PathOfQuasimorph.Core.PathOfQuasimorph;
using static QM_PathOfQuasimorph.MagnumPoQProjectsController;

namespace QM_PathOfQuasimorph
{
    internal class MagnumPoQProjectsController
    {
        public MagnumProjects magnumProjects;

        public MagnumPoQProjectsController(MagnumProjects magnumProjects)
        {
            this.magnumProjects = magnumProjects;
        }

        public class MagnumProjectWrapper
        {
            public string id { get; set; }
            public string customId { get; set; }
            public string rarity { get; set; }
            public RarityClass rarityClass { get; set; }
            public long uid { get; set; }
            public DateTime finishTime { get; set; }
            public string fullstring { get; set; }
        }

        public string CreateMagnumProjectWithMods(MagnumProjectType projectType, string projectId)
        {
            MagnumProject newProject = new MagnumProject(projectType, projectId);
            newProject.AppliedModifications.Add("rangeweapon_damage", "999");
            newProject.AppliedModifications.Add("rangeweapon_crit_damage", "999");
            newProject.AppliedModifications.Add("rangeweapon_max_durability", "999");
            newProject.AppliedModifications.Add("rangeweapon_weight", "0.1");

            // Generate a new UID
            var uid = Helpers.UniqueIDGenerator.GenerateRandomID();
            string uidStr;
            int d1, d2;

            GetDigits(uid, out uidStr, out d1, out d2);

            d1 = 0;
            d2 = (int)RarityClass.Quantum;

            // Rebuild the last two digits as a string
            string modifiedLastTwo = $"{d1}{d2}";

            // Replace in the original UID string
            string modifiedUidStr = uidStr.Substring(0, uidStr.Length - 2) + modifiedLastTwo;

            // New finish project time
            var finishTimeTemp = DateTime.FromBinary(long.Parse(modifiedUidStr)); // Convert uint64 to DateTime and this is our unique ID for item

            newProject.StartTime = DateTime.MinValue;
            newProject.FinishTime = finishTimeTemp;

            magnumProjects.Values.Add(newProject);

            var rarity = RarityClass.Quantum.ToString().ToLower(); ; // TODO
            var newId = $"{projectId}_custom_poq_{rarity}_{uid}";
            //MagnumDevelopmentSystem.InjectItemRecord(newProject);
            PathOfQuasimorph.InjectItemRecord(newProject, newId);
            Plugin.Logger.Log($"\t\t Created new project for {newProject.DevelopId}");

            return newId;
        }

        private static void GetDigits(long uid, out string uidStr, out int d1, out int d2)
        {
            // Convert to string
            uidStr = uid.ToString();

            // Use last two digits of as identifier
            // [0] = nothing
            // [1] = rarity

            // Extract last two digits as individual characters
            char digitOne = uidStr[uidStr.Length - 2]; // just for now
            char digitTwo = uidStr[uidStr.Length - 1];

            // Convert to integers
            d1 = int.Parse(digitOne.ToString());
            d2 = int.Parse(digitTwo.ToString());
        }

        internal MagnumProject GetProjectById(string itemId)
        {
            if (itemId.Contains("_poq_"))
            {
                var wrapped = SplitItemId(itemId);

                foreach (MagnumProject magnumProject in magnumProjects.Values)
                {
                    if (magnumProject.FinishTime == wrapped.finishTime)
                    {
                        return magnumProject;
                    }
                }
            }
            else
            {
                foreach (MagnumProject magnumProject in magnumProjects.Values)
                {
                    if (magnumProject.DevelopId == itemId)
                    {
                        return magnumProject;
                    }
                }
            }

            return null; // Return null if no project is found
        }

        public enum RarityClass
        {
            Standard,
            Enchanced,
            Advanced,
            Premium,
            Prototype,
            Quantum
        }

        public string WrapProjectDateTime(MagnumProject newProject)
        {
            var finishTimeTemp = newProject.FinishTime;
            long finishTimeAsLong = finishTimeTemp.Ticks;
            string uidStr;
            int d1, d2;
            GetDigits(finishTimeAsLong, out uidStr, out d1, out d2);

            var rarityClass = ((RarityClass)d2).ToString().ToLower();

            var newId = $"{newProject.DevelopId}_custom_poq_{rarityClass}_{uidStr}";
            return newId;
        }

        public MagnumProjectWrapper SplitItemId(string itemId)
        {
            if (itemId.Contains("_poq_"))
            {
                var newResult = itemId.Split(new string[] { "_poq_" }, StringSplitOptions.None);

                var realBaseId = newResult[0].Replace("_custom", string.Empty); // Real Base item ID
                var customId = realBaseId + "_custom"; // Custom ID

                var suffixParts = newResult[1].Split(new string[] { "_" }, 2, StringSplitOptions.None);
                string rarityName = suffixParts[0]; // e.g., "Prototype"
                long hash = Int64.Parse(suffixParts.Length > 1 ? suffixParts[1] : "0"); // "1234567890"

                RarityClass rarityClass = (RarityClass)Enum.Parse(typeof(RarityClass), rarityName);

                return new MagnumProjectWrapper
                {
                    id = realBaseId,
                    customId = customId,
                    rarity = rarityName,
                    rarityClass = rarityClass,
                    finishTime = DateTime.FromBinary((long)hash),
                    uid = hash,
                    fullstring = itemId
                };
            }

            var realBaseId2 = itemId.Replace("_custom", string.Empty); // Real Base item ID
            var customId2 = realBaseId2 + "_custom"; // Custom ID

            return new MagnumProjectWrapper
            {
                id = realBaseId2,
                customId = customId2,
                rarity = "Standard",
                rarityClass = RarityClass.Standard,
                uid = 0,
                fullstring = itemId,
            };
        }

        // itemId: pmc_shotgun_1
        public void CreateRangeWeaponProjectStraightAway(string itemdId)
        {
            /*
             * See:
             * MagnumCreateProjectWindow
             * private void StartDevelopmentButtonOnClick
             */
            var projectType = MagnumProjectType.RangeWeapon;
            var projectCustomId = "_custom_mod_1";

            var project = new MagnumProject(projectType, projectCustomId)
            {
                //project.InitRecord(); // constructor already calls InitRecord

                StartTime = DateTime.MinValue,
                FinishTime = DateTime.MinValue.AddDays(1),
                IsInDevelopment = false
            };

            MagnumDevelopmentSystem.InjectProjectRecord(project);

            //project.CustomRecord.TrimId(); // Maybe it works for initializing custom record manually.

            //var damageInfo = ((WeaponRecord)project._customRecord).Damage;
            //damageInfo.damage = "0";

            // = ((WeaponRecord)project.DefaultRecord).Damage * 2;

            //WeaponRecord weaponRecord = (WeaponRecord)project.con;




            Dictionary<string, string> dictionary = new Dictionary<string, string>();



            // Add range weapon parameters with default values if not already in the dictionary
            string[] rangeWeaponParams = {
                "rangeweapon_damage",
                "rangeweapon_crit_damage",
                "rangeweapon_max_durability",
                "rangeweapon_accuracy",
                "rangeweapon_scatter_angle",
                "rangeweapon_weight",
                "rangeweapon_reload_duration",
                "rangeweapon_magazine_capacity",
                "rangeweapon_special_ability"
            };

            Dictionary<string, string> defaultWeaponParameters = new Dictionary<string, string>
                {
                    {"Id", "default_id"},
                    {"Categories", "default_categories"},
                    {"TechLevel", "default_techlevel"},
                    {"Price", "0"},
                    {"Weight", "0"},
                    {"InventoryWidthSize", "0"},
                    {"ItemClass", "default_itemclass"},
                    {"WeaponClass", "default_weaponclass"},
                    {"WeaponSubClass", "default_weaponsubclass"},
                    {"IsImplicit", "False"},
                    {"RequiredAmmo", "default_requiredammo"},
                    {"OverrideAmmo", "False"},
                    {"DefaultAmmoId", "default_ammo_id"},
                    {"DefaultGrenadeId", "default_grenade_id"},
                    {"Damage", "0"},
                    {"BonusScatterAngle", "0"},
                    {"Firemodes", "default_firemodes"},
                    {"BonusAccuracy", "0"},
                    {"Range", "0"},
                    {"Falloff", "0"},
                    {"MagazineCapacity", "0"},
                    {"MinRandomAmmoCount", "0"},
                    {"ReloadDuration", "0"},
                    {"MaxDurability", "0"},
                    {"MinDurabilityAfterRepair", "0"},
                    {"Unbreakable", "False"},
                    {"RepairItemIds", "default_repair_ids"},
                    {"AllowedGrenadeIds", "default_allowed_grenade_ids"},
                    {"Traits", "default_traits"},
                    {"OverrideProjectileId", "default_projectile_id"}
                };


            foreach (string param in rangeWeaponParams)
            {
                if (!dictionary.ContainsKey(param))
                {
                    //dictionary[param] = Data.Items.GetSimpleRecord<WeaponRecord>(itemdId).Damage

                    //GetDefaultValue(param); // Assumes a method GetDefaultValue exists
                }
            }



            //case MagnumProjectParameterType.CritDamage:
            //return ((DmgInfo)this.GetPropertyValue(record, projectParameter.ParameterName)).critDmg;

            // config_magnum.txt
            // | Id                           | ParameterName      | ParameterType      | ProjectType | ViewType | MinValue | MaxValue | Step | TooltipTag             |
            // |------------------------------|--------------------|--------------------|-------------|----------|----------|----------|------|------------------------|
            // | rangeweapon_damage           | Damage             | Damage             | RangeWeapon | Damage   | 1        | 999      | 2    | tooltip.Damage         |
            // | rangeweapon_crit_damage      | CritDamage         | Damage             | RangeWeapon | Percent  | 1        | 999      | 0.1  | tooltip.CritDamage     |
            // | rangeweapon_max_durability   | MaxDurability      | Integer            | RangeWeapon | Integer  | 1        | 999      | 10   | tooltip.Condition      |
            // | rangeweapon_accuracy         | BonusAccuracy      | WeaponAccuracy     | RangeWeapon | Percent  | -1       | 1        | 0.03 | tooltip.RangeAccuracy  |
            // | rangeweapon_scatter_angle    | BonusScatterAngle  | WeaponScatterAngle |RangeWeapon  | Angle    | 0        | 180      | -0.2 | tooltip.ScatterAngle   |
            // | rangeweapon_weight           | Weight             | Float              | RangeWeapon | Float0_0 | 0.1      | 999      | -0.2 | tooltip.ItemWeight     |
            // | rangeweapon_reload_duration  | ReloadDuration     | Integer            | RangeWeapon | Integer  | 1        | 999      | -1   | tooltip.ReloadDuration |
            // | rangeweapon_magazine_capacity| MagazineCapacity   | Integer            | RangeWeapon | Integer  | 1        | 999      | 3    | tooltip.AmmoCapacity   |
            // | rangeweapon_special_ability  | SpecialAbility     | RangeWeaponTrait   |RangeWeapon  | None     |          |          |      |                        |







        }

    }

}