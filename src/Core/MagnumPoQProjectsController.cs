using MGSC;
using Newtonsoft.Json;
using QM_PathOfQuasimorph.Core;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Policy;
using UnityEngine;

namespace QM_PathOfQuasimorph.Core
{
    internal partial class MagnumPoQProjectsController
    {
        public const long MAGNUM_PROJECT_START_TIME = 1337L;

        public static MagnumProjects magnumProjects;
        public List<string> traitsTracker = new List<string>();
        private Logger _logger = new Logger(null, typeof(MagnumPoQProjectsController));
        internal MagnumProject dataPlaceholderProject;

        public MagnumPoQProjectsController(MagnumProjects _magnumProjects)
        {
            magnumProjects = _magnumProjects;
            //AffixManager.LoadLocalizationData();
        }

        public void AddItemRecords(JsonSerializerSettings jsonSettings)
        {
            _logger.Log($"AddItemRecords");

            CreateDataHolderProject();

            _logger.Log($"dataPlaceholderProject.UpcomingModifications.Count {dataPlaceholderProject.UpcomingModifications.Count}");

            // We store 3 strings
            // ElementAt is slow but for three entries it's ok.
            // var entry0 = dataPlaceholderProject.UpcomingModifications.ElementAt(0);
            //var entry1 = dataPlaceholderProject.UpcomingModifications.ElementAt(1);
            //var entry2 = dataPlaceholderProject.UpcomingModifications.ElementAt(2);

            RecordCollection.DeserializeCollection(dataPlaceholderProject);

            //Data.Items.AddRecord(keyValuePair.Key, deserializedData);
        }

        public void CreateDataHolderProject()
        {
            // Find and create if required our dataPlaceholderProject

            if (magnumProjects == null)
            {
                throw new Exception("Magnum project instance missing");
            }

            if (dataPlaceholderProject != null)
            {
                return;
            }

            foreach (var project in magnumProjects.Values)
            {
                _logger.Log($"CreateDataHolderProject: checking project {project.DevelopId} {project.FinishTime}");

                if (MetadataWrapper.IsPoqProject(project) && MetadataWrapper.IsSerializedStorage(project.FinishTime.Ticks))
                {
                    _logger.Log($"CreateDataHolderProject: IsSerializedStorage");

                    dataPlaceholderProject = project;
                    return;
                }
            }

            _logger.Log($"creating new project");

            MagnumProject newProject = new MagnumProject(MagnumProjectType.MeleeWeapon, "common_knife_1");
            var randomUid = Helpers.UniqueIDGenerator.GenerateRandomIDWith16Characters();
            DigitInfo digits = DigitInfo.GetDigits(randomUid);
            digits.FillZeroes();
            digits.Rarity = (int)ItemRarity.Standard;
            // boostedParamIndex, randomPrefix
            digits.BoostedParam = 99;
            digits.IsSerialized = true;
            var randomUidInjected = digits.ReturnUID();
            newProject.StartTime = DateTime.FromBinary(MAGNUM_PROJECT_START_TIME);
            newProject.FinishTime = DateTime.FromBinary(long.Parse(randomUidInjected));
            newProject.ModificationsCount = 3;

            _logger.Log($"randomUidInjected {randomUidInjected}");
            _logger.Log($"newProject.StartTime {newProject.StartTime}");
            _logger.Log($"newProject.FinishTime {newProject.FinishTime}");
            _logger.Log($"IsSerializedStorage {MetadataWrapper.IsSerializedStorage(newProject.FinishTime.Ticks)}");

            //MagnumDevelopmentSystem.InjectItemRecord(newProject);
            magnumProjects.Values.Add(newProject);

            dataPlaceholderProject = newProject;
        }

        [Obsolete]
        public bool CanProcessItemRecord(string id)
        {
            bool canProcess = true;

            // Blacklist some items
            List<string> blacklistedCategories = new List<string>
                {
                    "Possessed",
                    "CyberAug",
                    "PossessedAug",
                    "QuasiAug",
                    "none"
                };

            CompositeItemRecord compositeItemRecord = Data.Items.GetRecord(id, true) as CompositeItemRecord;

            foreach (var rec in compositeItemRecord.Records)
            {
                Type recordType = rec.GetType();
                bool checkWeaponRecord = false;

                switch (recordType.Name)
                {
                    case nameof(WeaponRecord):
                        checkWeaponRecord = true;
                        break;
                    case nameof(ArmorRecord):
                    case nameof(HelmetRecord):
                    case nameof(LeggingsRecord):
                    case nameof(BootsRecord):
                        break;
                    case nameof(AugmentationRecord):
                        canProcess = false;
                        break;
                    default:
                        canProcess = false;
                        break;
                }

                if (checkWeaponRecord)
                {
                    var weaponRecord = rec as WeaponRecord;
                    if (weaponRecord != null)
                    {
                        //_logger.Log($"\t\t\t IsImplicit {weaponRecord.IsImplicit}");
                        if (weaponRecord.IsImplicit)
                        {
                            canProcess = false;
                            break;
                        }

                        foreach (var mod in weaponRecord.Categories)
                        {
                            if (blacklistedCategories.Contains(mod))
                            {
                                canProcess = false;
                                break;
                            }

                            //_logger.Log($"\t\t\t Category  {mod}");
                        }

                        //_logger.Log($"\t\t\t ItemClass {weaponRecord.ItemClass}");
                        //_logger.Log($"\t\t\t WeaponClass {weaponRecord.WeaponClass}");
                    }
                }
            }

            return canProcess;
        }

