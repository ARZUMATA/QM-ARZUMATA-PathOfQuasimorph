using HarmonyLib;
using MGSC;
using System;
using TMPro;
using UnityEngine;

namespace QM_PathOfQuasimorph.Core
{
    internal partial class PathOfQuasimorph
    {
        [HarmonyPatch(typeof(UI), "Process")]
        public static class UI_Process_Patch
        {
            public static void Postfix(UI __instance)
            {
                // We can do transplier patch so every time we hold SHIFT, our tooltip shows.
                TooltipGeneratorPoq.HandlePoqTooltip();
            }
        }
    }
}