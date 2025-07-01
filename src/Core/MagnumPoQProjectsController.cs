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

        public class MagnumProjectWrapper
        {
            public string Id { get; set; }
            public string CustomId { get; set; }
            public string Rarity { get; set; }
            public ItemRarity RarityClass { get; set; }
            public DateTime StartTime { get; set; }
            public DateTime FinishTime { get; set; }
            public bool PoqItem { get; set; }

            public MagnumProjectWrapper(MagnumProject newProject)
            {

                // Generate metadata
                this.Id = newProject.DevelopId;

                // This is our project based on time.
                if (newProject.StartTime.Ticks == MAGNUM_PROJECT_START_TIME && newProject.FinishTime.Ticks > 0)
                {
                    PoqItem = true;
                }
                else
                {
                    PoqItem = false;
                }

                Plugin.Logger.Log($" MagnumProjectWrapper PoqItem {PoqItem}");

                if (PoqItem)
                {
                    CustomId = $"{Id}_custom_poq";
                    var digitinfo = DigitInfo.GetDigits(newProject.FinishTime.Ticks);
                    RarityClass = (ItemRarity)digitinfo.D6_Rarity;
                    Rarity = RarityClass.ToString().ToLower();
                    StartTime = newProject.StartTime;
                    FinishTime = newProject.FinishTime;

                }
                else
                {
                    CustomId = $"{Id}_custom";
                    RarityClass = ItemRarity.Standard;
                    Rarity = RarityClass.ToString().ToLower();
                    StartTime = newProject.StartTime;
                    FinishTime = newProject.FinishTime;
                }
            }

            public MagnumProjectWrapper()
            {

            }

            public MagnumProjectWrapper(string id, bool poqItem, DateTime startTime, DateTime finishTime)
            {
                this.Id = id;

                if (poqItem)
                {
                    this.CustomId = $"${id}_custom_poq";
                }
                else
                {
                    this.CustomId = $"{id}_custom";
                }

                var digitinfo = DigitInfo.GetDigits(finishTime.Ticks);
                this.RarityClass = (ItemRarity)digitinfo.D6_Rarity;
                this.Rarity = RarityClass.ToString().ToLower();
                this.StartTime = startTime;
                this.FinishTime = finishTime;
                this.PoqItem = poqItem;

                Plugin.Logger.Log($"Created MagnumProjectWrapper with id: {this.Id}, CustomId: {this.CustomId}, Rarity: {this.Rarity}, RarityClass: {this.RarityClass}, StartTime: {this.StartTime.Ticks}, FinishTime: {this.FinishTime.Ticks}, PoqItem: {this.PoqItem}");
            }

            public string ReturnItemUid()
            {
                if (PoqItem)
                {
                    return $"{this.CustomId}_{this.Rarity}_{this.StartTime.Ticks.ToString()}_{this.FinishTime.Ticks.ToString()}";
            }
            else
            {
                    return $"{this.CustomId}";
                }
            }

            public static string GetPoqItemId(MagnumProject newProject)
            {
                // Check our project, detect if it has metadata we injected.
                return new MagnumProjectWrapper(newProject).ReturnItemUid();
        }

        public static MagnumProjectWrapper SplitItemUid(string uid)
        {
                // trucker_pistol_1_custom_poq_quantum_1337_808576342000005

            // This is used for dynamic item creation during CreateForInventory
            if (uid.Contains("_poq_"))
            {
                    var splittedUid = uid.Split(new string[] { "_poq_" }, StringSplitOptions.None);
                    /* Two parts:
                     * First:
                     * trucker_pistol_1_custom
                     * _poq_
                     * Second:
                     * quantum_1337_808576342000005
                     * */

                    var realId = splittedUid[0].Replace("_custom", string.Empty); // Real Base item ID
                    var suffixParts = splittedUid[1].Split('_'); // T_T

                    /* 
                    * quantum
                    * 1337
                    * 808576342000005
                    */

                    //  string id, bool poqItem, ItemRarity rarityClass, DateTime startTime, DateTime finishTime)
                    var wrapper = new MagnumProjectWrapper(
                        id: realId,
                        poqItem: true,
                        startTime: new DateTime(Int64.Parse(suffixParts[1])),
                        finishTime: new DateTime(Int64.Parse(suffixParts[2]))
                        );

                    return wrapper;
            }

            var realBaseId2 = uid.Replace("_custom", string.Empty); // Real Base item ID
            var customId2 = realBaseId2 + "_custom"; // Custom ID

            return new MagnumProjectWrapper
            {
                    Id = realBaseId2,
                    CustomId = customId2,
                    Rarity = "Standard",
                    RarityClass = ItemRarity.Standard,
                    StartTime = DateTime.MinValue,
                    FinishTime = DateTime.MinValue,
                PoqItem = false
            };
        }
        }

        public class DigitInfo
        {
            public string LeftPart { get; set; }
            public int D1 { get; set; }
            public int D2 { get; set; }
            public int D3 { get; set; }
            public int D4 { get; set; }
            public int D5 { get; set; }
            public int D6_Rarity { get; set; }
            public string UID { get; set; }

            public DigitInfo(string leftPart, int d1, int d2, int d3, int d4, int d5, int d6)
            {
                // Use last 6 digits of as identifier
                LeftPart = leftPart;
                D1 = d1; // 0
                D2 = d2; // 0
                D3 = d3; // 0
                D4 = d4; // 0
                D5 = d5; // 0
                D6_Rarity = d6; // ItemRarity
            }

            public void FillZeroes()
            {
                D1 = 0;
                D2 = 0;
                D3 = 0;
                D4 = 0;
                D5 = 0;
                D6_Rarity = 0;
            }

            public string ReturnUID()
            {
                // Rebuild the six digits as a string
                string modifiedSixDigits = $"{D1}{D2}{D3}{D4}{D5}{D6_Rarity}";

                // Reconstruct the full UID using the left part and the modified six digits
                string modifiedUidStr = LeftPart + modifiedSixDigits;

                return modifiedUidStr;
            }

            public static DigitInfo GetDigits(long uid)
            {
                // Convert to string
                string uidStr = uid.ToString();

                // Extract the left part (all digits except the last six)
                string leftPart = uidStr.Substring(0, uidStr.Length - 6);

                // Extract last six digits as a substring
                string lastSixDigits = uidStr.Substring(uidStr.Length - 6);

                // Parse each digit into integers
                int d1 = int.Parse(lastSixDigits[0].ToString());
                int d2 = int.Parse(lastSixDigits[1].ToString());
                int d3 = int.Parse(lastSixDigits[2].ToString());
                int d4 = int.Parse(lastSixDigits[3].ToString());
                int d5 = int.Parse(lastSixDigits[4].ToString());
                int d6 = int.Parse(lastSixDigits[5].ToString());

                return new DigitInfo(leftPart, d1, d2, d3, d4, d5, d6);
            }
        }
    }
}
