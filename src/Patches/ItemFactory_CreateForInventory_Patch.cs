using HarmonyLib;
using MGSC;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;
using static QM_PathOfQuasimorph.Core.CreaturesControllerPoq;
using static QM_PathOfQuasimorph.Core.MagnumPoQProjectsController;
using static System.Runtime.CompilerServices.RuntimeHelpers;

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
                // If mod not enabled, don't create any more items.
                if (!Plugin.Config.Enable || Plugin.Config.CleanupMode)
                {
                    return true;
                }

                //Plugin.Logger.Log("ItemFactory_CreateForInventory_Patch :: Prefix :: Start");
                //Plugin.Logger.Log($"\t CreateForInventory: {itemId}"); // pmc_shotgun_1_custom or pmc_shotgun_1_custom_poq_epic_1234567890

                // Id here is either:
                // 1) Non mod item. i.e. pmc_shotgun_1
                // 2) Id with custom or pmc_shotgun_1_custom

                // We don't need to know existing project as we always create new items here.
                // ItemProductionSystem calls this method with _custom prefix.

                // Check if itemId is from ItemProductionSystem system.
                // Vanilla project i.e. just _custom
                if (itemId.EndsWith("_custom"))
                {
                    // It's item from production system. We don't apply rarity on crafted items.
                    if (Plugin.Config.ApplyRarityToMagnumItems == false)
                    {
                        return true;
                    }
                }

                if (ItemFactoryContext.CanDo == false)
                {
                    // Flag to block rarity apply for that item, skip and reset flag.
                    ItemFactoryContext.CanDo = true;
                    ItemFactoryContext.Context = "None";
                    Plugin.Logger.Log($"ItemFactoryContext.CanDo == {ItemFactoryContext.CanDo}. Context {ItemFactoryContext.Context}. Reverting flag back to original (true).");
                    return true;
                }

                //MagnumProject project = MagnumPoQProjectsController.GetProjectById(itemId);

                // No more magnum projects
                //if (false)//project == null)
                //{

                //    // Create new
                //    MagnumProjectType itemProjectType = MagnumDevelopmentSystem.GetItemProjectType(itemId);
                //    //Plugin.Logger.Log($"\t\t itemProjectType : {itemProjectType}");

                //    if (
                //        //itemProjectType == MagnumProjectType.Weapons ||
                //        itemProjectType == MagnumProjectType.RangeWeapon ||
                //        itemProjectType == MagnumProjectType.MeleeWeapon ||
                //        //itemProjectType == MagnumProjectType.Armors ||
                //        itemProjectType == MagnumProjectType.Armor ||
                //        itemProjectType == MagnumProjectType.Helmet ||
                //        itemProjectType == MagnumProjectType.Boots ||
                //        itemProjectType == MagnumProjectType.Leggings
                //        )
                //    {
                //        var rarityExtraBoost = false;

                //        // Item is OK
                //        if (MobContext.CurrentMobId != -1)
                //        {
                //            Plugin.Logger.Log($"ItemFactory_CreateForInventory called for Mob ID: {MobContext.CurrentMobId}");
                //            MobContext.CurrentMobId = -1;
                //            rarityExtraBoost = true;
                //        }
                //        else
                //        {
                //            rarityExtraBoost = false;
                //        }

                //        itemId = magnumProjectsController.CreateMagnumProjectWithMods(itemProjectType, itemId, rarityExtraBoost);
                //    }
                //    //else if (itemProjectType == MagnumProjectType.None ||
                //    //         itemProjectType == MagnumProjectType.Mercenary ||
                //    //         itemProjectType == MagnumProjectType.MercenaryClass ||
                //    //         itemProjectType == MagnumProjectType.QuasiPact ||
                //    //         itemProjectType == MagnumProjectType.Augmentic)
                //    //{
                //    else
                //    {
                //        //Plugin.Logger.Log($"\t\t itemProjectType is NOT OK: {itemProjectType}");
                //        // Skip if the project type is not OK
                //        //return true;
                //    }
                //}



                // Log stack trace and calling method
                /*
                StackTrace stackTrace = new StackTrace(true);
                string formattedStackTrace = stackTrace.ToString();
                Plugin.Logger.Log($"ItemFactory_CreateForInventory_Patch :: Prefix :: Called by: {formattedStackTrace}");
                */
                Plugin.Logger.Log($"ItemFactory_CreateForInventory_Patch");
                Plugin.Logger.Log($"wrapper {itemId}");
                var wrapper = MetadataWrapper.SplitItemUid(itemId);
                Plugin.Logger.Log($"wrapper == null {wrapper == null}");

                if (wrapper != null && (wrapper.PoqItem || wrapper.SerializedStorage))
                {
                    Plugin.Logger.Log($"wrapper.PoqItem {wrapper.PoqItem}");
                    Plugin.Logger.Log($"wrapper.SerializedStorage {wrapper.SerializedStorage}");

                    return true;
                }

                if (RecordCollection.HasRecord(itemId) == false)
                {
                    Plugin.Logger.Log($"RecordCollection.HasRecord(itemId) == false");

                    var mobRarityBoost = false;

                    // Item is OK
                    if (MobContext.CurrentMobId != -1)
                    {
                        Plugin.Logger.Log($"ItemFactory_CreateForInventory called for Mob ID: {MobContext.CurrentMobId}");

                        if (MobContext.Rarity == MonsterMasteryTier.None)
                        {
                            mobRarityBoost = false;
                        }

                        mobRarityBoost = true;
                    }
                    else
                    {
                        mobRarityBoost = false;
                    }

                    // Create new item record
                    itemId = PathOfQuasimorph.itemRecordsControllerPoq.InterceptAndReplaceItemId(itemId, mobRarityBoost, ItemRarity.Standard, true, false);
                }

                return true;  // Allow original method.
            }

            public static void Postfix(ref string itemId, bool randomizeConditionAndCapacity,
                ref BasePickupItem __result,
                ItemFactory __instance)
            {
                return;
                // Here we need to apply traits and other stuff that magnum projects don't cover.
                // That's why we have traitsTracker in MagnumPoQProjectsController.
                // We don't rely on magnum project here so we just look for traits tracker and slap them here.

                // On new game we can't access it yet.
                if (magnumProjectsController == null)
                {
                    return;
                }

                if (magnumProjectsController.traitsTracker.Contains(__result.Id))
                {

                    if (magnumProjectsController.CanProcessItemRecord(__result.Id) == false)
                    {
                        return;
                    }

                    if (__result != null)
                    {
                        PathOfQuasimorph.raritySystem.ApplyTraits(ref __result);
                    }
                }
            }
        }
    }
}
