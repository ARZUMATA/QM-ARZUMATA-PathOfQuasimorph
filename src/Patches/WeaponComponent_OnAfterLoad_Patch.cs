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
        [HarmonyPatch(typeof(WeaponComponent), "OnAfterLoad")]
        public static class WeaponComponent_OnAfterLoad_Patch
        {
            public static bool Prefix(WeaponComponent __instance)
            {
                // Fallback verify if we have item record.
                // While everything should be ok, I need to ensure we don't break user save is some items changes mame break game logic.
                // So we revert item Id to the baseline if that issue occurs and log it, and let user continue playing.
                // Log this issue for future fix.

                try
                {
                    WeaponRecord weaponRecord = Data.Items.GetSimpleRecord<WeaponRecord>(__instance._weaponId, false);
                }
                catch
                {
                    Plugin.Logger.LogWarning($"WeaponComponent: Error on get GetSimpleRecord");
                    Plugin.Logger.LogWarning($"WeaponComponent: reverting id to the baseline.");

                    var magnumProjectWrapper = MagnumProjectWrapper.SplitItemUid(__instance._weaponId);
                    __instance._weaponId = magnumProjectWrapper.Id; // This effectively makes item default.
                }

                return true;
            }
        }
    }
}
