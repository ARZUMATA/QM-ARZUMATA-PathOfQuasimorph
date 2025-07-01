using HarmonyLib;
using MGSC;
using System;
using UnityEngine;

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
                MagnumProject project = MagnumPoQProjectsController.GetProjectById(itemId);

                if (project == null)
                {

                    // Create new
                    MagnumProjectType itemProjectType = MagnumDevelopmentSystem.GetItemProjectType(itemId);
                    //Plugin.Logger.Log($"\t\t itemProjectType : {itemProjectType}");

                    if (
                        //itemProjectType == MagnumProjectType.Weapons ||
                        itemProjectType == MagnumProjectType.RangeWeapon ||
                        itemProjectType == MagnumProjectType.MeleeWeapon ||
                        //itemProjectType == MagnumProjectType.Armors ||
                        itemProjectType == MagnumProjectType.Armor ||
                        itemProjectType == MagnumProjectType.Helmet ||
                        itemProjectType == MagnumProjectType.Boots ||
                        itemProjectType == MagnumProjectType.Leggings
                        )
                    {
                        // Item is OK
                        itemId = magnumProjectsController.CreateMagnumProjectWithMods(itemProjectType, itemId);
                    }
                    //else if (itemProjectType == MagnumProjectType.None ||
                    //         itemProjectType == MagnumProjectType.Mercenary ||
                    //         itemProjectType == MagnumProjectType.MercenaryClass ||
                    //         itemProjectType == MagnumProjectType.QuasiPact ||
                    //         itemProjectType == MagnumProjectType.Augmentic)
                    //{
                    else
                    {
                        //Plugin.Logger.Log($"\t\t itemProjectType is NOT OK: {itemProjectType}");
                        // Skip if the project type is not OK
                        //return true;
                    }
                }

                return true;  // Allow original method.
            }

            public static void Postfix(ref string itemId, bool randomizeConditionAndCapacity,
                ref BasePickupItem __result,
                ItemFactory __instance)
            {
                // Here we need to apply traits and other stuff that magnum projects don't cover.
                // That's why we have traitsTracker in MagnumPoQProjectsController.
                // We don't rely on magnum project here so we just look for traits tracker and slap them here.

                if (magnumProjectsController.traitsTracker.Contains(__result.Id))
                {

                    if (magnumProjectsController.CanProcessItemRecord(__result.Id) == false)
                    {
                        return;
                    }

                    if (__result != null)
                    {
                        magnumProjectsController.raritySystem.ApplyTraits(ref __result);
                    }
                }
            }
        }
    }
}
