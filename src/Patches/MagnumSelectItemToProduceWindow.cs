using HarmonyLib;
using MGSC;
using System;
using System.Collections.Generic;
using System.Linq;
using static QM_PathOfQuasimorph.Core.MagnumPoQProjectsController;

namespace QM_PathOfQuasimorph.Core
{
    internal partial class PathOfQuasimorph
    {
        [HarmonyPatch(typeof(MagnumSelectItemToProduceWindow), nameof(MagnumSelectItemToProduceWindow.InitPanels))]
        public static class MagnumSelectItemToProduceWindow_Configure_Patch
        {
            static List<MagnumProject> tempProjects = new List<MagnumProject>();
            public static bool Prefix(MagnumSelectItemToProduceWindow __instance)
            {
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
            public static void Postfix(MagnumSelectItemToProduceWindow __instance)
            {
                magnumProjects.Values.AddRange(tempProjects);
                tempProjects.Clear();
            }
        }
    }
}
