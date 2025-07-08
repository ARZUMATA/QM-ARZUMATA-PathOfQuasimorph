using HarmonyLib;
using MGSC;

namespace QM_PathOfQuasimorph.Core
{
    internal partial class PathOfQuasimorph
    {
        // Access to MagnumProjects instance class.
        [HarmonyPatch(typeof(ComponentsLayout), nameof(ComponentsLayout.CreateGlobalComponents))]
        public static class ComponentsLayout_CreateGlobalComponents_Patch
        {
            public static void Postfix(ref ComponentsLayout __instance)
            {
                MagnumProjects magnumProjects = __instance._state.Get<MagnumProjects>(); // Assuming _state stores it

                if (magnumProjectsController == null)
                {
                    Plugin.Logger.LogWarning("MagnumPoQProjectsController missing. Creating one.");
                    magnumProjectsController = new MagnumPoQProjectsController(magnumProjects);
                    Plugin.Logger.Log($"\t\t magnumProjects Count: {MagnumPoQProjectsController.magnumProjects.Values.Count}"); // What projects we made in-game.
                }

                MagnumPoQProjectsController.magnumProjects = magnumProjects;
            }
        }
    }
}
