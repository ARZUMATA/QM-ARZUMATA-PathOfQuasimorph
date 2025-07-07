using MGSC;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static QM_PathOfQuasimorph.Core.MagnumPoQProjectsController;

namespace QM_PathOfQuasimorph.Core
{
    internal static class CleanupSystem
    {
        // Static variable to track elapsed time
        private static float lastIntervalCheckTime = 0f;
        private static readonly float interval = 60f; // e.g., every 5 seconds

        internal static List<string> CleanItemsInAutonomousCapsuleDepartment(AutonomousCapsuleDepartment department)
        {
            List<string> items = new List<string>();

            Plugin.Logger.Log("CleanItemsInAutonomousCapsuleDepartment");

            if (department.CapsuleStorage != null && department.CapsuleStorage.Items != null)
            {
                items.AddRange(CleanupPickupItem(department.CapsuleStorage.Items));
            }

            return items;
        }

        internal static List<string> CleanItemsInShuttleCargoDepartment(ShuttleCargoDepartment department)
        {
            List<string> items = new List<string>();

            Plugin.Logger.Log("CleanItemsInShuttleCargoDepartment");

            if (department.ShuttleCargo != null && department.ShuttleCargo.Items != null)
            {
                items.AddRange(CleanupPickupItem(department.ShuttleCargo.Items));
            }

            return items;
        }

        internal static List<string> CleanItemsInTradeShuttleDepartment(TradeShuttleDepartment department)
        {
            List<string> items = new List<string>();

            Plugin.Logger.Log("CleanItemsInTradeShuttleDepartment");

            if (department.ResultStorage != null && department.ResultStorage.Items != null)
            {
                items.AddRange(CleanupPickupItem(department.ResultStorage.Items));
            }

            if (department.TradeShuttleStorage != null && department.TradeShuttleStorage.Items != null)
            {
                items.AddRange(CleanupPickupItem(department.TradeShuttleStorage.Items));
            }

            return items;
        }

        internal static void CleanObsoleteProjects(IModContext context, bool cleanProjects = false)
        {
            List<string> idsToKeep = new List<string>();

            var listMagnumCargo = CleanupMagnumCargo(context);
            var listMissionRewards = CleanupMissionRewards(context);
            var listMercenariesCargo = CleanupMercenariesCargo(context);
            var listCreatureData = CleanupCreatureData(context);
            var listMagnumDepartmentsData = CleanupItemsMagnumDepartments(context);

            // Dedupe? There are not many entries anyway.
            idsToKeep.AddRange(listMagnumCargo);
            idsToKeep.AddRange(listMissionRewards);
            idsToKeep.AddRange(listMercenariesCargo);
            idsToKeep.AddRange(listCreatureData);
            idsToKeep.AddRange(listMagnumDepartmentsData);

            // Cleanup magnum projects.
            if (cleanProjects)
            {
                CleanupMagnumProjects(context, idsToKeep);
            }
        }

        internal static List<string> CleanupCreatureData(IModContext context)
        {
            Creatures creatures = context.State.Get<Creatures>();
            List<string> items = new List<string>();

            Plugin.Logger.Log("CleanupCreatureData");

            if (creatures != null)
            {
                Plugin.Logger.Log($"player creature {creatures.Player.CreatureData.UniqueId}");

                // Cleanup player data in raid
                foreach (ItemStorage storage in creatures.Player.CreatureData.Inventory.AllContainers)
                {
                    items.AddRange(CleanupPickupItem(storage.Items));
                }

                foreach (ItemStorage storage in creatures.Player.CreatureData.Inventory.WeaponSlots)
                {
                    items.AddRange(CleanupPickupItem(storage.Items));
                }

                // Cleanup monsters
                foreach (var creature in creatures.Monsters)
                {
                    var creatureData = creature.CreatureData;
                    if (creatureData == null)
                    {
                        continue;
                    }

                    Plugin.Logger.Log($"monster creature {creatureData.UniqueId}");

                    foreach (ItemStorage storage in creatureData.Inventory.AllContainers)
                    {
                        items.AddRange(CleanupPickupItem(storage.Items));
                    }
                   
                    // Part of all containers
                    //foreach (ItemStorage storage in creatureData.Inventory.Storages)
                    //{
                    //    items.AddRange(CleanupPickupItem(storage.Items));
                    //}

                    //foreach (ItemStorage storage in creatureData.Inventory.Slots)
                    //{
                    //    items.AddRange(CleanupPickupItem(storage.Items));
                    //}

                    foreach (ItemStorage storage in creatureData.Inventory.WeaponSlots)
                    {
                        items.AddRange(CleanupPickupItem(storage.Items));
                    }
                }
            }

            return items;
        }

        internal static void CleanupItem(PickupItem item)
        {
            var wrapper = MagnumProjectWrapper.SplitItemUid(item.Id);

            if (!wrapper.PoqItem)
            {
                return;
            }

            item.Id = wrapper.Id;
            foreach (var component in item.Components)
            {
                var weaponComponent = component as WeaponComponent;
                if (weaponComponent != null)
                {
                    weaponComponent._weaponId = item.Id;
                }

                var breakableItemComponent = component as BreakableItemComponent;
                if (breakableItemComponent != null)
                {
                    breakableItemComponent.Unbreakable = false;
                }
            }
        }

