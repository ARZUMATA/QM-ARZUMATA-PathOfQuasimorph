using MGSC;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngineInternal;
using static QM_PathOfQuasimorph.Core.MagnumPoQProjectsController;

namespace QM_PathOfQuasimorph.Core
{
    internal static class CleanupSystem
    {
        // Static variable to track elapsed time
        private static float lastIntervalCheckTime = 0f;
        private static readonly float interval = 60f; // e.g., every 5 seconds
        private static Logger _logger = new Logger(null, typeof(CleanupSystem));

        internal static List<string> CleanItemsInAutonomousCapsuleDepartment(AutonomousCapsuleDepartment department)
        {
            List<string> items = new List<string>();

            _logger.Log("CleanItemsInAutonomousCapsuleDepartment");

            if (department.CapsuleStorage != null && department.CapsuleStorage.Items != null)
            {
                items.AddRange(CleanupPickupItem(department.CapsuleStorage.Items));
            }

            return items;
        }

        internal static List<string> CleanItemsInShuttleCargoDepartment(ShuttleCargoDepartment department)
        {
            List<string> items = new List<string>();

            _logger.Log("CleanItemsInShuttleCargoDepartment");

            if (department.ShuttleCargo != null && department.ShuttleCargo.Items != null)
            {
                items.AddRange(CleanupPickupItem(department.ShuttleCargo.Items));
            }

            return items;
        }

