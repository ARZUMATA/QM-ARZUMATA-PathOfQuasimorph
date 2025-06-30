using HarmonyLib;
using MGSC;

namespace QM_PathOfQuasimorph.Core
{
    internal partial class PathOfQuasimorph
    {
        // We block original method. We can do IL-patch but I'm not that smart.
        //[HarmonyPatch(typeof(MagnumDevelopmentSystem), nameof(MagnumDevelopmentSystem.InjectItemRecord))]
        //public static class MagnumDevelopmentSystem_InjectItemRecord_Patch
        //{
        //    public static bool Prefix(MagnumProject project)
        //    {
        //        //Plugin.Logger.Log($"\t MagnumDevelopmentSystem_InjectItemRecord_Patch End");
        //        return true; // Block original method.
        //    }

        //    public static void Postfix(MagnumProject project)
        //    {
        //        //Plugin.Logger.Log($" MagnumDevelopmentSystems_InjectItemRecord_Patch : Postfix");
        //    }
        //}


        [HarmonyPatch(typeof(MagnumProjects), nameof(MagnumProjects.OnAfterLoad))]
        public static class MagnumProjects_OnAfterLoad_Patch
        {
            public static void Postfix(MagnumProjects __instance)
            {
                Plugin.Logger.Log($" MagnumProjects_OnAfterLoad_Patch : Postfix");
                InjectProjectRecords(__instance);
            }
        }








    }
}
