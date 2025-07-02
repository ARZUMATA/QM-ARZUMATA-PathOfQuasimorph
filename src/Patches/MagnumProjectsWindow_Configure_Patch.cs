using HarmonyLib;
using MGSC;
using System.Collections.Generic;
using System.Linq;
using static QM_PathOfQuasimorph.Core.MagnumPoQProjectsController;

namespace QM_PathOfQuasimorph.Core
{
    internal partial class PathOfQuasimorph
    {
        // I tried IL patch... T_T
        [HarmonyPatch(typeof(MagnumProjectsWindow), nameof(MagnumProjectsWindow.Configure))]
        public static class MagnumProjectsWindow_Configure_Patch
        {
            static List<MagnumProject> tempProjects = new List<MagnumProject>();

            public static bool Prefix(MagnumProjectType projectType, int maxProjects, MagnumProjectsWindow __instance)
            {
                // Temporarily remove projects that are not PoQ projects so they are not shown in craft.
                foreach (var project in magnumProjects.Values.ToList())
                {
                    var wrapper = MagnumProjectWrapper.SplitItemUid(MagnumProjectWrapper.GetPoqItemId(project));

                    if (wrapper.PoqItem)
                    {
                        tempProjects.Add(project);
                        magnumProjects.Values.Remove(project);
                    }
                }

                return true;
            }

            public static void Postfix(MagnumProjectType projectType, int maxProjects, MagnumProjectsWindow __instance)
            {
                magnumProjects.Values.AddRange(tempProjects);
                tempProjects.Clear();
            }
        }
    }
}