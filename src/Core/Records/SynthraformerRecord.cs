using MGSC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static QM_PathOfQuasimorph.Core.SynthraformerController;

namespace QM_PathOfQuasimorph.Core.Records
{
    // We will use repair item approach as it's viable and suits our needs.
    public class SynthraformerRecord : RepairRecord, IStackableRecord, IAllowInVestRecord
    {
        public string GetId(bool rarity = false)
        {
            if (rarity)
            {
                return $"{BaseId}_{(int)Type}_{Rarity.ToString().ToLower()}";
            }

            return $"{BaseId}_{(int)Type}";
        }

        public string BaseId
        {
            get;
            set;
        }

        public override int InventorySortOrder
        {
            get
            {
                return 40;
            }
        }

        public SynthraformerType Type { get; set; }
        public ItemRarity Rarity { get; set; }

        // Whitelist dictionary: maps each SynthraformerType to allowed record types
        public static readonly Dictionary<SynthraformerType, List<Type>> AllowedTypesByType =
            new Dictionary<SynthraformerType, List<Type>>
        {
            {
                SynthraformerType.Amplifier,
                new List<Type> {
                    typeof(WeaponRecord),
                    typeof(HelmetRecord),
                    typeof(ArmorRecord),
                    typeof(LeggingsRecord),
                    typeof(BootsRecord),
                    typeof(AmmoRecord),
                    //typeof(ImplantRecord),
                    //typeof(AugmentationRecord),
                }
            },
            {
                SynthraformerType.Traits,
                new List<Type> {
                    typeof(WeaponRecord),
                    typeof(AmmoRecord),
                }
            },
            {
                SynthraformerType.Indestructible,
                new List<Type> { typeof(BreakableItemRecord) }
            },
            {
                SynthraformerType.RandomRarity,
                new List<Type>
                {
                    //typeof(WeaponRecord),
                    //typeof(HelmetRecord),
                    //typeof(ArmorRecord),
                    //typeof(LeggingsRecord),
                    //typeof(BootsRecord),
                    //typeof(AmmoRecord),
                    //typeof(ImplantRecord),
                    //typeof(AugmentationRecord),
                }
            },
            {
                SynthraformerType.Catalyst,
                new List<Type>
                { 
                    //typeof(AugmentationRecord),
                    //typeof(ImplantRecord)
                }
            }
        };

        // Check if the given type is allowed
        internal bool IsValidTarget(PickupItem target, SynthraformerRecord synthraformerRecord)
        {
            //Plugin.Logger.Log($"IsValidTarget");
            //Plugin.Logger.Log($"target {target.Id}");
            //Plugin.Logger.Log($"Type {synthraformerRecord.Type}");

            if (!AllowedTypesByType.TryGetValue(synthraformerRecord.Type, out var allowedTypes))
            {
                Plugin.Logger.Log("NO MATCH - No allowed types defined for this SynthraformerType");
                return false;
            }

            // Check if any allowed type is assignable from the current record's type

            foreach (BasePickupItemRecord record in target._records)
            {
                Type recordType = record.GetType();

                if (allowedTypes.Any(allowedType => allowedType.IsAssignableFrom(recordType)))
                {
                    Plugin.Logger.Log($"MATCH {recordType.Name} (via inheritance)");
                    return true;
                }
            }

            //Plugin.Logger.Log($"NO MATCH");

            return false;
        }

        public SynthraformerRecord()
        {
        }
    }
}
