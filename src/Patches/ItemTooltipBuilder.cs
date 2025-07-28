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
        [HarmonyPatch(typeof(ItemTooltipBuilder), "InitAugmentation")]
         public static class ItemTooltipBuilder_InitAugmentation_Patch
        {
            public static void Postfix(ItemTooltipBuilder __instance, AugmentationRecord record, AugmentationComponent augComponent)
            {
                // So we can add our comparions to the aug tooltip
                __instance._tooltip.ShowAdditionalBlock();
            }
        }
    }
}
