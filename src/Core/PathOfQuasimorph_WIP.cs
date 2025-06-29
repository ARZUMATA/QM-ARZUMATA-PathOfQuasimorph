using HarmonyLib;
using MGSC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace QM_PathOfQuasimorph.Core
{
    internal partial class PathOfQuasimorph
    {

        public static void UpdateKey(string lookupstr, string prefix, string suffix)
        {
            foreach (KeyValuePair<Localization.Lang, Dictionary<string, string>> languageToDict in Singleton<Localization>.Instance.db)
            {
                if (languageToDict.Value.ContainsKey(lookupstr))
                {
                    languageToDict.Value[lookupstr] = prefix + languageToDict.Value[lookupstr] + suffix;
                }
            }
        }

        // Other mods compatibility issue.
        // We created our own method using original one to avoid this.
        public static void InjectItemRecord(MagnumProject project, string newId)
        {
            // Default method copied here.
            // Plugin.Logger.Log($"\t MagnumDevelopmentSystem_InjectItemRecord_Patch Start");
            //Plugin.Logger.Log($"InjectItemRecord :: Prefix :: Start");

            string text = newId;//project.DevelopId + "_custom";
            //string text = project.DevelopId + "_custom";
            //Plugin.Logger.Log($"\t _InjectItemRecord project.DevelopId {project.DevelopId}");
            //Plugin.Logger.Log($"\t _InjectItemRecord text {text}");

            Localization.DuplicateKey("item." + project.DevelopId + ".name", "item." + text + ".name");
            Localization.DuplicateKey("item." + project.DevelopId + ".shortdesc", "item." + text + ".shortdesc");

            UpdateKey("item." + text + ".name", "SupaDupa", "T-800");
            UpdateKey("item." + text + ".shortdesc", "Power", "of Doom");


            CompositeItemRecord compositeItemRecord = Data.Items.GetRecord(project.DevelopId, true) as CompositeItemRecord;

            // PathOfQuasimorph ADD Start
            //Plugin.Logger.Log($"\t compositeItemRecord == null {compositeItemRecord == null}");
            // PathOfQuasimorph ADD End

            Data.Items.RemoveRecord(text);
            Data.ItemTransformation.RemoveRecord(text);
            ItemTransformationRecord record = Data.ItemTransformation.GetRecord(project.DevelopId, true);

            // PathOfQuasimorph ADD Start
            //Plugin.Logger.Log($"\t ItemTransformationRecord == null {record == null}");
            if (record == null)
            {
                //Plugin.Logger.Log($"\t ItemTransformationRecord null. Need add placeholder.");
                record = Data.ItemTransformation.GetRecord("broken_weapon", true); //Rusty Parts //We won't use it anyway. Don't care.
                //Plugin.Logger.Log($"\t ItemTransformationRecord text: {text}");
                //Plugin.Logger.Log($"\t ItemTransformationRecord record: {record}");
                //Plugin.Logger.Log($"\t ItemTransformationRecord record null?: {record == null}");
            }
            // PathOfQuasimorph ADD End

            Data.ItemTransformation.AddRecord(text, record.Clone(text));
            foreach (BasePickupItemRecord basePickupItemRecord in compositeItemRecord.Records)
            {
                WeaponRecord weaponRecord = basePickupItemRecord as WeaponRecord;
                if (weaponRecord != null)
                {
                    WeaponRecord weaponRecord2 = weaponRecord.Clone(text);
                    Data.Items.AddRecord(text, weaponRecord2);
                    project.ApplyModifications(weaponRecord2);
                }
                else
                {
                    ArmorRecord armorRecord = basePickupItemRecord as ArmorRecord;
                    if (armorRecord != null)
                    {
                        ArmorRecord armorRecord2 = armorRecord.Clone(text);
                        Data.Items.AddRecord(text, armorRecord2);
                        project.ApplyModifications(armorRecord2);
                    }
                    else
                    {
                        HelmetRecord helmetRecord = basePickupItemRecord as HelmetRecord;
                        if (helmetRecord != null)
                        {
                            HelmetRecord helmetRecord2 = helmetRecord.Clone(text);
                            Data.Items.AddRecord(text, helmetRecord2);
                            project.ApplyModifications(helmetRecord2);
                        }
                        else
                        {
                            LeggingsRecord leggingsRecord = basePickupItemRecord as LeggingsRecord;
                            if (leggingsRecord != null)
                            {
                                LeggingsRecord leggingsRecord2 = leggingsRecord.Clone(text);
                                Data.Items.AddRecord(text, leggingsRecord2);
                                project.ApplyModifications(leggingsRecord2);
                            }
                            else
                            {
                                BootsRecord bootsRecord = basePickupItemRecord as BootsRecord;
                                if (bootsRecord != null)
                                {
                                    Plugin.Logger.Log($"\t bootsRecord2");
                                    BootsRecord bootsRecord2 = bootsRecord.Clone(text);
                                    Data.Items.AddRecord(text, bootsRecord2);
                                    project.ApplyModifications(bootsRecord2);
                                }
                                else
                                {
                                    throw new NotImplementedException(string.Format("Failed create project {0}. No clone method for additional records: {1}.", project.DevelopId, basePickupItemRecord.GetType()));
                                }

                            }
                        }
                    }
                }
            }
        }


        // We block original method. We can do IL-patch but I'm not that smart.
        //[HarmonyPatch(typeof(MagnumDevelopmentSystem), nameof(MagnumDevelopmentSystem.InjectItemRecord))]
        //public static class MagnumDevelopmentSystem_InjectItemRecord_Patch
        //{
        //    public static bool Prefix(MagnumProject project)
        //    {
        //        //Plugin.Logger.Log($"\t MagnumDevelopmentSystem_InjectItemRecord_Patch End");
        //        return true; // Block original method.
        //    }

        //    public static void Postfix(MagnumProject project)
        //    {
        //        //Plugin.Logger.Log($" MagnumDevelopmentSystems_InjectItemRecord_Patch : Postfix");
        //    }
        //}


        [HarmonyPatch(typeof(MagnumProjects), nameof(MagnumProjects.OnAfterLoad))]
        public static class MagnumProjects_OnAfterLoad_Patch
        {
            public static void Postfix(MagnumProjects __instance)
            {
                Plugin.Logger.Log($" MagnumProjects_OnAfterLoad_Patch : Postfix");
                InjectProjectRecords(__instance);
            }
        }

        public static void InjectProjectRecords(MagnumProjects projects)
        {
            Plugin.Logger.Log($" InjectProjectRecords : Postfix");

            foreach (MagnumProject magnumProject in projects.Values)
            {
                InjectProjectRecord(magnumProject);
            }
        }

        public static void InjectProjectRecord(MagnumProject project)
        {
            Plugin.Logger.Log($" InjectProjectRecord : Postfix");

            switch (project.ProjectType)
            {
                case MagnumProjectType.RangeWeapon:
                    //case MagnumProjectType.MeleeWeapon:
                    //case MagnumProjectType.Armor:
                    //case MagnumProjectType.Helmet:
                    //case MagnumProjectType.Boots:
                    //case MagnumProjectType.Leggings:
                    if (project.StartTime == DateTime.MinValue)
                    {
                        var newId = magnumProjectsController.WrapProjectDateTime(project);
                        InjectItemRecord(project, newId);
                    }
                    return;
                default:
                    return;
            }
        }

        // We can hook it and intercept item creation.
        // That way we can create our own "projects" on the fly when needed. For example making one of the item custom and different rarity.
        [HarmonyPatch(typeof(ItemFactory), nameof(ItemFactory.CreateForInventory))]
        public static class ItemFactory_CreateForInventory_Patch
        {
            public static bool Prefix(ref string itemId, bool randomizeConditionAndCapacity, ref BasePickupItem __result, ItemFactory __instance)
            {
                //Plugin.Logger.Log("ItemFactory_CreateForInventory_Patch :: Prefix :: Start");
                //Plugin.Logger.Log($"\t CreateForInventory: {itemId}"); // pmc_shotgun_1_custom or pmc_shotgun_1_custom_poq_epic_1234567890

                // Id here is always non-mod as game is not aware of it. So we do our magic.
                // Also we don't need to know existing project as we always create new items here.
                MagnumProject project = magnumProjectsController.GetProjectById(itemId);

                // If project is not null, then we have a project for that item.
                //if (project != null)
                //{
                //    Plugin.Logger.Log($"\t Found project with DevelopId: {itemId}");
                //    Plugin.Logger.Log($"\t project DevelopId: {project.DevelopId}");
                //    Plugin.Logger.Log($"\t project StartTime: {project.StartTime}");
                //    Plugin.Logger.Log($"\t project FinishTime: {project.FinishTime}");
                //    Plugin.Logger.Log($"\t project DefaultRecord: {project.DefaultRecord}");
                //    Plugin.Logger.Log($"\t project CustomRecord: {project.CustomRecord}");
                //    Plugin.Logger.Log($"\t project IsInDevelopment: {project.IsInDevelopment}");
                //    itemId = itemId + "_custom"; //temp
                //}

                if (project == null)
                {
                    //Plugin.Logger.Log($"\t No project found with DevelopId: {itemId}");

                    // Create new
                    MagnumProjectType itemProjectType = MagnumDevelopmentSystem.GetItemProjectType(itemId);
                    //Plugin.Logger.Log($"\t\t itemProjectType : {itemProjectType}");

                    if (
                        //itemProjectType == MagnumProjectType.Weapons ||
                        itemProjectType == MagnumProjectType.RangeWeapon
                        //itemProjectType == MagnumProjectType.MeleeWeapon ||
                        //itemProjectType == MagnumProjectType.Armors ||
                        //itemProjectType == MagnumProjectType.Armor ||
                        //itemProjectType == MagnumProjectType.Helmet ||
                        //itemProjectType == MagnumProjectType.Boots ||
                        //itemProjectType == MagnumProjectType.Leggings
                        )
                    {
                        // Item is OK
                        //Plugin.Logger.Log($"\t Item cleanedDevId: {cleanedDevId}");
                        //Plugin.Logger.Log($"\t\t itemProjectType is OK: {itemProjectType}");
                        //try
                        //{
                        Plugin.Logger.Log($"\t  CreateForInventory: {itemId} {itemProjectType}"); // pmc_shotgun_1_custom or pmc_shotgun_1_custom_poq_epic_1234567890

                        //var newProject = new MagnumProject(itemProjectType, customDevId); //todo handle randomize creating in controller

                        var newId = magnumProjectsController.CreateMagnumProjectWithMods(itemProjectType, itemId);
                        itemId = newId;
                        Plugin.Logger.Log($"\t  newId: {newId}"); // pmc_shotgun_1_custom or pmc_shotgun_1_custom_poq_epic_1234567890


                    }
                    //else if (itemProjectType == MagnumProjectType.None ||
                    //         itemProjectType == MagnumProjectType.Mercenary ||
                    //         itemProjectType == MagnumProjectType.MercenaryClass ||
                    //         itemProjectType == MagnumProjectType.QuasiPact ||
                    //         itemProjectType == MagnumProjectType.Augmentic)
                    //{
                    else
                    {
                        // Skip if the project type is not OK
                        //Plugin.Logger.Log($"\t\t itemProjectType is NOT OK: {itemProjectType}");
                        //return true;
                    }
                }

                //Plugin.Logger.Log("ItemFactory_CreateForInventory_Patch :: Prefix :: End");

                return true;  // Allow original method.
            }

            public static void Postfix(string itemId, bool randomizeConditionAndCapacity, ref BasePickupItem __result, ItemFactory __instance)
            {
            }
        }

        // It creates all projects possible at game start. Always.
        // We reuse original method. We can do IL-patch but I'm not that smart.
        [HarmonyPatch(typeof(MagnumProject), nameof(MagnumProject.InitRecord))]
        public static class MagnumProject_InitRecord_Patch
        {
            public static bool Prefix(MagnumProject __instance)
            {
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




        [Hook(ModHookType.DungeonStarted)]
        public static void DungeonStarted(IModContext context)
        {
        }

        [Hook(ModHookType.DungeonFinished)]
        public static void DungeonFinished(IModContext context)
        {
            isInitialized = false;
        }

        [Hook(ModHookType.SpaceStarted)]
        public static void SpaceStarted(IModContext context)
        {
        }

        [Hook(ModHookType.SpaceFinished)]
        public static void SpaceFinished(IModContext context)
        {
            isInitialized = false;
        }


        [Hook(ModHookType.SpaceUpdateBeforeGameLoop)]
        public static void SpaceUpdateBeforeGameLoop(IModContext context)
        {
            if (!isInitialized)
            {
                try
                {
                }
                catch (Exception ex)
                {
                    Plugin.Logger.Log($"Error in ColorTuner initialization: {ex.Message}");
                }
            }
        }

        [Hook(ModHookType.DungeonUpdateBeforeGameLoop)]
        public static void DungeonUpdateBeforeGameLoop(IModContext context)
        {
            if (!isInitialized)
            {
                try
                {
                }
                catch (Exception ex)
                {
                    Plugin.Logger.Log($"Error in ColorTuner initialization: {ex.Message}");
                }
            }
        }








    }
}
