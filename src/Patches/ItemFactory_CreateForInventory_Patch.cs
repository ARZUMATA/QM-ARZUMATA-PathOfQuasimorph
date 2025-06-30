using HarmonyLib;
using MGSC;

namespace QM_PathOfQuasimorph.Core
{
    internal partial class PathOfQuasimorph
    {
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

                        itemId = magnumProjectsController.CreateMagnumProjectWithMods(itemProjectType, itemId); ;
                        Plugin.Logger.Log($"\t  newId: {itemId}"); // pmc_shotgun_1_custom or pmc_shotgun_1_custom_poq_epic_1234567890


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








    }
}
