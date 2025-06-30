using HarmonyLib;
using MGSC;
using System;

namespace QM_PathOfQuasimorph.Core
{
    internal partial class PathOfQuasimorph
    {


        // It creates all projects possible at game start. Always.
        // We reuse original method. We can do IL-patch but I'm not that smart.
        [HarmonyPatch(typeof(MagnumProject), nameof(MagnumProject.InitRecord))]
        public static class MagnumProject_InitRecord_Patch
        {
            public static bool Prefix(MagnumProject __instance)
            {
                //Plugin.Logger.Log($"\t\t MagnumProject_InitRecord_Patch");
                //Plugin.Logger.Log($"\t\t MagnumProject null {__instance.DevelopId}");


                // Default method copied here.
                ConfigTableRecord configTableRecord = null;
                MagnumProjectPrice magnumProjectPrice = Data.MagnumProjectPrices.Get(__instance.ProjectType);
                __instance.ItemsGrades = magnumProjectPrice.ItemsGrades;
                __instance.ModifyLevelLimit = magnumProjectPrice.ModifyLevelLimit;

                // PathOfQuasimorph ADD Start
                ItemProduceReceipt itemProduceReceipt = Data.ProduceReceipts.Get(__instance.DevelopId);

                if (itemProduceReceipt == null)
                {
                    itemProduceReceipt = Data.ProduceReceipts[0]; // We won't use it anyway. Don't care.
                }

                var itemProduceReceipt2 = itemProduceReceipt;
                var itemProduceReceipt3 = itemProduceReceipt;
                var itemProduceReceipt4 = itemProduceReceipt;
                var itemProduceReceipt5 = itemProduceReceipt;

                // PathOfQuasimorph ADD End

                switch (__instance.ProjectType)
                {
                    case MagnumProjectType.RangeWeapon:
                    case MagnumProjectType.MeleeWeapon:
                        {
                            // PathOfQuasimorph ADD Start
                            //Plugin.Logger.Log($"\t\t MagnumProject_InitRecord_Patch");
                            //Plugin.Logger.Log($"\t\t magnumProjectPrice null {magnumProjectPrice == null}");
                            //Plugin.Logger.Log($"\t\t itemProduceReceipt null {itemProduceReceipt == null}");
                            //Plugin.Logger.Log($"\t\t __instance.ProjectType {__instance.ProjectType}");
                            //Plugin.Logger.Log($"\t\t Data.Items.GetSimpleRecord to get {__instance.DevelopId}");
                            //Plugin.Logger.Log($"\t\t configTableRecord null? 1 {configTableRecord == null}");
                            // PathOfQuasimorph ADD End

                            configTableRecord = Data.Items.GetSimpleRecord<WeaponRecord>(__instance.DevelopId, true);

                            // PathOfQuasimorph ADD Start
                            //Plugin.Logger.Log($"\t\t configTableRecord null? 2 {configTableRecord == null}");
                            // PathOfQuasimorph ADD End

                            // ItemProduceReceipt itemProduceReceipt = Data.ProduceReceipts.Get(__instance.DevelopId); // Original code
                            __instance.ModifyStartPrice = itemProduceReceipt.ModifyStartCost;
                            __instance.ModifyStep = itemProduceReceipt.ModifyStep;
                            if (itemProduceReceipt.ModifyItemsGrades.Count > 0)
                            {
                                __instance.ItemsGrades = itemProduceReceipt.ModifyItemsGrades;
                            }
                            if (itemProduceReceipt.ModifyLevelLimit != 0)
                            {
                                __instance.ModifyLevelLimit = itemProduceReceipt.ModifyLevelLimit;
                            }
                            break;
                        }
                    case MagnumProjectType.Armor:
                        {
                            configTableRecord = Data.Items.GetSimpleRecord<ArmorRecord>(__instance.DevelopId, true);
                            // ItemProduceReceipt itemProduceReceipt2 = Data.ProduceReceipts.Get(__instance.DevelopId); // Original code
                            __instance.ModifyStartPrice = itemProduceReceipt2.ModifyStartCost;
                            __instance.ModifyStep = itemProduceReceipt2.ModifyStep;
                            if (itemProduceReceipt2.ModifyItemsGrades.Count > 0)
                            {
                                __instance.ItemsGrades = itemProduceReceipt2.ModifyItemsGrades;
                            }
                            if (itemProduceReceipt2.ModifyLevelLimit != 0)
                            {
                                __instance.ModifyLevelLimit = itemProduceReceipt2.ModifyLevelLimit;
                            }
                            break;
                        }
                    case MagnumProjectType.Helmet:
                        {
                            configTableRecord = Data.Items.GetSimpleRecord<HelmetRecord>(__instance.DevelopId, true);
                            // ItemProduceReceipt itemProduceReceipt3 = Data.ProduceReceipts.Get(__instance.DevelopId); // Original code
                            __instance.ModifyStartPrice = itemProduceReceipt3.ModifyStartCost;
                            __instance.ModifyStep = itemProduceReceipt3.ModifyStep;
                            if (itemProduceReceipt3.ModifyItemsGrades.Count > 0)
                            {
                                __instance.ItemsGrades = itemProduceReceipt3.ModifyItemsGrades;
                            }
                            if (itemProduceReceipt3.ModifyLevelLimit != 0)
                            {
                                __instance.ModifyLevelLimit = itemProduceReceipt3.ModifyLevelLimit;
                            }
                            break;
                        }
                    case MagnumProjectType.Boots:
                        {
                            configTableRecord = Data.Items.GetSimpleRecord<BootsRecord>(__instance.DevelopId, true);
                            // ItemProduceReceipt itemProduceReceipt4 = Data.ProduceReceipts.Get(__instance.DevelopId); // Original code
                            __instance.ModifyStartPrice = itemProduceReceipt4.ModifyStartCost;
                            __instance.ModifyStep = itemProduceReceipt4.ModifyStep;
                            if (itemProduceReceipt4.ModifyItemsGrades.Count > 0)
                            {
                                __instance.ItemsGrades = itemProduceReceipt4.ModifyItemsGrades;
                            }
                            if (itemProduceReceipt4.ModifyLevelLimit != 0)
                            {
                                __instance.ModifyLevelLimit = itemProduceReceipt4.ModifyLevelLimit;
                            }
                            break;
                        }
                    case MagnumProjectType.Leggings:
                        {
                            configTableRecord = Data.Items.GetSimpleRecord<LeggingsRecord>(__instance.DevelopId, true);
                            // ItemProduceReceipt itemProduceReceipt5 = Data.ProduceReceipts.Get(__instance.DevelopId); // Original code
                            __instance.ModifyStartPrice = itemProduceReceipt5.ModifyStartCost;
                            __instance.ModifyStep = itemProduceReceipt5.ModifyStep;
                            if (itemProduceReceipt5.ModifyItemsGrades.Count > 0)
                            {
                                __instance.ItemsGrades = itemProduceReceipt5.ModifyItemsGrades;
                            }
                            if (itemProduceReceipt5.ModifyLevelLimit != 0)
                            {
                                __instance.ModifyLevelLimit = itemProduceReceipt5.ModifyLevelLimit;
                            }
                            break;
                        }
                    case MagnumProjectType.Mercenary:
                        {
                            configTableRecord = Data.MercenaryProfiles.GetRecord(__instance.DevelopId, true);
                            MercenaryProfileRecord mercenaryProfileRecord = (MercenaryProfileRecord)configTableRecord;
                            __instance.ModifyStartPrice = mercenaryProfileRecord.ModifyStartCost;
                            __instance.ModifyStep = mercenaryProfileRecord.ModifyStep;
                            if (mercenaryProfileRecord.ModifyLevelLimit != 0)
                            {
                                __instance.ModifyLevelLimit = mercenaryProfileRecord.ModifyLevelLimit;
                            }
                            break;
                        }
                    case MagnumProjectType.MercenaryClass:
                        {
                            configTableRecord = Data.MercenaryClasses.GetRecord(__instance.DevelopId, true);
                            MercenaryClassRecord mercenaryClassRecord = (MercenaryClassRecord)configTableRecord;
                            __instance.ModifyStartPrice = mercenaryClassRecord.ModifyStartCost;
                            __instance.ModifyStep = mercenaryClassRecord.ModifyStep;
                            break;
                        }
                }
                if (configTableRecord == null)
                {
                    throw new NotImplementedException("Failed apply modifications to " + __instance.DevelopId + ", no suitable records.");
                }
                __instance._properties = configTableRecord.GetType().GetProperties();

                return false; // Block original method.
            }

            public static void Postfix(MagnumProject __instance)
            {
            }
        }








    }
}
