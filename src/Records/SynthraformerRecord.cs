using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MGSC;
using QM_PathOfQuasimorph.Core;
using UnityEngine;
using static QM_PathOfQuasimorph.Controllers.SynthraformerController;

namespace QM_PathOfQuasimorph.Records
{
    // Helper struct to define allowed combinations
    public readonly struct AllowedTarget
    {
        public readonly Type ItemType;
        public readonly List<ItemRarity> AllowedRarities;

        public AllowedTarget(Type itemType, params ItemRarity[] rarities)
        {
            ItemType = itemType;
            AllowedRarities = new List<ItemRarity>(rarities);
        }

        public bool IsRarityAllowed(ItemRarity rarity) => AllowedRarities.Contains(rarity);
    }

    // We will use repair item approach as it's viable and suits our needs.
    public class SynthraformerRecord : RepairRecord, IStackableRecord, IAllowInVestRecord
    {
        static AllowedTarget AllowAll(Type type) => new AllowedTarget(type); // Empty list = all rarities allowed

        static AllowedTarget BlockStandard(Type type) =>
            new AllowedTarget(
                type,
                ItemRarity.Enhanced,
                ItemRarity.Advanced,
                ItemRarity.Premium,
                ItemRarity.Prototype,
                ItemRarity.Quantum
            );

        public string GetId()
        {
            return $"{BaseId}_{(int)Type}";
        }

        public string BaseId { get; set; }

        public override int InventorySortOrder => 40;

        public SynthraformerType Type { get; set; }

        // Whitelist dictionary: maps each SynthraformerType to allowed record types
        // Now includes both type and allowed rarities
        public readonly Dictionary<SynthraformerType, List<AllowedTarget>> AllowedTargetsByType =
            new Dictionary<SynthraformerType, List<AllowedTarget>>
            {
                {
                    SynthraformerType.PrimalCore,
                    new List<AllowedTarget>
                    {
                        AllowAll(typeof(WeaponRecord)),
                        AllowAll(typeof(HelmetRecord)),
                        AllowAll(typeof(ArmorRecord)),
                        AllowAll(typeof(LeggingsRecord)),
                        AllowAll(typeof(BootsRecord)),
                        AllowAll(typeof(AmmoRecord)),
                        AllowAll(typeof(ImplantRecord)),
                        AllowAll(typeof(AugmentationRecord)),
                    }
                },
                {
                    SynthraformerType.Rarity,
                    new List<AllowedTarget>
                    {
                        AllowAll(typeof(WeaponRecord)),
                        AllowAll(typeof(HelmetRecord)),
                        AllowAll(typeof(ArmorRecord)),
                        AllowAll(typeof(LeggingsRecord)),
                        AllowAll(typeof(BootsRecord)),
                        AllowAll(typeof(AmmoRecord)),
                        AllowAll(typeof(ImplantRecord)),
                        AllowAll(typeof(AugmentationRecord)),
                    }
                },
                {
                    SynthraformerType.Infuser,
                    new List<AllowedTarget>
                    {
                        BlockStandard(typeof(AmmoRecord)),
                    }
                },
                {
                    SynthraformerType.Traits,
                    new List<AllowedTarget>
                    {
                        BlockStandard(typeof(WeaponRecord)),
                        BlockStandard(typeof(AmmoRecord)),
                    }
                },
                {
                    SynthraformerType.Indestructible,
                    new List<AllowedTarget>
                    {
                        BlockStandard(typeof(BreakableItemRecord))
                    }
                },
                {
                    SynthraformerType.Amplifier,
                    new List<AllowedTarget>
                    {
                        BlockStandard(typeof(WeaponRecord)),
                        BlockStandard(typeof(HelmetRecord)),
                        BlockStandard(typeof(ArmorRecord)),
                        BlockStandard(typeof(LeggingsRecord)),
                        BlockStandard(typeof(BootsRecord)),
                        //BlockStandard(typeof(AmmoRecord)),
                        //typeof(ImplantRecord),
                        //typeof(AugmentationRecord),
                    }
                },
                {
                    SynthraformerType.Transmuter,
                    new List<AllowedTarget>
                    {
                        BlockStandard(typeof(AmmoRecord)),
                        AllowAll(typeof(WeaponRecord)),
                    }
                },
                {
                    SynthraformerType.Catalyst,
                    new List<AllowedTarget>
                    {
                        AllowAll(typeof(WeaponRecord)),
                        AllowAll(typeof(ImplantRecord)),
                        AllowAll(typeof(AugmentationRecord)),
                    }
                },
                {
                    SynthraformerType.Azure,
                    new List<AllowedTarget>
                    {
                        BlockStandard(typeof(AmmoRecord)),
                    }
                },
            };

        // Check if the given type is allowed
        internal bool IsValidTarget(PickupItem target, SynthraformerRecord synthraformerRecord)
        {
            // We don't process magnum crafted items, period.
            if (MetadataWrapper.IsMagnumProjectItemUid(target.Id))
            {
                return false;
            }

            //Plugin.Logger.Log($"IsValidTarget");
            //Plugin.Logger.Log($"target {target.Id}");
            //Plugin.Logger.Log($"Type {synthraformerRecord.Type}");
            if (!AllowedTargetsByType.TryGetValue(synthraformerRecord.Type, out var allowedTargets))
            {
                Plugin.Logger.Log(
                    "NO MATCH - No allowed targets defined for this SynthraformerType"
                );
                return false;
            }

            var metadata = RecordCollection.MetadataWrapperRecords.GetOrAdd(target.Id, MetadataWrapper.SplitItemUid);

            ItemRarity targetRarity = metadata.RarityClass;

            foreach (BasePickupItemRecord record in target._records)
            {
                Type recordType = record.GetType();

                foreach (var allowedTarget in allowedTargets)
                {
                    if (allowedTarget.ItemType.IsAssignableFrom(recordType))
                    {
                        // If no rarities specified → all allowed
                        if (allowedTarget.AllowedRarities.Count == 0)
                        {
                            Plugin.Logger.Log($"MATCH {recordType.Name}: All rarities allowed");
                            return true;
                        }

                        // Otherwise check if current rarity is allowed
                        if (allowedTarget.IsRarityAllowed(targetRarity))
                        {
                            Plugin.Logger.Log(
                                $"MATCH {recordType.Name} with rarity {targetRarity}"
                            );
                            return true;
                        }
                        else
                        {
                            Plugin.Logger.Log(
                                $"BLOCKED: {recordType.Name} has disallowed rarity {targetRarity}"
                            );
                        }
                    }
                }
            }

            Plugin.Logger.Log("NO MATCH - No valid type/rarity combination found");
            return false;
        }

        public SynthraformerRecord() { }
    }
}
