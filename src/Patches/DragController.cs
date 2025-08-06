using HarmonyLib;
using MGSC;
using QM_PathOfQuasimorph.Core.Records;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QM_PathOfQuasimorph.Core
{
    internal partial class PathOfQuasimorph
    {
        [HarmonyPatch(typeof(DragController), "CanPutInSlot")]
        public static class DragController_CanPutInSlot_Patch
        {
            public static bool Prefix(DragController __instance, ref bool __result, ItemSlot slot)
            {
                if (__instance._draggableItem.Is<SynthraformerRecord>())
                {
                    if (__instance._draggableItem == null)
                    {
                        return true;
                    }

                    if (slot.Item != null && slot.Item.IsImplicit)
                    {
                        return true;
                    }

                    if (__instance._dragMode == DragMode.RepairMode)
                    {
                        //Plugin.Logger.Log($"DragController_CanPutInSlot_Patch");
                        //Plugin.Logger.Log($"_dragMode {__instance._dragMode}");
                        __result = slot.Item != null && ItemInteractionSystem.CanRepair(slot.Item, __instance._draggableItem);
                        //Plugin.Logger.Log($"__result {__result}");

                        return false;
                    }

                    return false;
                }

                return true;
            }
        }
    }
}
