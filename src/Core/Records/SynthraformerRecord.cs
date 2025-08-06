using MGSC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static QM_PathOfQuasimorph.Core.SynthraformerController;

namespace QM_PathOfQuasimorph.Core.Records
{
    // We will use repair item approach as it's viable and suits our needs.
    public class SynthraformerRecord : RepairRecord, IStackableRecord, IAllowInVestRecord
    {
        public override int InventorySortOrder
        {
            get
            {
                return 40;
            }
        }

        public SynthraformerType Type { get; set; }

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
                    typeof(HelmetRecord),
                }
            },
            {
                SynthraformerType.Indestructible,
                new List<Type> { typeof(BreakableItemRecord) }
            }

        };

        // Check if the given type is allowed
        internal bool IsValidTarget(PickupItem target, SynthraformerRecord synthraformerRecord)
        {
            //Plugin.Logger.Log($"IsValidTarget");
            //Plugin.Logger.Log($"target {target.Id}");
            //Plugin.Logger.Log($"Type {synthraformerRecord.Type}");

            if (synthraformerRecord.Type == SynthraformerType.Amplifier)
            {
                var allowedTypes = AllowedTypesByType[SynthraformerType.Amplifier];

                foreach (BasePickupItemRecord record in target._records)
                {
                    Type recordType = record.GetType();
                    //Plugin.Logger.Log($"recordType {recordType.Name}");

                    // Check if any allowed type is assignable from the current record's type
                    if (allowedTypes.Any(allowedType => allowedType.IsAssignableFrom(recordType)))
                    {
                        Plugin.Logger.Log($"MATCH {recordType.Name} (via inheritance)");
                        return true;
                    }
                }
            }

            Plugin.Logger.Log($"NO MATCH");

            return false;
        }

        public SynthraformerRecord()
        {
        }
    }
}
