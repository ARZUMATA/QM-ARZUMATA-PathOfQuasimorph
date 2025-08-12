using HarmonyLib;
using MGSC;
using QM_PathOfQuasimorph.Core.Records;
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
                typeof(BasePickupItem),
                typeof(Player),
                typeof(Mercenary),
            }
        )]
        public static class ItemTooltipBuilder_Build_Patch
        {
            public static bool Prefix(ItemTooltipBuilder __instance, BasePickupItem item, Player player, Mercenary mercenary = null)
            {
                if (SynthraformerController.Is(item.Id))
                {
                    var synRec = item.Record<SynthraformerRecord>();

                    //Plugin.Logger.Log($"SynthraformerRecord null {synRec == null}");

                    if (synRec == null)
                    {
                        return true;
                    }

                    //Plugin.Logger.Log($"synRec.Type: {synRec.Type}");

                    __instance._tooltip = __instance._factory.BuildEmptyTooltip(true, false);

                    // Amplifiers have rarity
                    if (synRec.Type == SynthraformerController.SynthraformerType.Amplifier)
                    {
                        PropertiesTooltipHelper.SetCaption1(__instance._tooltip, Localization.Get($"item.{synRec.Id}.name"), __instance._factory.FirstLetterColor, RaritySystem.Colors[synRec.Rarity]);

                        PropertiesTooltipHelper.SetCaption2(__instance._tooltip, Localization.Get($"item.{synRec.Id}.shortdesc"), RaritySystem.Colors[synRec.Rarity]);

                        // Rarity
                        __instance._factory.AddPanelToTooltip().SetValue(synRec.Rarity.ToString().WrapInColor(RaritySystem.Colors[synRec.Rarity].Replace("#", string.Empty)));

                        // Quote
                        __instance._factory.AddPanelToTooltip().SetMultilineName(Localization.Get($"ui.quote.{synRec.Id}.{synRec.Rarity.ToString().ToLower()}.quote.nahuatl")).SetNameColor(Colors.AltGreen);
                    }
                    else
                    {
                        __instance._tooltip.SetCaption1(Localization.Get("item." + synRec.Id + ".name"), __instance._factory.FirstLetterColor);
                        __instance._tooltip.SetCaption2(Localization.Get("item." + synRec.Id + ".shortdesc"));

                        // Quote
                        __instance._factory.AddPanelToTooltip().SetMultilineName(Localization.Get($"ui.quote.{synRec.Id}.quote.nahuatl")).SetNameColor(Colors.AltGreen);
                    }

                    __instance._tooltip.ShowAdditionalBlock();

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
                if (SynthraformerController.Is(record.Id))
                {
                    // Gotta build our own since we use repair record direvatives
                    var synRec = (SynthraformerRecord)record;

                    // Amplifiers have rarity
                    if (synRec.Type == SynthraformerController.SynthraformerType.Amplifier)
                    {
                        // Rarity
                        __instance._factory.AddPanelToTooltip().SetValue(synRec.Rarity.ToString().WrapInColor(RaritySystem.Colors[synRec.Rarity].Replace("#", string.Empty)));

                        // Quote
                        __instance._factory.AddPanelToTooltip().SetMultilineName(Localization.Get($"ui.quote.{synRec.Id}.{synRec.Rarity.ToString().ToLower()}.quote.nahuatl")).SetNameColor(Colors.AltGreen);
                    }
                    else
                    {
                        // Quote
                        __instance._factory.AddPanelToTooltip().SetMultilineName(Localization.Get($"ui.quote.{synRec.Id}.quote.nahuatl")).SetNameColor(Colors.AltGreen);
                    }

                    __instance._tooltip.ShowAdditionalBlock();
                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(ItemTooltipBuilder), "BuildAdditionalInfo")]
        public static class ItemTooltipBuilder_BuildAdditionalInfo_Patch
        {
            public static void Postfix(ItemTooltipBuilder __instance, BasePickupItem item, BasePickupItemRecord record, Mercenary mercenary = null)
            {
                if (SynthraformerController.Is(item.Id))
                {
                    var synRec = item.Record<SynthraformerRecord>();

                    // Amplifiers have rarity
                    if (synRec.Type == SynthraformerController.SynthraformerType.Amplifier)
                    {
                        // Rarity
                        __instance._factory.AddPanelToTooltip().SetValue(synRec.Rarity.ToString().WrapInColor(RaritySystem.Colors[synRec.Rarity].Replace("#", string.Empty)));

                        // Quote
                        __instance._factory.AddPanelToTooltip().SetMultilineName(Localization.Get($"ui.quote.{synRec.Id}.{synRec.Rarity.ToString().ToLower()}.quote")).SetNameColor(Colors.AltGreen);

                    }
                    else
                    {
                        // Quote
                        __instance._factory.AddPanelToTooltip().SetMultilineName(Localization.Get($"ui.quote.{synRec.Id}.quote")).SetNameColor(Colors.AltGreen);
                    }

                    // Desc
                    __instance._factory.AddPanelToTooltip().SetMultilineName(Localization.Get($"item.{synRec.Id}.desc")).SetNameColor(Colors.DarkYellow);
                }
            }
        }
    }
}
