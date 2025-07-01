using HarmonyLib;
using MGSC;

namespace QM_PathOfQuasimorph.Core
{
    internal partial class PathOfQuasimorph
    {
        [HarmonyPatch(typeof(ItemSlot), nameof(ItemSlot.LateUpdate))]
        public static class ItemSlot_LateUpdate_Patch
        {
            public static void Postfix(ItemSlot __instance)
            {
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