        [Obsolete]
        public string CreateMagnumProjectWithMods(MagnumProjectType projectType, string projectId, bool rarityExtraBoost)
        {
            // Check for some items that can't be easily added like augmentations that can be used as melee weapons.
            // NotImplementedException: Failed create project possesed_centaur_hand. No clone method for additional records: MGSC.AugmentationRecord.

            if (!CanProcessItemRecord(projectId))
            {
                return projectId;
            }

            _logger.Log($"\t No project found with DevelopId: {projectId}");
            _logger.Log($"Creating a new project with mods for {projectId} - {projectType}");

            // Determine if we ever need to create a new project
            var itemRarity = PathOfQuasimorph.raritySystem.SelectRarity();

            // We don't need to do anything.
            // That way we just return the project ID and it goes as defined by game design.
            if (itemRarity == ItemRarity.Standard)
            {
                return projectId;
            }

            // Create a new project
            MagnumProject newProject = new MagnumProject(projectType, projectId);

            // Apply various project related parameters
            var boostedParamIndex = PathOfQuasimorph.raritySystem.ApplyProjectParameters(ref newProject, itemRarity, rarityExtraBoost);

            // Generate a new UID
            var randomUid = Helpers.UniqueIDGenerator.GenerateRandomIDWith16Characters();
            DigitInfo digits = DigitInfo.GetDigits(randomUid);
            digits.FillZeroes();
            digits.Rarity = (int)itemRarity;
            // boostedParamIndex, randomPrefix
            digits.BoostedParam = boostedParamIndex;
            var randomUidInjected = digits.ReturnUID();

            // New finish project time
            newProject.StartTime = DateTime.FromBinary(MAGNUM_PROJECT_START_TIME);
            newProject.FinishTime = DateTime.FromBinary(long.Parse(randomUidInjected)); // Convert uint64 to DateTime and this is our unique ID for item

            // Resulting Uid
            var magnumProjectWrapper = new MetadataWrapper(newProject);
            var newId = magnumProjectWrapper.ReturnItemUid();

            // Add our new Id to traits tracker as traits can't be added during project in game.
            // I'ts per item.
            // for: raritySystem.ApplyTraits
            traitsTracker.Add(newId);

            //PathOfQuasimorph.InjectItemRecord(newProject);
            MagnumDevelopmentSystem.InjectItemRecord(newProject);

            // Add the project to the list
            magnumProjects.Values.Add(newProject);
            RaritySystem.AddAffixes(newProject);

            _logger.Log($"\t\t Created new project for {newProject.DevelopId} with itemId: {newId}");
            return newId;
        }

        [Obsolete]
        internal static MagnumProject GetProjectById(string itemId)
        {
            if (itemId.Contains("_poq_"))
            {
                var wrapped = MetadataWrapper.SplitItemUid(itemId);

                foreach (MagnumProject magnumProject in magnumProjects.Values)
                {
                    if (magnumProject.FinishTime == wrapped.FinishTime)
                    {
                        return magnumProject;
                    }
                }
            }
            else
            {
                // When you start new game magnum project are not yet available.
                if (magnumProjects != null)
                {
                    foreach (MagnumProject magnumProject in magnumProjects.Values)
                    {
                        if (magnumProject.DevelopId == itemId)
                        {
                            return magnumProject;
                        }
                    }
                }
            }

            return null; // Return null if no project is found
        }

        [Obsolete]
        public static ItemRarity GetItemRarity(long finishTime)
        {
            DigitInfo digits = DigitInfo.GetDigits(finishTime);
            return (ItemRarity)digits.Rarity;
        }

        // Yes I dont know to get it right in IL opcodes.
        [Obsolete]
        public static ItemTransformationRecord GetItemTransformationRecord(ItemTransformationRecord record, MagnumProject project)
        {
            if (record == null || record.Id == string.Empty)
            {
                Plugin.Logger.Log($"GetItemTransformationRecord adding placeholder");

                // Item breaks into this, unless it has it's own record.
                return Data.ItemTransformation.GetRecord("prison_tshirt_1", true);
            }

            // Since this method used during InjectItemRecord, we can safely extra update our language keys.
            // I don't like making classes static but this won't hurt.
            RaritySystem.AddAffixes(project);

            return record;
        }
    }
}
