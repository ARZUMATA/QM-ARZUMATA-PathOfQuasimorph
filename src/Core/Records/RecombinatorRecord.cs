using MGSC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static QM_PathOfQuasimorph.Core.RecombinatorController;

namespace QM_PathOfQuasimorph.Core.Records
{
    // We will use repair item approach as it's viable and suits our needs.
    public class RecombinatorRecord : RepairRecord, IStackableRecord, IAllowInVestRecord
    {
        public override int InventorySortOrder
        {
            get
            {
                return 41;
            }
        }

        public RecombinatorType RecombinatorType { get; set; }

        // Whitelist dictionary: maps each RecombinatorType to allowed record types
        public static readonly Dictionary<RecombinatorType, List<Type>> AllowedTypesByType = 
            new Dictionary<RecombinatorType, List<Type>>
        {
            {
                RecombinatorType.WeaponTraits,
                new List<Type> { typeof(WeaponRecord) }
            },
            {
                RecombinatorType.WeaponRandoms,
                new List<Type> { typeof(AmmoRecord) }
            },
            {
                RecombinatorType.Indestructible,
                new List<Type> { typeof(BreakableItemRecord) }
            }
        };


        // Check if the given type is allowed for this recombimator's type
        public bool IsAllowed(Type t)
        {
            return AllowedTypesByType.TryGetValue(RecombinatorType, out var allowedTypes) &&
                   allowedTypes.Contains(t);
        }

        public RecombinatorRecord()
        {
        }
    }
}
