using HarmonyLib;
using MGSC;

namespace QM_PathOfQuasimorph.Core
{
    internal partial class PathOfQuasimorph
    {
        // Our naming structure is different, so we check for the presence of "_poq_" and split the string to get the base.Id without our additions on top of default method.
        [HarmonyPatch(typeof(PickupItem), "get_RenderId")]
        public static class PickupItem_RenderId_Patch
        {
            public static void Postfix(PickupItem __instance, ref string __result)
            {
                if (__result.Contains("_poq_"))
                {
                    var newResult = __result.Split('_');
                    __result = newResult[0]; // Return real base.Id
                }
            }
        }
    }
}
