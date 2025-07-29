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
        [HarmonyPatch(typeof(EscScreen), "OnEnable")]
        public static class SpaceGameMode_EscScreenOnExitToMainMenu_Patch
        {
            public static bool Prefix(EscScreen __instance)
            {
                // Save our metadata right bewoe gamesave
                if (__instance._mode == EscScreen.Mode.Spacemode)
                {
                    Plugin.Logger.Log($"EscScreen.Mode.Spacemode : Can clean obsolete projects.");
                    CleanupSystem.CleanObsoleteProjects(_context, true, true);
                }
                else
                {
                    Plugin.Logger.Log($"EscScreen.Mode.Ingame : Holding on cleaning projects.");

                }

                Plugin.Logger.Log($"EscScreen");
                return true;
            }
        }
    }
}
