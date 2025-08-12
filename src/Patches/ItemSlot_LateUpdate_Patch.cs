using HarmonyLib;
using MGSC;
using QM_PathOfQuasimorph.Core.Records;

namespace QM_PathOfQuasimorph.Core
{
    internal partial class PathOfQuasimorph
    {
        [HarmonyPatch(typeof(ItemSlot), nameof(ItemSlot.LateUpdate))]
        public static class ItemSlot_LateUpdate_Patch
        {
            public static void Postfix(ItemSlot __instance)
            {
                if (PathOfQuasimorph.GameLoopGroup == GameLoopGroup.Space)
                {
                    DragController drag = UI.Drag;
                    bool flag = __instance.IsPointerInside && !drag.IsDragging;

                    if (flag && drag._dragMode != DragMode.RepairMode)
                    {
                        //Plugin.Logger.Log($"ItemSlot_LateUpdate_Patch");

                        if (__instance.Item != null && __instance.Item.Is<SynthraformerRecord>())
                        {
                            //Plugin.Logger.Log($"SynthraformerRecord");
                            SynthraformerContext.Item = __instance.Item;
                            SynthraformerContext.Process = true;
                            SynthraformerContext.RecombinatorType = __instance.Item.Record<SynthraformerRecord>().Type;
                        }
                        else
                        {
                            //Plugin.Logger.Log($"No SynthraformerRecord and No RecombinatorRecord");
                            SynthraformerContext.Process = false;
                        }
                    }
                }

                // If mod not enabled, don't create any more backgrounds.
                if (!Plugin.Config.Enable)
                {
                    return;
                }

                ApplyItemRarityBackground(__instance.gameObject, __instance.Item);
            }
        }
    }
}