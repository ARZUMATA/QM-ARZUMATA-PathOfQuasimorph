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
        private static void WaitForDevelopId(TimeSpan timeout, MagnumProject newProject)
        {
            var startTime = DateTime.UtcNow;

            while (DateTime.UtcNow - startTime < timeout)
            {
                if (Data.Items.Ids.Contains(newProject.DevelopId))
                {
                    Plugin.Logger.Log($"DevelopId found in Data.Items.Ids: {newProject.DevelopId}");
                    return;
                }

                if (Data.Items.Records.Any(r => r.Id == newProject.DevelopId))
                {
                    Plugin.Logger.Log($"DevelopId found in Data.Items.Records: {newProject.DevelopId}");
                    return;
                }

                Plugin.Logger.Log($"Awaiting: {newProject.DevelopId}");

                System.Threading.Thread.Sleep(1000); // Wait 1 second before checking again
            }

            Plugin.Logger.Log($"Timeout: DevelopId {newProject.DevelopId} not found within {timeout.TotalSeconds} seconds.");
        }
    }
}
