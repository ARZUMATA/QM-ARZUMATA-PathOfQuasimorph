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
        internal static WoundSlotRecord CloneWoundSlotRecord(WoundSlotRecord original, string newId)
        {
            Plugin.Logger.Log($"WoundSlotRecord: CloneWoundSlotRecord");

            WoundSlotRecord clone = ReflectionHelper.CloneViaProperties(original);
            clone.Id = newId;

            // Deep copy dictionaries
            clone.CoreEffects = DataSerializerHelper.MakeDeepCopy(original.CoreEffects);
            clone.AmputationCoreEffects = DataSerializerHelper.MakeDeepCopy(original.AmputationCoreEffects);
            clone.ImplicitBonusEffects = DataSerializerHelper.MakeDeepCopy(original.ImplicitBonusEffects);
            clone.ImplicitPenaltyEffects = DataSerializerHelper.MakeDeepCopy(original.ImplicitPenaltyEffects);

            // Deep copy lists
            clone.IgnoreEffects = DataSerializerHelper.MakeDeepCopy(original.IgnoreEffects);
            clone.IgnoreStatusEffects = DataSerializerHelper.MakeDeepCopy(original.IgnoreStatusEffects);
            clone.AmputatedDrop = DataSerializerHelper.MakeDeepCopy(original.AmputatedDrop);
            clone.AdditionalAmputation = DataSerializerHelper.MakeDeepCopy(original.AdditionalAmputation);

            return clone;
        }

        internal static AugmentationRecord CloneAugmentationRecord(AugmentationRecord original, string newId)
        {
            Plugin.Logger.Log($"AugmentationRecord: CloneAugmentationRecord");
            AugmentationRecord clone = ReflectionHelper.CloneViaProperties(original);

            // Override with new ID
            clone.Id = newId;

            // Deep copy known mutable collections
            clone.WoundSlotIds = DataSerializerHelper.MakeDeepCopy(original.WoundSlotIds);
            clone.Categories = DataSerializerHelper.MakeDeepCopy(original.Categories);

            Plugin.Logger.Log($"\t CloneAugmentationRecord");
            Plugin.Logger.Log($"\t original {original.Id}");
            Plugin.Logger.Log($"\t ContentDescriptor {original.ContentDescriptor}");
            Plugin.Logger.Log($"\t ItemDesc {original.ItemDesc}");

            return clone;
        }

        internal static ImplantRecord CloneImplantRecord(ImplantRecord original, string newId)
        {
            Plugin.Logger.Log($"ImplantRecord: CloneImplantRecord");

            ImplantRecord clone = ReflectionHelper.CloneViaProperties(original);
            clone.Id = newId;

            clone.NatureTypes = DataSerializerHelper.MakeDeepCopy(original.NatureTypes);
            clone.ImplicitBonusEffects = DataSerializerHelper.MakeDeepCopy(original.ImplicitBonusEffects);
            clone.ImplicitPenaltyEffects = DataSerializerHelper.MakeDeepCopy(original.ImplicitPenaltyEffects);
            clone.ChargeItemTypes = DataSerializerHelper.MakeDeepCopy(original.ChargeItemTypes);

            return clone;
        }
    }
}
