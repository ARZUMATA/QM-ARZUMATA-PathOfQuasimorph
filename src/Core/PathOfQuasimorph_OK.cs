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
        /* RGP themed weapon tiers
           1. **Standard**
           2. **Enhanced** // Magical
           3. **Advanced** // Rare
           4. **Premium** // Epic
           5. **Prototype** // Legendary
           6. **Quantum** // Mythic

        //Arcane
        //Exotic
        //Mythic
        //Relic
        //Premium

        * Name project cleanedDevId: devID_custom_poq_prototype_rndhash
       */
       

        /*
         * Our naming structure is different, so we check for the presence of "_poq_" and split the string to get the base.Id without our additions on top of default method.
         */
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

        // Access to MagnumProjects instance class.
        [HarmonyPatch(typeof(ComponentsLayout), nameof(ComponentsLayout.CreateGlobalComponents))]
        public static class ComponentsLayout_CreateGlobalComponents_Patch
        {
            public static void Postfix(ref ComponentsLayout __instance)
            {
                if (magnumProjectsController == null)
                {
                    Plugin.Logger.LogWarning("MagnumPoQProjectsController missing. Creating one.");
                    MagnumProjects magnumProjects = __instance._state.Get<MagnumProjects>(); // Assuming _state stores it
                    magnumProjectsController = new MagnumPoQProjectsController(magnumProjects);
                    Plugin.Logger.Log($"\t\t test: {magnumProjectsController.magnumProjects.Values.Count}"); // What projects we made in-game.
                }
            }
        }

        // We reuse original method with edits.
        // We add check for our custom projects via DateTime.StartTime.
        // Normal projects don't have DateTime.MinValue and it's good for finding them later on.
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
