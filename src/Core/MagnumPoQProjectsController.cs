using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Security.AccessControl;
using System.Security.Cryptography;
using MGSC;
using QM_PathOfQuasimorph.Core;
using UnityEngine;
using static QM_PathOfQuasimorph.Core.PathOfQuasimorph;
using static QM_PathOfQuasimorph.Core.MagnumPoQProjectsController;
using System.Security.Policy;

namespace QM_PathOfQuasimorph.Core
{
    internal partial class MagnumPoQProjectsController
    {
        public const long MAGNUM_PROJECT_START_TIME = 1337L;

        public MagnumProjects magnumProjects;
        public RaritySystem raritySystem = new RaritySystem();
        public ItemProduceReceipt itemProduceReceiptPlaceHolder = null;
        public Dictionary<string, List<string>> traitsTracker = new Dictionary<string, List<string>>();
        public MagnumPoQProjectsController(MagnumProjects magnumProjects)
        {
            this.magnumProjects = magnumProjects;
        }

        public string CreateMagnumProjectWithMods(MagnumProjectType projectType, string projectId)
        {
            // Check for some items that can't be easily added like augmentations that can be used as melee weapons.
            // NotImplementedException: Failed create project possesed_centaur_hand. No clone method for additional records: MGSC.AugmentationRecord.
            bool dropMethod = false;

            CompositeItemRecord compositeItemRecord = Data.Items.GetRecord(projectId, true) as CompositeItemRecord;

            foreach (var rec in compositeItemRecord.Records)
            {
                Type recordType = rec.GetType();

                switch (recordType.Name)
                {
                    case nameof(WeaponRecord):
                    case nameof(ArmorRecord):
                    case nameof(HelmetRecord):
                    case nameof(LeggingsRecord):
                    case nameof(BootsRecord):
                        break;
                    case nameof(AugmentationRecord):
                        dropMethod = true;
                        break;
                    default:
                        dropMethod = true;
                        break;

                }
            }

            if (dropMethod)
            {
                return projectId;
            }


            // Determine if we ever need to create a new project
            var itemRarity = raritySystem.SelectRarity();

            // We don't need to do anything.
            // That way we just return the project ID and it goes as defined by game design.
            if (itemRarity == ItemRarity.Standard)
            {
                return projectId;
            }

            // Create a new project
            MagnumProject newProject = new MagnumProject(projectType, projectId);

            // Generate a new UID
            var randomUid = Helpers.UniqueIDGenerator.GenerateRandomID();
            DigitInfo digits = DigitInfo.GetDigits(randomUid);
            digits.FillZeroes();
            digits.D6_Rarity = (int)itemRarity;
            var randomUidInjected = digits.ReturnUID();

            // New finish project time
            newProject.StartTime = DateTime.FromBinary(MAGNUM_PROJECT_START_TIME);

            newProject.FinishTime = DateTime.FromBinary(long.Parse(randomUidInjected)); // Convert uint64 to DateTime and this is our unique ID for item

            // Apply various project related parameters
            raritySystem.ApplyProjectParameters(ref newProject, itemRarity);

            // Resulting Uid
            var magnumProjectWrapper = new MagnumProjectWrapper(newProject);
            var newId = magnumProjectWrapper.ReturnItemUid();

            // Add our new Id to traits tracker as traits can't be added during project in game.
            // I'ts per item.
            // for: raritySystem.ApplyTraits
            traitsTracker.Add(newId, new List<string>());

            //PathOfQuasimorph.InjectItemRecord(newProject);
            MagnumDevelopmentSystem.InjectItemRecord(newProject);

            // Add the project to the list
            magnumProjects.Values.Add(newProject);

            Plugin.Logger.Log($"\t\t Created new project for {newProject.DevelopId}");
            return newId;
        }

        internal MagnumProject GetProjectById(string itemId)
        {
            if (itemId.Contains("_poq_"))
            {
                var wrapped = MagnumProjectWrapper.SplitItemUid(itemId);

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
                foreach (MagnumProject magnumProject in magnumProjects.Values)
                {
                    if (magnumProject.DevelopId == itemId)
                    {
                        return magnumProject;
                    }
                }
            }

            return null; // Return null if no project is found
        }

        public static ItemRarity GetItemRarity(long finishTime)
        {
            DigitInfo digits = DigitInfo.GetDigits(finishTime);
            return (ItemRarity)digits.D6_Rarity;
        }

        // Yes I dont know to get it right in IL opcodes.
        public static ItemTransformationRecord GetItemTransformationRecord(ItemTransformationRecord record, MagnumProject project)
        {
            if (record == null || record.Id == string.Empty)
            {
                // Item breaks into this, unless it has it's own record.
                return Data.ItemTransformation.GetRecord("prison_tshirt_1", true);
            }

            return record;
        }

        public ItemProduceReceipt GetPlaceHolderItemProduceReceipt()
        {
            // If we already found it, reuse it as iteration and calls are very intensive for the hook.
            if (itemProduceReceiptPlaceHolder != null && itemProduceReceiptPlaceHolder.Id != string.Empty)
            {
                return itemProduceReceiptPlaceHolder;
            }

            // Iterate whole receipts do find our placeholder.
            for (int i = 0; i < Data.ProduceReceipts.Count; i++)
            {
                // Item has no recipe, let's add placeholder as we won't see it in recipe's list anyway.
                if (Data.ProduceReceipts[i].OutputItem == "pills_sorbent")
                {
                    itemProduceReceiptPlaceHolder = Data.ProduceReceipts[i];
                    return itemProduceReceiptPlaceHolder;
                }
            }

            return itemProduceReceiptPlaceHolder;
        }
    }
}
