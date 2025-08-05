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

        [HarmonyPatch(typeof(ItemTooltipBuilder), "InitImplant")]

        public static class ItemTooltipBuilder_InitImplant_Patch
        {
            public static void Postfix(ItemTooltipBuilder __instance, ImplantRecord record, Mercenary mercenary)
            {
                // So we can add our comparions to the aug tooltip
                __instance._tooltip.ShowAdditionalBlock();
            }
        }


        [HarmonyPatch(typeof(ItemTooltipBuilder), "InitRepair")]
        public static class ItemTooltipBuilder_InitRepair_Patch
        {
            public static void Postfix(ItemTooltipBuilder __instance, RepairRecord record)
            {
                if (SynthraformerController.Is(record.Id))
                {
                    __instance._tooltip.ShowAdditionalBlock();
          
                }
            }
        }
        [HarmonyPatch(typeof(ItemTooltipBuilder), "BuildAdditionalInfo")]
        public static class ItemTooltipBuilder_BuildAdditionalInfo_Patch
        {
            public static void Postfix(ItemTooltipBuilder __instance, BasePickupItem item, BasePickupItemRecord record, Mercenary mercenary = null)
            {
                if (SynthraformerController.Is(item.Id))
                {
                    __instance._factory.AddPanelToTooltip().SetMultilineName(Localization.Get($"item.{SynthraformerController.nameBase}.desc")).SetNameColor(Colors.AltGreen);
                }
            }
        }

    }
}
