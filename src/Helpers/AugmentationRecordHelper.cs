using MGSC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QM_PathOfQuasimorph.PoQHelpers
{
    internal class AugmentationRecordHelper
    {
        internal static AugmentationRecord CloneAugmentationRecord(AugmentationRecord original, string newId)
        {

            AugmentationRecord clone = new AugmentationRecord
            {
                Id = newId,
                WoundSlotIds = original.WoundSlotIds is null ? null : new List<string>(original.WoundSlotIds),
            };

            return clone;
        }
    }
}
