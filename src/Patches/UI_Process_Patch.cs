using HarmonyLib;
using MGSC;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.OleDb;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using static QM_PathOfQuasimorph.Core.MagnumPoQProjectsController;
using static Unity.IO.LowLevel.Unsafe.AsyncReadManagerMetrics;
using static UnityEngine.Rendering.DebugUI;

namespace QM_PathOfQuasimorph.Core
{
    internal partial class PathOfQuasimorph
    {
        [HarmonyPatch(typeof(UI), "Process")]
        public static class UI_Process_Patch
        {
            public static void Postfix(UI __instance)
            {
                TooltipGeneratorPoq.HandlePoqTooltip();
            }
        }
    }
}