        internal static List<string> CleanItemsInTradeShuttleDepartment(TradeShuttleDepartment department)
        {
            List<string> items = new List<string>();

            _logger.Log("CleanItemsInTradeShuttleDepartment");

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

        internal static void CleanObsoleteProjects(IModContext context, bool cleanProjects = false, bool force = false)
        {
            _logger.Log($"CleanObsoleteProjects");
            List<string> idsToKeep = new List<string>();

            var listMagnumCargo = CleanupMagnumCargo(context);
            var listMissionRewards = CleanupMissionRewards(context);
            var listStationItems = CleanStationInternalStorage(context);
            var listMercenariesCargo = CleanupMercenariesCargo(context);
            var listCreatureData = CleanupCreatureData(context);
            var listMagnumDepartmentsData = CleanupItemsMagnumDepartments(context);
            var listMapObstacles = CleanupItemsMapObstacles(context);
            var listItemsOnFloor = CleanupItemsItemsOnFloor(context);

            _logger.Log($"Lists count:");
            _logger.Log($"\t listMagnumCargo {listMagnumCargo.Count}");
            _logger.Log($"\t listMissionRewards {listMissionRewards.Count}");
            _logger.Log($"\t listStationItems {listStationItems.Count}");
            _logger.Log($"\t listMercenariesCargo {listMercenariesCargo.Count}");
            _logger.Log($"\t listCreatureData {listCreatureData.Count}");
            _logger.Log($"\t listMagnumDepartmentsData {listMagnumDepartmentsData.Count}");
            _logger.Log($"\t listMapObstacles {listMapObstacles.Count}");
            _logger.Log($"\t listItemsOnFloor {listMapObstacles.Count}");

            // Dedupe? There are not many entries anyway.
            idsToKeep.AddRange(listMagnumCargo);
            idsToKeep.AddRange(listMissionRewards);
            idsToKeep.AddRange(listMercenariesCargo);
            idsToKeep.AddRange(listCreatureData);
            idsToKeep.AddRange(listMagnumDepartmentsData);
            idsToKeep.AddRange(listStationItems);
            idsToKeep.AddRange(listMapObstacles);
            idsToKeep.AddRange(listItemsOnFloor);

            _logger.Log($"\t idsToKeep {idsToKeep.Count}");

            // Cleanup magnum projects.
            if (cleanProjects)
            {
                CleanupMagnumProjects(context, idsToKeep, force);
            }
        }

        private static List<string> CleanupItemsItemsOnFloor(IModContext context)
        {
            ItemsOnFloor itemsOnFloor = context.State.Get<ItemsOnFloor>();
            List<string> items = new List<string>();

            _logger.Log("CleanStationInternalStorage");

            if (itemsOnFloor != null)
            {
                foreach (var value in itemsOnFloor.Values)
                {
                    if (value.Storage != null)
                    {
                        items.AddRange(CleanupPickupItem(value.Storage.Items));
                    }

                }
            }

            return items;
        }

        private static List<string> CleanupItemsMapObstacles(IModContext context)
        {
            MapObstacles obstacles = context.State.Get<MapObstacles>();
            List<string> items = new List<string>();

            _logger.Log("CleanupItemsMapObstacles");

            if (obstacles != null)
            {
                foreach (var obstacle in obstacles.Obstacles)
                {
                    foreach (var comp in obstacle._comps)
                    {
                        var corpseStorage = comp as CorpseStorage;
                        if (corpseStorage != null && corpseStorage._creatureData != null && corpseStorage._creatureData.Inventory != null)
                        {
                            foreach (ItemStorage storage in corpseStorage._creatureData.Inventory.AllContainers)
                            {
                                items.AddRange(CleanupPickupItem(storage.Items));
                            }

                            var store = comp as Store;

                            if (store != null && store.storage != null && store.storage.Items != null)
                            {
                                items.AddRange(CleanupPickupItem(store.storage.Items));
                            }
                        }
                    }
                }
            }

            return items;
        }

        internal static List<string> CleanupCreatureData(IModContext context)
        {
            Creatures creatures = context.State.Get<Creatures>();
            List<string> items = new List<string>();

            _logger.Log("CleanupCreatureData");

            if (creatures != null)
            {
                _logger.Log($"player creature {creatures.Player.CreatureData.UniqueId}");

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

                    _logger.Log($"monster creature {creatureData.UniqueId}");

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
            if (RecordCollection.MetadataWrapperRecords.TryGetValue(item.Id, out MetadataWrapper wrapper))
            {
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
            else
            {
                if (MetadataWrapper.IsPoqItemUid(item.Id))
                {
                    throw new Exception($"CleanupItem: trying to cleanup poq item but record is missing.");
                }
            }
        }

        internal static List<string> CleanupItemsMagnumDepartments(IModContext context)
        {
            List<string> items = new List<string>();

            MagnumProgression magnumSpaceship = context.State.Get<MagnumProgression>();

            _logger.Log("CleanupItemsMagnumDepartments");

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

        internal static List<string> CleanStationInternalStorage(IModContext context)
        {
            Stations stations = context.State.Get<Stations>();
            List<string> items = new List<string>();

            _logger.Log("CleanStationInternalStorage");

            if (stations != null)
            {
                foreach (var station in stations.Values)
                {
                    if (station.InternalStorage != null)
                    {
                        items.AddRange(CleanupPickupItem(station.InternalStorage.Items));
                    }

                }
            }

            return items;
        }

        internal static List<string> CleanupItemsOnFloor(IModContext context)
        {
            ItemsOnFloor itemsOnFloor = context.State.Get<ItemsOnFloor>();
            List<string> items = new List<string>();

            _logger.Log("CleanupItemsOnFloor");

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

            _logger.Log("CleanupMercenariesCargo");

            if (magnumCargo != null)
            {
                foreach (var cargo in magnumCargo.ShipCargo)
                {
                    items.AddRange(CleanupPickupItem(cargo.Items));
                }
            }

            return items;
        }

        internal static void CleanupMagnumProjects(IModContext context, List<string> idsToKeep, bool force = false)
        {
            // Get the current game time (you can also use Time.time or Time.unscaledTime depending on your need)
            float currentTime = Time.time;

            if (currentTime - lastIntervalCheckTime >= interval || force)
            {
                // We need to quick cleanup magnum projects that are no longer in use by anything.
                // Mercs, mission rewards, station cargo.

                // LINQ maybe?

                MagnumProjects magnumProjects = context.State.Get<MagnumProjects>();

                _logger.Log("CleanupMagnumProjects");

                if (magnumProjects != null)
                {
                    _logger.Log($"magnumProjects != null");
                    foreach (var project in magnumProjects.Values.ToList()) // Using ToList() to avoid modification during iteration
                    {
                        _logger.Log($"\t checking project {project.DevelopId}");

                        var itemId = MetadataWrapper.GetPoqItemIdFromProject(project);

                        if (!RecordCollection.MetadataWrapperRecords.TryGetValue(itemId, out MetadataWrapper wrapper))
                        {
                            if (MetadataWrapper.IsPoqItemUid(itemId) && !MetadataWrapper.IsSerializedStorage(itemId))
                            {
                                // Handle our dataholder project
                                throw new Exception($"CleanupMagnumProjects: trying to cleanup poq item but record is missing.");
                            }
                        }
                        else
                        {
                            // Here we assume record exist in records collection already so we remove any project that is POQ but not dataholder.

                            _logger.Log($"PoqItem {wrapper.PoqItem}, SerializedStorage {wrapper.SerializedStorage}");

                            if (!wrapper.SerializedStorage && wrapper.PoqItem)
                            {
                                _logger.Log($"WARNING: Removing {project.FinishTime.Ticks} {project.DevelopId}  -- {wrapper.ReturnItemUid()}");

                                magnumProjects.Values.Remove(project); // Remove the item if it doesn't meet the condition
                            }
                        }

                    }
                }

                _logger.Log("Time interval reached, doing something... CleanObsoleteProjects");

                // Reset the timer
                lastIntervalCheckTime = currentTime;

                RecordCollection.CleanObsoleteItemRecords(idsToKeep);
            }
        }

        internal static List<string> CleanupMercenariesCargo(IModContext context)
        {
            Mercenaries mercenaries = context.State.Get<Mercenaries>();
            List<string> items = new List<string>();

            _logger.Log("CleanupMercenariesCargo");

            if (mercenaries != null)
            {
                _logger.Log("mercenaries != null");

                foreach (var merc in mercenaries.Values)
                {
                    _logger.Log($"merc {merc.ProfileId}");

                    foreach (ItemStorage storage in merc.CreatureData.Inventory.AllContainers)
                    {
                        //_logger.Log($"storage {storage}");

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

            _logger.Log("CleanupMissionRewards");

            if (missions != null)
            {
                foreach (var misson in missions.Values)
                {
                    items.AddRange(CleanupPickupItem(misson.RewardItems));
                    items.AddRange(CleanupPickupItem(misson.RewardItemsExample));
                }

                foreach (var misson in missions.Reversed)
                {
                    items.AddRange(CleanupPickupItem(misson.RewardItems));
                    items.AddRange(CleanupPickupItem(misson.RewardItemsExample));
                }
            }

            return items;
        }

        internal static List<string> CleanupPickupItem(List<BasePickupItem> basePickupItemsList)
        {
            //_logger.Log($"CleanupPickupItem");

            List<string> itemsToReturn = new List<string>();

            foreach (PickupItem item in basePickupItemsList)
            {
                if (item.Id.Contains("_poq"))
                {
                    itemsToReturn.Add(item.Id);
                    _logger.Log($"CleanupPickupItem: item.Id {item.Id}");

                    if (Plugin.Config.CleanupMode)
                    {
                        _logger.Log($"\t\t Skipped");

                        CleanupItem(item);
                    }
                }
            }

            return itemsToReturn;
        }
    }
}