using HarmonyLib;
using MGSC;
using QM_PathOfQuasimorph.Controllers;
using QM_PathOfQuasimorph.Records;
using QM_PathOfQuasimorph.PoqHelpers;
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

        [HarmonyPatch(typeof(ItemTooltipBuilder),
            nameof(ItemTooltipBuilder.Build),
            new Type[]
            {
                typeof(BasePickupItemRecord),
            }
        )]
        public static class ItemTooltipBuilder_Build_Patch_BasePickupItemRecord
        {
            public static bool Prefix(ItemTooltipBuilder __instance, BasePickupItemRecord itemRecord)
            {
                CompositeItemRecord compositeItemRecord = itemRecord as CompositeItemRecord;
                SynthraformerRecord synRec = compositeItemRecord.GetRecord<SynthraformerRecord>();

                if (synRec == null)
                {
                    return true;
                }
                
                //Plugin.Logger.Log($"ItemTooltipBuilder_Build_Patch_BasePickupItemRecord");
                PathOfQuasimorph.tooltipGeneratorPoq.BuildSynthraformerTooltip(__instance, synRec);

                return false;
            }
        }

        [HarmonyPatch(typeof(ItemTooltipBuilder),
            nameof(ItemTooltipBuilder.Build),
            new Type[]
            {
                        typeof(BasePickupItem),
                        typeof(Player),
                        typeof(Mercenary),
            }
        )]
        public static class ItemTooltipBuilder_Build_Patch_BasePickupItem
        {
            public static bool Prefix(ItemTooltipBuilder __instance, BasePickupItem item, Player player, Mercenary mercenary = null)
            {
                //Plugin.Logger.Log($"ItemTooltipBuilder_Build_Patch_BasePickupItem");

                if (SynthraformerController.Is(item.Id))
                {
                    var synRec = item.Record<SynthraformerRecord>();

                    //Plugin.Logger.Log($"SynthraformerRecord null {synRec == null}");

                    if (synRec == null)
                    {
                        return true;
                    }

                     PathOfQuasimorph.tooltipGeneratorPoq.BuildSynthraformerTooltip(__instance, synRec);

                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(ItemTooltipBuilder), "InitRepair")]
        public static class ItemTooltipBuilder_InitRepair_Patch
        {
            public static bool Prefix(ItemTooltipBuilder __instance, RepairRecord record)
            {
                //Plugin.Logger.Log($"record.Id {record.Id}");
                //Plugin.Logger.Log($"SynthraformerController.Is(record.Id) {SynthraformerController.Is(record.Id)}");

                if (SynthraformerController.Is(record.Id))
                {
                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(ItemTooltipBuilder), "BuildAdditionalInfo")]
        public static class ItemTooltipBuilder_BuildAdditionalInfo_Patch
        {
            public static bool Prefix(ItemTooltipBuilder __instance, BasePickupItem item, BasePickupItemRecord record, Mercenary mercenary = null)
            {
                SynthraformerRecord synRec = null;

                if (item != null)
                {
                    synRec = item.Record<SynthraformerRecord>();
                }
                else if (record != null)
                {
                    CompositeItemRecord compositeItemRecord = record as CompositeItemRecord;
                    synRec = compositeItemRecord.GetRecord<SynthraformerRecord>();
                }

                if (synRec != null)
                {
                    PathOfQuasimorph.tooltipGeneratorPoq.BuildSynthraformerTooltip(__instance, synRec, true);
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
    }
}
