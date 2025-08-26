using MGSC;
using QM_PathOfQuasimorph.Controllers;
using QM_PathOfQuasimorph.Core;
using QM_PathOfQuasimorph.Records;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace QM_PathOfQuasimorph.Processors
{
    internal class AmmoRecordProcessor<T> : ItemRecordProcessor<T> where T : AmmoRecord
    {
        //private new Logger _logger = new Logger(null, typeof(AmmoRecordProcessor));
        public override Dictionary<string, bool> parameters => _parameters;

        public AmmoRecordProcessor(ItemRecordsControllerPoq controller) : base(controller) 
        {
            //_parameters["BallisticType"] = true;
            _parameters["MinAmmoAmount"] = true;
            _parameters["MaxAmmoAmount"] = true;
            //_parameters["AmmoType"] = true;
            //_parameters["DmgType"] = true;
            _parameters["DmgCritChance"] = true;
            _parameters["RangeBonus"] = true;
            _parameters["AccuracyMult"] = true;
            _parameters["ScatterMult"] = false;
            _parameters["DamageMult"] = true;
            _parameters["BulletCastsPerShot"] = true;
            //_parameters["StatusEffectId"] = true;
            _parameters["StatusDamageModifier"] = true;
            _parameters["StatusResistModifier"] = true;
            //_parameters["Traits"] = true;
            //_parameters["ProjectileId"] = true;
        }


        List<string> AmmoTypes = new List<string>
        {
            "BatteryCells",
            "Bolts",
            "Bullets",
            "Gas",
            "Heavy",
            "Medium",
            "QuasiCells",
            "Rocket",
            "SawBlade",
            "Shells",
            "SuperHeavy",
            "Toxic",
        };

        List<string> DmgTypes = new List<string>
        {
            "pierce",
            "blunt",
            "explosion",
            "lacer",
            "cold",
            "plasma",
            "fire",
            "poison",
            "beam",
            "shock",
            "chaos",
        };

        List<HashSet<string>> traitsMutuallyExclusiveGroups = new List<HashSet<string>>
        {
            new HashSet<string> { "fires", "heavy_fires" },
            new HashSet<string> { "toxic", "heavy_toxic" },
            new HashSet<string> { "knockback", "heavy_knockback" },
            new HashSet<string> { "knockdown", "heavy_knockdown" },
            new HashSet<string> { "incendiary", "heavy_incendiary" },
            new HashSet<string> { "explosive", "explosive_fire", "explosive_flak", "explosive_hfg", "explosive_light_flak", "explosive_poison", "explosive_quasi", "explosive_shotgun" },
        };

        protected override List<string> GetTraitsList() => itemRecord.Traits;
        protected override T GetGenericRecord() => Data.Items.GetSimpleRecord<T>(oldId, true);
        protected override ItemTraitType? GetTraitType() => ItemTraitType.AmmoTrait;
        protected override List<HashSet<string>> GetMutuallyExclusiveGroups() => traitsMutuallyExclusiveGroups;


        internal override void ProcessRecord(ref string boostedParamString)
        {
            if (itemRarity == ItemRarity.Standard)
            {
                return;
            }

            ApplyParameters(ref boostedParamString);
            ApplyTraits(true);
        }

        private void ApplyParameters(ref string boostedParamString)
        {
            float baseModifier, finalModifier;
            int numToHinder, numToImprove, improvedCount, hinderedCount;
            //string boostedParamString;
            bool increase;
            PrepGenericData(out baseModifier, out finalModifier, out numToHinder, out numToImprove, out boostedParamString, out improvedCount, out hinderedCount, out increase);

            foreach (var stat in parameters)
            {
                finalModifier = GetFinalModifier(baseModifier, numToHinder, numToImprove, ref improvedCount, ref hinderedCount, boostedParamString, ref increase, stat.Key, stat.Value, _logger);
                ApplyStat(finalModifier, increase, stat);
            }
        }

        protected override void ApplyStat(float finalModifier, bool increase, KeyValuePair<string, bool> stat, T genericRecord = null)
        {
            // Simply for logging
            float outOldValue = -1;
            float outNewValue = -1;

            // If we got declared generic we take their values for reroll, and if not, use it as actual item record.
            if (genericRecord == null)
            {
                genericRecord = itemRecord;
            }

            switch (stat.Key)
            {
                case "BallisticType":
                    //var values = Enum.GetValues(typeof(AmmoBallisticType));
                    //var randEnum = Helpers._random.Next(0, values.Length);
                    //itemRecord.BallisticType = (AmmoBallisticType)values.GetValue(randEnum);
                    break;

                case "MinAmmoAmount":
                    PathOfQuasimorph.raritySystem.Apply<int>(v => itemRecord.MinAmmoAmount = v, () => genericRecord.MinAmmoAmount, finalModifier, increase, out outOldValue, out outNewValue);
                    break;

                case "MaxAmmoAmount":
                    PathOfQuasimorph.raritySystem.Apply<int>(v => itemRecord.MaxAmmoAmount = v, () => genericRecord.MaxAmmoAmount, finalModifier, increase, out outOldValue, out outNewValue);
                    break;

                case "AmmoType":
                    // Skip for now
                    // itemRecord.AmmoType = AmmoTypes[Helpers._random.Next(0, AmmoTypes.Count)];
                    break;

                case "DmgType":
                    //itemRecord.DmgType = DmgTypes[Helpers._random.Next(0, DmgTypes.Count)];
                    break;

                case "DmgCritChance":
                    PathOfQuasimorph.raritySystem.Apply<float>(v => itemRecord.DmgCritChance = v, () => genericRecord.DmgCritChance, finalModifier, increase, out outOldValue, out outNewValue);
                    break;

                case "RangeBonus":
                    PathOfQuasimorph.raritySystem.Apply<int>(v => itemRecord.RangeBonus = v, () => genericRecord.RangeBonus, finalModifier, increase, out outOldValue, out outNewValue);
                    break;

                case "AccuracyMult":
                    PathOfQuasimorph.raritySystem.Apply<float>(v => itemRecord.AccuracyMult = v, () => genericRecord.AccuracyMult, finalModifier, increase, out outOldValue, out outNewValue);
                    break;

                case "ScatterMult":
                    PathOfQuasimorph.raritySystem.Apply<float>(v => itemRecord.ScatterMult = v, () => genericRecord.ScatterMult, finalModifier, increase, out outOldValue, out outNewValue);
                    break;

                case "DamageMult":
                    PathOfQuasimorph.raritySystem.Apply<float>(v => itemRecord.DamageMult = v, () => genericRecord.DamageMult, finalModifier, increase, out outOldValue, out outNewValue);
                    break;

                case "BulletCastsPerShot":
                    // PathOfQuasimorph.raritySystem.Apply<int>(v => itemRecord.BulletCastsPerShot = v, () => genericRecord.BulletCastsPerShot, finalModifier, increase, out outOldValue, out outNewValue);
                    break;

                case "StatusEffectId":
                    break;

                case "StatusDamageModifier":
                    PathOfQuasimorph.raritySystem.Apply<float>(v => itemRecord.StatusDamageModifier = v, () => genericRecord.StatusDamageModifier, finalModifier, increase, out outOldValue, out outNewValue);
                    break;

                case "StatusResistModifier":
                    PathOfQuasimorph.raritySystem.Apply<float>(v => itemRecord.StatusResistModifier = v, () => genericRecord.StatusResistModifier, finalModifier, increase, out outOldValue, out outNewValue);
                    break;

                case "Traits":
                    break;

                case "ProjectileId":
                    break;

                default:
                    base.ApplyStat(finalModifier, increase, stat, genericRecord);
                    return;
            }

            Plugin.Logger.Log($"\t\t old value {outOldValue}");
            Plugin.Logger.Log($"\t\t new value {outNewValue}");
        }

        internal void RerollRandomStat(SynthraformerRecord recomb, MetadataWrapper metadata, bool blockHinder)
        {
            var genericRecord = Data.Items.GetSimpleRecord<T>(metadata.Id, true);

            float baseModifier, finalModifier;
            int numToHinder, numToImprove, improvedCount, hinderedCount;
            string boostedParamString;
            bool increase;
            PrepGenericData(out baseModifier, out finalModifier, out numToHinder, out numToImprove, out boostedParamString, out improvedCount, out hinderedCount, out increase);

            var statIdx = Helpers._random.Next(0, parameters.Count);
            var stat = parameters.ElementAt(statIdx);

            if (blockHinder)
            {
                hinderedCount = 999; // Test
            }

            finalModifier = GetFinalModifier(baseModifier, numToHinder, numToImprove, ref improvedCount, ref hinderedCount, boostedParamString, ref increase, stat.Key, stat.Value, _logger);
            ApplyStat(finalModifier, increase, stat, genericRecord);
        }

        internal void ReplaceAmmoTraits(SynthraformerRecord record, MetadataWrapper metadata, float removeChance, bool keepGeneric)
        {
            ApplyTraits(true, removeChance, keepGeneric);
        }

        internal void RerollBallisticType(SynthraformerRecord record, MetadataWrapper metadata)
        {
            // This breaks game as it's unexpected behavior.
            var values = Enum.GetValues(typeof(AmmoBallisticType));
            var randEnum = Helpers._random.Next(0, values.Length);
            itemRecord.BallisticType = (AmmoBallisticType)values.GetValue(randEnum);
        }

        internal void RerollDamageType(SynthraformerRecord record, MetadataWrapper metadata)
        {
            itemRecord.DmgType = DmgTypes[Helpers._random.Next(0, DmgTypes.Count)];
        }

        internal void RerollAmmoType(SynthraformerRecord record, MetadataWrapper metadata)
        {
            // This breaks game as it's unexpected behavior.
            itemRecord.AmmoType = AmmoTypes[Helpers._random.Next(0, AmmoTypes.Count)];
        }

    }
}