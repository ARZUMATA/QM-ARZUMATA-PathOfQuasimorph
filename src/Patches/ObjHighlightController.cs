using HarmonyLib;
using MGSC;
using QM_PathOfQuasimorph.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QM_PathOfQuasimorph.Core
{
    internal partial class PathOfQuasimorph
    {
        [HarmonyPatch(typeof(ObjHighlightController), "Process")]
        public static class ObjHighlightController_Process_Patch
        {
            public static bool Prefix(ObjHighlightController __instance, CellPosition cellUnderCursor, MapObstacle mapObstacleUnderCursor, bool altPressed, bool altUp)
            {
                CreaturesControllerPoq.HighlightMobsPoq(cellUnderCursor, __instance);
                return true;
            }

            public static void Postfix(ObjHighlightController __instance, CellPosition cellUnderCursor, MapObstacle mapObstacleUnderCursor, bool altPressed, bool altUp)
            {
            }

            // Red highliht on enemies
            // UsesMonster Highlight
            [HarmonyPatch(typeof(ObjHighlightController), "HighlightAllInViewRadius")]
            public static class ObjHighlightController_HighlightAllInViewRadius_Patch
            {
                public static bool Prefix(ObjHighlightController __instance, bool val)
                {
                    return true;
                }
            }

            // Left thing with name and mouse icons
            [HarmonyPatch(typeof(ObjHighlightController), "RefreshMonsterTooltip")]
            public static class ObjHighlightController_RefreshMonsterTooltip_Patch
            {
                public static bool Prefix(ObjHighlightController __instance, Creature monster)
                {
                    //TooltipGeneratorPoq.HandlePoqTooltipMonster(monster); // Fires once so key should be pressed already
                    return true; // can add other metadata
                }

                public static void Postfix(ObjHighlightController __instance, Creature monster)
                {
                    __instance._hintPanel.AddAction("HighlightAllItems", "item.ledgerBook.shortdesc");
                }
            }

            [HarmonyPatch(typeof(ObjHighlightController), "CheckMonster")]
            public static class ObjHighlightController_CheckMonster_Patch
            {
                public static bool Prefix(ObjHighlightController __instance, CellPosition cellUnderCursor)
                {
                    TooltipGeneratorPoq.HandlePoqTooltipMonster(__instance, cellUnderCursor);
                    return true; // can add other metadata
                }

                public static void Postfix(ObjHighlightController __instance, CellPosition cellUnderCursor, bool __result)
                {
                    if (!__result)
                    {
                        //TooltipGeneratorPoq.HandlePoqTooltipMonsterRemove();
                    }
                }
            }

            [HarmonyPatch(typeof(ObjHighlightController), "Unhighlight")]
            public static class ObjHighlightController_Unhighlight_Patch
            {
                public static bool Prefix(ObjHighlightController __instance)
                {
                    return true;
                }

                public static void Postfix()
                {
                }
            }
        }
    }
}
