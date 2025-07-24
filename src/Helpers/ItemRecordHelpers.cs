using MGSC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QM_PathOfQuasimorph.PoQHelpers
{
    internal class ItemRecordHelpers
    {
        internal static AugmentationRecord CloneAugmentationRecord(AugmentationRecord original, string newId)
        {
            AugmentationRecord clone = new AugmentationRecord
            {
                AugmentationClass = original.AugmentationClass,
                TooltipIconTag = original.TooltipIconTag,
                WoundSlotIds = SerializationHelper.MakeDeepCopy(original.WoundSlotIds)
            };

            // Assign new Id
            clone.Id = newId;

            return clone;
        }

        internal static ImplantRecord CloneImplantRecord(ImplantRecord original, string newId)
        {
            ImplantRecord clone = new ImplantRecord
            {
                AugmentationClass = original.AugmentationClass,
                IsActive = original.IsActive,
                SlotType = original.SlotType,
                NatureTypes = original.NatureTypes?.ToList(),
                ImplicitBonusEffects = original.ImplicitBonusEffects?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                ImplicitPenaltyEffects = original.ImplicitPenaltyEffects?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                ChargeItemTypes = original.ChargeItemTypes?.ToList(),
                CapacitySize = original.CapacitySize
            };

            // Assign new Id
            clone.Id = newId;

            return clone;
        }
    }
}
