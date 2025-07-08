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
        [HarmonyPatch(typeof(ObjHighlightController), "Process")]
        public static class Patch_ObjHighlightController_Process
        {
            public static void Postfix(CellPosition cellUnderCursor, ObjHighlightController __instance)
            {
                TooltipGeneratorPoq.HandlePoqTooltipMonster(cellUnderCursor, __instance);
            }
        }


        [HarmonyPatch(typeof(ObjHighlightController), "Unhighlight")]
        public static class Patch_ObjHighlightController_Unhighlight
        {
            public static void Postfix()
            {
                TooltipGeneratorPoq.HandlePoqTooltipMonsterRemove();
            }
        }
    }
}
