using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QM_PathOfQuasimorph.Core
{
    internal partial class MagnumPoQProjectsController
    {



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
                StartTime = DateTime.MinValue,
                FinishTime = DateTime.MinValue.AddDays(1),
                IsInDevelopment = false,
            };

            MagnumDevelopmentSystem.InjectProjectRecord(project);

            //project.CustomRecord.TrimId(); // Maybe it works for initializing custom record manually.

            //var damageInfo = ((WeaponRecord)project._customRecord).Damage;
            //damageInfo.damage = "0";

            // = ((WeaponRecord)project.DefaultRecord).Damage * 2;

            //WeaponRecord weaponRecord = (WeaponRecord)project.con;

            Dictionary<string, string> dictionary = new Dictionary<string, string>();

            // Add range weapon parameters with default values if not already in the dictionary
            string[] rangeWeaponParams =
            {
                "rangeweapon_damage",
                "rangeweapon_crit_damage",
                "rangeweapon_max_durability",
                "rangeweapon_accuracy",
                "rangeweapon_scatter_angle",
                "rangeweapon_weight",
                "rangeweapon_reload_duration",
                "rangeweapon_magazine_capacity",
                "rangeweapon_special_ability",
            };

            Dictionary<string, string> defaultWeaponParameters = new Dictionary<string, string>
            {
                { "Id", "default_id" },
                { "Categories", "default_categories" },
                { "TechLevel", "default_techlevel" },
                { "Price", "0" },
                { "Weight", "0" },
                { "InventoryWidthSize", "0" },
                { "ItemClass", "default_itemclass" },
                { "WeaponClass", "default_weaponclass" },
                { "WeaponSubClass", "default_weaponsubclass" },
                { "IsImplicit", "False" },
                { "RequiredAmmo", "default_requiredammo" },
                { "OverrideAmmo", "False" },
                { "DefaultAmmoId", "default_ammo_id" },
                { "DefaultGrenadeId", "default_grenade_id" },
                { "Damage", "0" },
                { "BonusScatterAngle", "0" },
                { "Firemodes", "default_firemodes" },
                { "BonusAccuracy", "0" },
                { "Range", "0" },
                { "Falloff", "0" },
                { "MagazineCapacity", "0" },
                { "MinRandomAmmoCount", "0" },
                { "ReloadDuration", "0" },
                { "MaxDurability", "0" },
                { "MinDurabilityAfterRepair", "0" },
                { "Unbreakable", "False" },
                { "RepairItemIds", "default_repair_ids" },
                { "AllowedGrenadeIds", "default_allowed_grenade_ids" },
                { "Traits", "default_traits" },
                { "OverrideProjectileId", "default_projectile_id" },
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
