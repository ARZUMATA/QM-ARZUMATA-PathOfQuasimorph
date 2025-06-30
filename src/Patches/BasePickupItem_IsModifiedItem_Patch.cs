using HarmonyLib;
using MGSC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QM_PathOfQuasimorph.Core
{
    internal partial class PathOfQuasimorph
    {
        [HarmonyPatch(typeof(BasePickupItem), "get_IsModifiedItem")]
        public static class BasePickupItem_IsModifiedItem_Patch
        {
            public static void Postfix(PickupItem __instance, ref bool __result)
            {
                if (__instance.Id.Contains("_poq_"))
                {
                    __result = false;
                }
            }
        }
    }
}
