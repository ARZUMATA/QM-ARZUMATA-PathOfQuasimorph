using HarmonyLib;
using MGSC;
using System;

namespace QM_PathOfQuasimorph.Core
{
    internal partial class PathOfQuasimorph
    {
        // We reuse original method with edits.
        // We add check for our custom projects via
        //   magnumProject.StartTime
        //   magnumProject.FinishTime
        // Normal projects in MagnumProjects.Values have both as non-zero.
        // Our project have StartTime = 0, and FinishTime > 0 and it's good for finding them later on.

        [HarmonyPatch(typeof(MagnumProjects), nameof(MagnumProjects.Get))]
        public static class MagnumProjects_Get_Patch
        {
            public static void Postfix(string devId, ref MagnumProject __result, MagnumProjects __instance)
            {
                // Result can be null if you start new project, so...
                if (__result != null && MagnumPoQProjectsController.IsPoqProject(__result))
                {
                    __result = null;
                }
            }
        }
    }
}
