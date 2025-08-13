using HarmonyLib;
using MGSC;
using System;
using System.Collections.Generic;
using System.Linq;
using static QM_PathOfQuasimorph.Controllers.MagnumPoQProjectsController;

namespace QM_PathOfQuasimorph.Core
{
    internal partial class PathOfQuasimorph
    {
        [HarmonyPatch(typeof(MagnumProjectsWindow), nameof(MagnumProjectsWindow.Configure))]
        public static class MagnumProjectsWindow_Configure_Patch
        {
            static List<MagnumProject> tempProjects = new List<MagnumProject>();

            public static bool Prefix(MagnumProjectType projectType, int maxProjects, MagnumProjectsWindow __instance)
            {
                Plugin.Logger.Log($"MagnumProjectsWindow_Configure_Patch");

                // Temporarily remove projects that are not PoQ projects so they are not shown in craft.
                foreach (var project in magnumProjects.Values.ToList())
                {
                    var wrapper = MetadataWrapper.SplitItemUid(MetadataWrapper.GetPoqItemIdFromProject(project));

                    if (wrapper.PoqItem || wrapper.SerializedStorage)
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