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

                        if (__instance.Item != null && __instance.Item.Is<AmplifierRecord>())
                        {
                            //Plugin.Logger.Log($"AmplifierRecord");
                            AmplifierContext.Item = __instance.Item;
                            AmplifierContext.Process = true;
                            AmplifierContext.Rarity = __instance.Item.Record<AmplifierRecord>().Rarity;
                        }

                        else if (__instance.Item != null && __instance.Item.Is<RecombinatorRecord>())
                        {
                            //Plugin.Logger.Log($"RecombinatorRecord");
                            RecombinatorContext.Item = __instance.Item;
                            RecombinatorContext.Process = true;
                        }
                        else
                        {
                            //Plugin.Logger.Log($"No AmplifierRecord and No RecombinatorRecord");
                            AmplifierContext.Process = false;
                            RecombinatorContext.Process = false;
                        }
                    }
                }

                // If mod not enabled, don't create any more backgrounds.
                if (!Plugin.Config.Enable)
                {
                    return;
                }

                ApplyItemRarityBackground(__instance, __instance.Item);
            }
        }
    }
}