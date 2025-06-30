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
        public MagnumProjects magnumProjects;
        public RaritySystem raritySystem = new RaritySystem();
        public ItemProduceReceipt itemProduceReceiptPlaceHolder = null;

        public MagnumPoQProjectsController(MagnumProjects magnumProjects)
        {
            this.magnumProjects = magnumProjects;
        }

        public string CreateMagnumProjectWithMods(MagnumProjectType projectType, string projectId)
        {
            // Determine if we ever need to create a new project
            var itemRarity = raritySystem.SelectRarity();

            // We don't need to do anything.
            // That way we just return the project ID and it goes as defined by game design.
            if (itemRarity == ItemRarity.Standard)
            {
                return projectId;
            }

            var rarityString = itemRarity.ToString().ToLower();

            // Create a new project
            MagnumProject newProject = new MagnumProject(projectType, projectId);

            // Generate a new UID
            var randomUid = Helpers.UniqueIDGenerator.GenerateRandomID();
            DigitInfo digits = DigitInfo.GetDigits(randomUid);
            digits.FillZeroes();
            digits.D6_Rarity = (int)itemRarity;
            var randomUidInjected = digits.ReturnUID();

            // New finish project time
            var finishTimeTemp = DateTime.FromBinary(long.Parse(randomUidInjected)); // Convert uint64 to DateTime and this is our unique ID for item
            newProject.StartTime = DateTime.MinValue;
            newProject.FinishTime = finishTimeTemp;

            // Apply various project related parameters
            raritySystem.ApplyProjectParameters(ref newProject, itemRarity);

            // Resulting Uid
            var newId = $"{projectId}_custom_poq_{rarityString}_{randomUidInjected}";

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
                var wrapped = SplitItemUid(itemId);

                foreach (MagnumProject magnumProject in magnumProjects.Values)
                {
                    if (magnumProject.FinishTime == wrapped.finishTime)
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
            if (record == null)
            {
                Data.ItemTransformation.GetRecord("broken_weapon", true);
            }

            return record;
        }

        public ItemProduceReceipt GetPlaceHolderItemProduceReceipt()
        {
            // If we already found it, reuse it as iteration and calls are very intensive for the hook.
            if (itemProduceReceiptPlaceHolder != null)
            {
                return itemProduceReceiptPlaceHolder;
            }

            // Safe in case we don't find in cycle.
            ItemProduceReceipt itemProduceReceiptPlaceHolderTemp = Data.ProduceReceipts[0];

            // Iterate whole receipts do find our placeholder.
            for (int i = 0; i < Data.ProduceReceipts.Count; i++)
            {
                if (Data.ProduceReceipts[i].OutputItem == "pills_sorbent")
                {
                    itemProduceReceiptPlaceHolder = Data.ProduceReceipts[i];
                    break;
                }
            }

            // Reuse new found one for our placeholder.
            if (itemProduceReceiptPlaceHolder == null)
            {
                itemProduceReceiptPlaceHolder = itemProduceReceiptPlaceHolderTemp;
            }

            return itemProduceReceiptPlaceHolder;
        }

        public static string GetPoqItemId(MagnumProject newProject)
        {
            // Get POQ item ID if we can, else return as is.
            var splittedUid = SplitItemUid(newProject.DevelopId);
            Plugin.Logger.Log($"newProject {newProject.DevelopId}");
            Plugin.Logger.Log($"StartTime {newProject.StartTime.Ticks}");
            Plugin.Logger.Log($"FinishTime {newProject.FinishTime.Ticks}");

            // So either have 'broken' newProject.DevelopId during item creation passed and get PoqItem from it.
            if (splittedUid.PoqItem)// || newProject.StartTime == DateTime.MinValue)
            {
                Plugin.Logger.Log($"poq1");
                return $"{splittedUid.id}_custom_poq_{splittedUid.rarity}_{splittedUid.finishTime.Ticks.ToString()}";

            }
            // Or we have saved existing project with standard DevelopId so we need to check using project start time.
            else if (newProject.StartTime.Ticks == 0 && newProject.FinishTime.Ticks > 0)
            {
                Plugin.Logger.Log($"poq2");

                var rarity = GetItemRarity(newProject.FinishTime.Ticks).ToString().ToLower();
                // We got another existing 'poq' item i.e. during gameload.
                return $"{splittedUid.id}_custom_poq_{rarity}_{newProject.FinishTime.Ticks.ToString()}";
            }
            else
            {
                Plugin.Logger.Log($"no poq");

                return splittedUid.customId; // returns customId i.e. common_shirt_2_custom
            }
        }

        public static MagnumProjectWrapper SplitItemUid(string uid)
        {
            // This is used for dynamic item creation during CreateForInventory
            if (uid.Contains("_poq_"))
            {
                var newResult = uid.Split(new string[] { "_poq_" }, StringSplitOptions.None);

                var realBaseId = newResult[0].Replace("_custom", string.Empty); // Real Base item ID
                var customId = realBaseId + "_custom"; // Custom ID

                var suffixParts = newResult[1]
                    .Split(new string[] { "_" }, 2, StringSplitOptions.None);
                string rarityName = suffixParts[0]; // e.g., "Prototype"
                long hash = Int64.Parse(suffixParts.Length > 1 ? suffixParts[1] : "0"); // "1234567890"

                ItemRarity rarityClass = (ItemRarity)Enum.Parse(typeof(ItemRarity), rarityName, true);

                return new MagnumProjectWrapper
                {
                    id = realBaseId,
                    customId = customId,
                    rarity = rarityName,
                    rarityClass = rarityClass,
                    finishTime = DateTime.FromBinary((long)hash),
                    uid = hash,
                    fullstring = uid,
                    PoqItem = true
                };
            }

            var realBaseId2 = uid.Replace("_custom", string.Empty); // Real Base item ID
            var customId2 = realBaseId2 + "_custom"; // Custom ID

            return new MagnumProjectWrapper
            {
                id = realBaseId2,
                customId = customId2,
                rarity = "Standard",
                rarityClass = ItemRarity.Standard,
                finishTime = DateTime.MinValue,
                uid = 0,
                fullstring = uid,
                PoqItem = false
            };
        }

        public class MagnumProjectWrapper
        {
            public string id { get; set; }
            public string customId { get; set; }
            public string rarity { get; set; }
            public ItemRarity rarityClass { get; set; }
            public long uid { get; set; }
            public DateTime finishTime { get; set; }
            public string fullstring { get; set; }
            public bool PoqItem { get; set; }
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
