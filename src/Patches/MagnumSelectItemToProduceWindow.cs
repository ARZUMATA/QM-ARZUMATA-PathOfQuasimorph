using HarmonyLib;
using MGSC;
using QM_PathOfQuasimorph.Core.Records;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using TMPro;
using UnityEngine;
using static MGSC.MagnumSelectItemToProduceWindow;
using static QM_PathOfQuasimorph.Core.MagnumPoQProjectsController;

namespace QM_PathOfQuasimorph.Core
{
    internal partial class PathOfQuasimorph
    {
        private enum ReceiptCategoryPoq
        {
            Poq = 108,
        }

        [HarmonyPatch(typeof(MagnumSelectItemToProduceWindow), nameof(MagnumSelectItemToProduceWindow.Awake))]
        public static class MagnumSelectItemToProduceWindow_Awake_Patch
        {
            private static GameObject btnPoq;
            private static MagnumSelectItemToProduceWindow magnumSelectItemToProduceWindow;
            public static bool requestedPoqItems;
            private static LocalizableLabel localizableLabel;

            public static void Postfix(MagnumSelectItemToProduceWindow __instance)
            {
                Plugin.Logger.Log($"MagnumSelectItemToProduceWindow_Configure_Patch");

                if (btnPoq == null)
                {
                    magnumSelectItemToProduceWindow = __instance;

                    var window = __instance.transform.Find("Window");
                    var categoriesBlock = window.transform.Find("CategoriesBlock");
                    var btnOther = categoriesBlock.transform.Find("BtnOther");

                    btnPoq = GameObject.Instantiate(btnOther.gameObject, btnOther.parent);
                    btnPoq.name = "BtnPathOfQuasimorph";

                    var btnPoqCommonButton = btnPoq.GetComponent<CommonButton>();

                    btnPoqCommonButton.OnClick -= __instance.OtherReceiptsButtonOnClick;
                    btnPoqCommonButton.OnClick += PoqReceiptsButtonOnClick;

                    var caption = btnPoq.transform.Find("Caption");
                    localizableLabel = caption.transform.gameObject.GetComponent<LocalizableLabel>();
                    localizableLabel.ChangeLabel("ui.itemproduction.synthraformers");

                    btnPoqCommonButton._captionTag = "ui.itemproduction.synthraformers";
                }
            }

            public static void PoqReceiptsButtonOnClick(CommonButton obj, int clickCount)
            {
                requestedPoqItems = true;
                magnumSelectItemToProduceWindow._receiptCategory = ReceiptCategory.Augmentations; // Don't care
                Plugin.Logger.Log($"magnumSelectItemToProduceWindow._receiptCategory {(int)magnumSelectItemToProduceWindow._receiptCategory}");
                magnumSelectItemToProduceWindow._receiptToProduce = null;
                magnumSelectItemToProduceWindow.InitPanels();
            }
        }

        [HarmonyPatch(typeof(MagnumSelectItemToProduceWindow), nameof(MagnumSelectItemToProduceWindow.InitPanels))]
        public static class MagnumSelectItemToProduceWindow_InitPanels_Patch
        {
            static List<MagnumProject> tempProjects = new List<MagnumProject>();
            public static bool Prefix(MagnumSelectItemToProduceWindow __instance)
            {
                foreach (var project in magnumProjects.Values.ToList())
                {
                    var wrapper = MetadataWrapper.SplitItemUid(MetadataWrapper.GetPoqItemIdFromProject(project));

                    if (wrapper.PoqItem || wrapper.SerializedStorage)
                    {
                        tempProjects.Add(project);
                        magnumProjects.Values.Remove(project);
                    }
                }

                return true;
            }
            public static void Postfix(MagnumSelectItemToProduceWindow __instance)
            {
                // Bring back old project (used for placeholder project)
                magnumProjects.Values.AddRange(tempProjects);
                tempProjects.Clear();

                //Plugin.Logger.Log($"MagnumSelectItemToProduceWindow_InitPanels_Patch");
                Plugin.Logger.Log($"_receiptCategory: {__instance._receiptCategory}");
                Plugin.Logger.Log($"requestedPoqItems: {MagnumSelectItemToProduceWindow_Awake_Patch.requestedPoqItems}");

                // Add synthraformers production items
                //foreach (var outputItem in SynthraformerController.recipesOutputItems)
                //{
                //    if (__instance._magnumCargo.UnlockedProductionItems.IndexOf(outputItem) == -1)
                //    {
                //        __instance._magnumCargo.UnlockedProductionItems.Add(outputItem);
                //    }
                //}

                if (MagnumSelectItemToProduceWindow_Awake_Patch.requestedPoqItems)
                {
                    __instance._receiptCategory = (ReceiptCategory)ReceiptCategoryPoq.Poq;
                    MagnumSelectItemToProduceWindow_Awake_Patch.requestedPoqItems = false;
                }

                foreach (ItemProduceReceipt itemProduceReceipt in Data.ProduceReceipts)
                {
                    //Plugin.Logger.Log($"itemProduceReceipt {itemProduceReceipt.Id} - {itemProduceReceipt.OutputItem}");
                    CompositeItemRecord compositeItemRecord = Data.Items.GetRecord(itemProduceReceipt.OutputItem, true) as CompositeItemRecord;
                    //Plugin.Logger.Log($"compositeItemRecord {compositeItemRecord.Id}");
                    //Plugin.Logger.Log($"CanDo? {__instance._receiptCategory == (ReceiptCategory)ReceiptCategoryPoq.Poq}");

                    if (__instance._receiptCategory == (ReceiptCategory)ReceiptCategoryPoq.Poq || __instance._receiptCategory == ReceiptCategory.All)
                    {
                        SynthraformerRecord record = compositeItemRecord.GetRecord<SynthraformerRecord>();

                        //Plugin.Logger.Log($"SynthraformerRecord null {record == null}");

                        if (record == null)
                        {
                            continue;
                        }
                        else
                        {
                            //Plugin.Logger.Log($"SynthraformerRecord panels");
                            Plugin.Logger.Log($"OutputItem {itemProduceReceipt.OutputItem}");

                            ItemReceiptPanel component = __instance._panelsPool.Take().GetComponent<ItemReceiptPanel>();
                            component.Initialize(__instance._magnumCargo, __instance._magnumSpaceship, __instance._magnumProjects, itemProduceReceipt, __instance._difficulty);
                            component.OnStartProduction += __instance.ReceiptPanelOnStartProduction;
                            component.transform.SetParent(__instance._panelsRoot, false);
                            component.transform.SetAsLastSibling();
                            __instance._receiptPanels.Add(component);
                        }
                    }
                }
            }
        }
    }
}
