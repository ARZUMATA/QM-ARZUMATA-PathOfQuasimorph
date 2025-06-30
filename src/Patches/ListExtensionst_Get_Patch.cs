using HarmonyLib;
using MGSC;
using System;
using System.Collections.Generic;

namespace QM_PathOfQuasimorph.Core
{
    internal partial class PathOfQuasimorph
    {
        //[HarmonyPatch(typeof(ListExtensions), nameof(ListExtensions.Get))]
        [HarmonyPatch(typeof(ListExtensions), "Get", new Type[] {
            typeof(List<ItemProduceReceipt>), typeof(string) }
        )]

        public static class ListExtensionst_Get_Patch
        {
            // Instead of IL patching MagnumProject.InitRecord we can check for null here.
            // All items that become potential projects for PathOfQuasimorph but not available to the end user have no ItemReceipts at all as they are never craft.
            // However MagnumProject.InitRecord required the recipe, so we add sorbet output item as placeholder.
            // Here that approach can invalidate the recipe if added by other mods I believe. Needs testing.

            public static void Postfix(string outputId, ref ItemProduceReceipt __result)
            {
                if (__result == null || __result.Id == string.Empty)
                {
                    ItemProduceReceipt itemProduceReceiptPlaceHolder = magnumProjectsController.GetPlaceHolderItemProduceReceipt();
                    __result = itemProduceReceiptPlaceHolder;
                }
            }
        }

        // It creates all projects possible at game start.
        // Since it requires recipe for "new" items that are not available for crafting by default, we could hook or Il patch, but better to use ListExtensions.Get
        //[HarmonyPatch(typeof(MagnumProject), nameof(MagnumProject.InitRecord))]
        //public static class MagnumProject_InitRecord_Patch

    }
}
