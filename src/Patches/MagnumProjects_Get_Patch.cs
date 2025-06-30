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
           // Default method copied here.
           [HarmonyPrefix]
           public static bool Get(string devId, ref MagnumProject __result, MagnumProjects __instance)
           {
               foreach (MagnumProject magnumProject in __instance.Values)
               {
                   if (magnumProject.DevelopId.Equals(devId))
                   {
                       __result = magnumProject;
                       return false; // Return false to skip the original method
                   }

                   // PathOfQuasimorph ADD START
                   // DateTime MinValue = new DateTime(0L, DateTimeKind.Unspecified);
                   // DateTime MaxValue = new DateTime(3155378975999999999L, DateTimeKind.Unspecified);
                   // Both Int64

                   if (magnumProject.StartTime == DateTime.MinValue)
                   {
                       __result = null;
                       return false; // Return false to skip the original method
                   }

                   // PathOfQuasimorph END START
               }

               __result = null;
               return false; // Return false to skip the original method
           }
        }











    }
}
