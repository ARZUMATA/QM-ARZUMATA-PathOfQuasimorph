using HarmonyLib;
using MGSC;
using QM_PathOfQuasimorph.Core.Records;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEngine.UI.Image;
using Type = System.Type;

namespace QM_PathOfQuasimorph.Core
{
    internal partial class PathOfQuasimorph
    {
        [HarmonyPatch(typeof(ItemInteractionSystem), nameof(ItemInteractionSystem.CanRepair))]

        // TODO: IL patch
        public static class ItemInteractionSystem_CanRepair_Patch
        {
            public static bool Prefix(BasePickupItem target, BasePickupItem repair, ref bool __result)
            {
                Plugin.Logger.Log($"ItemInteractionSystem_CanRepair_Patch");

                if (target.Comp<BreakableItemComponent>() == null || (!repair.Is<AmplifierRecord>() || !repair.Is<RecombinatorRecord>()) || target.Locked)
                {
                    // Do original method
                    return true;
                }

                RepairRecord repairRecord = repair.Record<RepairRecord>();
                __result = repairRecord.IsValidCategory(target);// || breakableItemRecord.RepairItemIds.Contains(repairRecord.Id);
                return false;
            }
        }

        [HarmonyPatch(typeof(ItemInteractionSystem), nameof(ItemInteractionSystem.Repair))]

        public static class ItemInteractionSystem_Repair_Patch
        {
            public static bool Prefix(BasePickupItem target, BasePickupItem repair, Inventory inventory, out bool disassembleToTrash, ref bool __result)
            {
                //Plugin.Logger.Log($"ItemInteractionSystem_Repair_Patch");

                disassembleToTrash = false;

                if (!repair.Is<AmplifierRecord>() && !repair.Is<RecombinatorRecord>())
                {
                    return true;
                }

                if (target.Is<ImplantRecord>() || target.Is<AugmentationRecord>() || target.Is<ResistRecord>() || target.Is<WeaponRecord>())
                {
                    RepairRecord repairRecord = repair.Record<RepairRecord>();
                    Plugin.Logger.Log($"target.Is<RepairRecord>() {repair.Is<RepairRecord>()}");

                    if (!repairRecord.IsValidCategory(target))
                    {
                        Plugin.Logger.Log($"Invalid category");

                        // Do original method
                        return true;
                    }

                    if (repair.Is<AmplifierRecord>() && PathOfQuasimorph.itemRecordsControllerPoq.ChangeRecordFromAmplifier(target, repair))
                    {
                        ItemInteractionSystem.ConsumeItem(repair);
                        __result = true;
                        return false;
                    }
                }

                // Do original method
                return true;
            }
        }
    }
}
