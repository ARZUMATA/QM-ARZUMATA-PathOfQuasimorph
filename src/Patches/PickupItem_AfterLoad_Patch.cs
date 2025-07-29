using HarmonyLib;
using MGSC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static QM_PathOfQuasimorph.Core.MagnumPoQProjectsController;

namespace QM_PathOfQuasimorph.Core
{
    internal partial class PathOfQuasimorph
    {
        [HarmonyPatch(typeof(PickupItem), "OnAfterLoad")]
        public static class PickupItem_OnAfterLoad_Patch
        {
            public static bool Prefix(PickupItem __instance)
            {
                // Fallback verify if we have item record.
                // While everything should be ok, I need to ensure we don't break user save is some items changes mame break game logic.
                // So we revert item Id to the baseline if that issue occurs and log it, and let user continue playing.
                // Log this issue for future fix.
                CompositeItemRecord compositeItemRecord = Data.Items.GetRecord(__instance.Id, false) as CompositeItemRecord;

                if (compositeItemRecord == null)
                {
                    Plugin.Logger.LogWarning($"WARNING: PickupItem: compositeItemRecord == null {compositeItemRecord == null}");
                    Plugin.Logger.LogWarning($"WARNING: PickupItem: reverting id to the baseline.");
                    var magnumProjectWrapper = MetadataWrapper.SplitItemUid(__instance.Id);
                    __instance.Id = magnumProjectWrapper.Id; // This effectively makes item default.
                    Plugin.Logger.LogWarning($"{magnumProjectWrapper.Id}_magnumProjectWrapper.Id_FIXME");
                }

                return true;
            }
        }
    }
}