        internal static List<string> CleanupItemsMagnumDepartments(IModContext context)
        {
            List<string> items = new List<string>();

            MagnumProgression magnumSpaceship = context.State.Get<MagnumProgression>();

            Plugin.Logger.Log("CleanupItemsMagnumDepartments");

            foreach (var department in magnumSpaceship.Departments)
            {
                switch (department._departmentId)
                {
                    case "autonomcapsule_department":
                        items.AddRange(CleanItemsInAutonomousCapsuleDepartment(department as AutonomousCapsuleDepartment));
                        break;
                    case "cargoshuttle_department":
                        items.AddRange(CleanItemsInShuttleCargoDepartment(department as ShuttleCargoDepartment));
                        break;
                    case "tradeshuttle_department":
                        items.AddRange(CleanItemsInTradeShuttleDepartment(department as TradeShuttleDepartment));
                        break;
                }
            }

            return items;
        }

        internal static List<string> CleanupItemsOnFloor(IModContext context)
        {
            ItemsOnFloor itemsOnFloor = context.State.Get<ItemsOnFloor>();
            List<string> items = new List<string>();

            if (itemsOnFloor != null)
            {
                foreach (var itemOnFloor in itemsOnFloor.Values)
                {
                    if (itemOnFloor.Storage != null && itemOnFloor.Storage.Items != null)
                    {
                        items.AddRange(CleanupPickupItem(itemOnFloor.Storage.Items));
                    }
                }
            }

            return items;
        }

        internal static List<string> CleanupMagnumCargo(IModContext context)
        {
            MagnumCargo magnumCargo = context.State.Get<MagnumCargo>();
            List<string> items = new List<string>();

            Plugin.Logger.Log("CleanupMercenariesCargo");

            if (magnumCargo != null)
            {
                foreach (var cargo in magnumCargo.ShipCargo)
                {
                    items.AddRange(CleanupPickupItem(cargo.Items));
                }
            }

            return items;
        }

        internal static void CleanupMagnumProjects(IModContext context, List<string> idsToKeep)
        {
            // Get the current game time (you can also use Time.time or Time.unscaledTime depending on your need)
            float currentTime = Time.time;

            if (currentTime - lastIntervalCheckTime >= interval)
            {
                // We need to quick cleanup magnum projects that are no longer in use by anything.
                // Mercs, mission rewards, station cargo.

                // LINQ maybe?

                MagnumProjects magnumProjects = context.State.Get<MagnumProjects>();
                Plugin.Logger.Log("CleanupMagnumProjects");

                if (magnumProjects != null)
                {
                    Plugin.Logger.Log($"magnumProjects != null");
                    foreach (var project in magnumProjects.Values.ToList()) // Use ToList() to avoid modification during iteration
                    {
                        var projectWrapper = MagnumProjectWrapper.SplitItemUid(MagnumProjectWrapper.GetPoqItemId(project));
                        Plugin.Logger.Log($"magnumProjects != null");

                        if (projectWrapper.PoqItem && !idsToKeep.Contains(projectWrapper.ReturnItemUid()))
                        {
                            Plugin.Logger.Log($"WARNING: Removing {project.FinishTime.Ticks} {project.DevelopId}  -- {projectWrapper.ReturnItemUid()}");

                            magnumProjects.Values.Remove(project); // Remove the item if it doesn't meet the condition
                        }
                    }
                }

                // Perform your time-based action here
                Plugin.Logger.Log("Time interval reached, doing something... CleanObsoleteProjects");

                // Reset the timer
                lastIntervalCheckTime = currentTime;
            }
        }

        internal static List<string> CleanupMercenariesCargo(IModContext context)
        {
            Mercenaries mercenaries = context.State.Get<Mercenaries>();
            List<string> items = new List<string>();

            Plugin.Logger.Log("CleanupMercenariesCargo");

            if (mercenaries != null)
            {
                Plugin.Logger.Log("mercenaries != null");

                foreach (var merc in mercenaries.Values)
                {
                    Plugin.Logger.Log($"merc {merc.ProfileId}");

                    foreach (ItemStorage storage in merc.CreatureData.Inventory.AllContainers)
                    {
                        //Plugin.Logger.Log($"storage {storage}");

                        items.AddRange(CleanupPickupItem(storage.Items));
                    }

                    // Part of all containers
                    //foreach (ItemStorage storage in merc.CreatureData.Inventory.Storages)
                    //{
                    //    items.AddRange(CleanupPickupItem(storage.Items));
                    //}

                    //foreach (ItemStorage storage in merc.CreatureData.Inventory.Slots)
                    //{
                    //    items.AddRange(CleanupPickupItem(storage.Items));
                    //}

                    foreach (ItemStorage storage in merc.CreatureData.Inventory.WeaponSlots)
                    {
                        items.AddRange(CleanupPickupItem(storage.Items));
                    }
                }
            }

            return items;
        }

        internal static List<string> CleanupMissionRewards(IModContext context)
        {
            Missions missions = context.State.Get<Missions>();
            List<string> items = new List<string>();

            Plugin.Logger.Log("CleanupMissionRewards");

            if (missions != null)
            {
                foreach (var misson in missions.Values)
                {
                    items.AddRange(CleanupPickupItem(misson.RewardItems));
                }
            }

            return items;
        }

        internal static List<string> CleanupPickupItem(List<BasePickupItem> basePickupItemsList)
        {
            //Plugin.Logger.Log($"CleanupPickupItem");

            List<string> itemsToReturn = new List<string>();

            foreach (PickupItem item in basePickupItemsList)
            {
                if (item.Id.Contains("_poq"))
                {
                    itemsToReturn.Add(item.Id);
                    Plugin.Logger.Log($"CleanupPickupItem: item.Id {item.Id}");

                    if (Plugin.Config.CleanupMode)
                    {
                        Plugin.Logger.Log($"\t\t Skipped");

                        CleanupItem(item);
                    }
                }
            }

            return itemsToReturn;
        }
    }
}