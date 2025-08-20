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
    internal class AmmoRecordProcessorPoq : ItemRecordProcessor<AmmoRecord>
    {
        public AmmoRecordProcessorPoq(ItemRecordsControllerPoq controller) : base(controller) { }
        private new Logger _logger = new Logger(null, typeof(AmmoRecordProcessorPoq));

        public override Dictionary<string, bool> parameters => _parameters;

        internal Dictionary<string, bool> _parameters = new Dictionary<string, bool>()
        {
            //{ "BallisticType", true },
            { "MinAmmoAmount", true },
            { "MaxAmmoAmount", true },
            //{ "AmmoType", true },
            //{ "DmgType", true },
            { "DmgCritChance", true },
            { "RangeBonus", true },
            { "AccuracyMult", true },
            { "ScatterMult", false },
            { "DamageMult", true },
            { "BulletCastsPerShot", true },
            //{ "StatusEffectId", true },
            { "StatusDamageModifier", true },
            { "StatusResistModifier", true },
            //{ "Traits", true },
            //{ "ProjectileId", true },
        };

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
            new HashSet<string> { "explosive_light_flak", "explosive_flak" },
        };

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
        private void ApplyStat(float finalModifier, bool increase, KeyValuePair<string, bool> stat, AmmoRecord genericRecord = null)
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
            }

            Plugin.Logger.Log($"\t\t old value {outOldValue}");
            Plugin.Logger.Log($"\t\t new value {outNewValue}");
        }

        internal void ApplyTraits(bool clearTraits = false, float removeChance = 0.2f, bool keepGeneric = false)
        {
            Plugin.Logger.Log($"ApplyTraits: clearTraits args: {clearTraits}, removeChance: {removeChance}, keepGeneric: {keepGeneric}");

            var ammoRecord = Data.Items.GetSimpleRecord<AmmoRecord>(oldId, true);

            Plugin.Logger.Log($"ammoRecord null: {ammoRecord == null}");
            Plugin.Logger.Log($"itemRecord null: {itemRecord.Id}");

            var extraTraitCount = 0;

            // If we keep generic, recheck chance.
            if (keepGeneric && ammoRecord != null)
            {
                keepGeneric = Helpers._random.NextDouble() < removeChance;
            }
            else
            {
                keepGeneric = false;
            }

            Plugin.Logger.Log($"Keeping generic? {keepGeneric}");

            // Existing traits
            Plugin.Logger.Log($"\tExisting traits: {itemRecord.Traits.Count}");

            foreach (var trait in itemRecord.Traits)
            {
                Plugin.Logger.Log($"\t\t {trait}");
            }

            // Generic traits
            if (ammoRecord != null)
            {
                Plugin.Logger.Log($"\tGeneric traits: {ammoRecord.Traits.Count}");

                foreach (var trait in ammoRecord.Traits)
                {
                    Plugin.Logger.Log($"\t\t {trait}");
                }

                extraTraitCount = keepGeneric ? 0 : ammoRecord.Traits.Count;
                Plugin.Logger.Log($"\textraTraitCount: {extraTraitCount}");
            }

            // Apply traits to record
            // Should we remove existing traits?
            if (clearTraits)
            {
                itemRecord.Traits.Clear();

                if (keepGeneric && ammoRecord != null)
                {
                    Plugin.Logger.Log($"Keeping generic? Yes.");
                    itemRecord.Traits.AddRange(ammoRecord.Traits);
                }
                else
                {
                    Plugin.Logger.Log($"Keeping generic? No.");
                }
            }
            else
            {
                // Randomly decide whether to remove existing traits (20% chance)
                if (Helpers._random.NextDouble() < removeChance)
                {
                    Plugin.Logger.Log($"Keeping existing? Yes.");
                    itemRecord.Traits.Clear();
                }
                else
                {
                    Plugin.Logger.Log($"Keeping existing? No.");

                }
            }

            // Select traits
            List<string> selectedTraits = PrepareTraits(extraTraitCount);

            Plugin.Logger.Log($"\tSelectedTraits traits: {selectedTraits.Count}");

            foreach (var trait in selectedTraits)
            {
                Plugin.Logger.Log($"\t\t {trait}");
            }


            // Add traits
            for (int i = 0; i < selectedTraits.Count; i++)
            {
                itemRecord.Traits.Add(selectedTraits[i]);
            }

            Plugin.Logger.Log($"\tNew traits: {itemRecord.Traits.Count}");

            foreach (var trait in itemRecord.Traits)
            {
                Plugin.Logger.Log($"\t\t {trait}");
            }
        }
        private List<string> PrepareTraits(int extraTraitCount)
        {
            // Allowed traits for item type
            var allowedTraits = itemRecordsControllerPoq.GetAddeableTraits(ItemTraitType.AmmoTrait);

            _logger.Log($"AmmoRecord PrepareTraits: allowedTraits {allowedTraits.Count}");

            Dictionary<string, int> allTraitsCombined = allowedTraits
            .ToDictionary(
                trait => trait,        // key: the string itself
                trait => 5             // value: constant 5 for each
            );

            // Determine total number of traits to add based on rarity
            var totalTraitCount = PathOfQuasimorph.raritySystem.GetTraitCountByRarity(itemRarity, allTraitsCombined.Count + extraTraitCount);

            // Select traits based on weights
            var selectedTraits = SelectWeightedTraits(allTraitsCombined, totalTraitCount, itemRecord.Traits, traitsMutuallyExclusiveGroups);

            //if (removeExisting)
            //{
            //    // Remove already present traits
            //    selectedTraits.RemoveAll(t => itemRecord.Traits.Contains(t));
            //}


            // Filter all traits if they are not in allowed list (just in case)
            selectedTraits.RemoveAll(t => !allowedTraits.Contains(t));
            return selectedTraits;
        }

        internal void RerollRandomStat(SynthraformerRecord recomb, MetadataWrapper metadata)
        {
            var genericRecord = Data.Items.GetSimpleRecord<AmmoRecord>(metadata.Id, true);

            float baseModifier, finalModifier;
            int numToHinder, numToImprove, improvedCount, hinderedCount;
            string boostedParamString;
            bool increase;
            PrepGenericData(out baseModifier, out finalModifier, out numToHinder, out numToImprove, out boostedParamString, out improvedCount, out hinderedCount, out increase);

            var statIdx = Helpers._random.Next(0, parameters.Count);
            var stat = parameters.ElementAt(statIdx);

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