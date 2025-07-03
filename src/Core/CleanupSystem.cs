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

            if (department.CapsuleStorage != null && department.CapsuleStorage.Items != null)
            {
                items.AddRange(CleanupPickupItem(department.CapsuleStorage.Items));
            }

            return items;
        }

        internal static List<string> CleanItemsInShuttleCargoDepartment(ShuttleCargoDepartment department)
        {
            List<string> items = new List<string>();

            if (department.ShuttleCargo != null && department.ShuttleCargo.Items != null)
            {
                items.AddRange(CleanupPickupItem(department.ShuttleCargo.Items));
            }

            return items;
        }

        internal static List<string> CleanItemsInTradeShuttleDepartment(TradeShuttleDepartment department)
        {
            List<string> items = new List<string>();

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

            if (creatures != null)
            {
                foreach (var creature in creatures.Monsters)
                {
                    var creatureData = creature.CreatureData;
                    if (creatureData != null)
                    {
                        break;
                    }

                    foreach (ItemStorage storage in creatureData.Inventory.AllContainers)
                    {
                        items.AddRange(CleanupPickupItem(storage.Items));
                    }

                    foreach (ItemStorage storage in creatureData.Inventory.Storages)
                    {
                        items.AddRange(CleanupPickupItem(storage.Items));
                    }

                    foreach (ItemStorage storage in creatureData.Inventory.Slots)
                    {
                        items.AddRange(CleanupPickupItem(storage.Items));
                    }

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

            foreach (var department in magnumSpaceship.Departments)
            {
                switch (department._departmentId)
                {
                    case "autonomcapsule_department":
                        CleanItemsInAutonomousCapsuleDepartment(department as AutonomousCapsuleDepartment);
                        break;
                    case "cargoshuttle_department":
                        CleanItemsInShuttleCargoDepartment(department as ShuttleCargoDepartment);
                        break;
                    case "tradeshuttle_department":
                        CleanItemsInTradeShuttleDepartment(department as TradeShuttleDepartment);
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

                if (magnumProjects != null)
                {
                    foreach (var project in magnumProjects.Values.ToList()) // Use ToList() to avoid modification during iteration
                    {
                        var projectWrapper = MagnumProjectWrapper.SplitItemUid(MagnumProjectWrapper.GetPoqItemId(project));

                        if (projectWrapper.PoqItem && !idsToKeep.Contains(projectWrapper.ReturnItemUid()))
                        {
                            Plugin.Logger.Log($"Removing {project.FinishTime.Ticks} {project.DevelopId}  -- {projectWrapper.ReturnItemUid()}");

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

            if (mercenaries != null)
            {
                foreach (var merc in mercenaries.Values)
                {
                    foreach (ItemStorage storage in merc.CreatureData.Inventory.AllContainers)
                    {
                        items.AddRange(CleanupPickupItem(storage.Items));
                    }

                    foreach (ItemStorage storage in merc.CreatureData.Inventory.Storages)
                    {
                        items.AddRange(CleanupPickupItem(storage.Items));
                    }

                    foreach (ItemStorage storage in merc.CreatureData.Inventory.Slots)
                    {
                        items.AddRange(CleanupPickupItem(storage.Items));
                    }

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
            List<string> itemsToReturn = new List<string>();

            foreach (PickupItem item in basePickupItemsList)
            {
                itemsToReturn.Add(item.Id);

                if (Plugin.Config.CleanupMode)
                {
                    CleanupItem(item);
                }
            }

            return itemsToReturn;
        }
    }
}