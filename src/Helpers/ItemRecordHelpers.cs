using MGSC;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static UnityEngine.UI.Image;

namespace QM_PathOfQuasimorph.PoQHelpers
{
    internal class ItemRecordHelpers
    {
        // In game the Clone method doesn't copy Traits that we need also it for some reason does a deep-copy on immutable fields like int and string.

        internal static ItemRecord CloneItemRecord(ItemRecord original, string newId)
        {
            Plugin.Logger.Log($"ItemRecord: CloneAmmoRecord");

            ItemRecord clone = ReflectionHelper.CloneViaProperties(original);
            clone.Id = newId;

            clone.Categories = SerializationHelper.MakeDeepCopy(original.Categories);

            return clone;
        }

        internal static AmmoRecord CloneAmmoRecord(AmmoRecord original, string newId)
        {
            Plugin.Logger.Log($"AmmoRecord: CloneAmmoRecord");

            AmmoRecord clone = ReflectionHelper.CloneViaProperties(original);
            clone.Id = newId;

            clone.Traits = SerializationHelper.MakeDeepCopy(original.Traits);

            return clone;
        }
        
        internal static WeaponRecord CloneWeaponRecord(WeaponRecord original, string newId)
        {
            Plugin.Logger.Log($"WeaponRecord: CloneWeaponRecord");

            WeaponRecord clone = ReflectionHelper.CloneViaProperties(original);
            clone.Id = newId;

            clone.RepairItemIds = SerializationHelper.MakeDeepCopy(original.RepairItemIds);
            clone.OverrideAmmo = SerializationHelper.MakeDeepCopy(original.OverrideAmmo);
            clone.Firemodes = SerializationHelper.MakeDeepCopy(original.Firemodes);
            clone.AllowedGrenadeIds = SerializationHelper.MakeDeepCopy(original.AllowedGrenadeIds);
            clone.Traits = SerializationHelper.MakeDeepCopy(original.Traits);

            return clone;
        }

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

            clone.ContentDescriptor = original.ContentDescriptor;

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

        internal static PerkRecord ClonePerkRecord(PerkRecord original, string newId)
        {
            Plugin.Logger.Log($"PerkRecord: ClonePerkRecord");

            PerkRecord clone = ReflectionHelper.CloneViaProperties(original);
            clone.Id = newId;

            clone.ActiveWeaponClassLimit = DataSerializerHelper.MakeDeepCopy(original.ActiveWeaponClassLimit);
            clone.ActiveWeaponSubClassLimit = DataSerializerHelper.MakeDeepCopy(original.ActiveWeaponSubClassLimit);
            clone.Parameters = DataSerializerHelper.MakeDeepCopy(original.Parameters);


            return clone;
        }


    }
}
