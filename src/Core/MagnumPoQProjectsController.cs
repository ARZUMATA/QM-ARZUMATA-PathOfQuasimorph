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

namespace QM_PathOfQuasimorph.Core
{
    internal partial class MagnumPoQProjectsController
    {
        public MagnumProjects magnumProjects;
        public RaritySystem raritySystem = new RaritySystem();
        
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
            magnumProjects.Values.Add(newProject);

            // Apply various project related parameters
            raritySystem.ApplyProjectParameters(ref newProject, itemRarity);

            // Resulting Uid
            var newId = $"{projectId}_custom_poq_{rarityString}_{randomUidInjected}";

            PathOfQuasimorph.InjectItemRecord(newProject);
            Plugin.Logger.Log($"\t\t Created new project for {newProject.DevelopId}");
            return newId;
        }

        internal MagnumProject GetProjectById(string itemId)
        {
            if (itemId.Contains("_poq_"))
            {
                var wrapped = SplitItemId(itemId);

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


        public string WrapProjectDateTime(MagnumProject newProject)
        {
            var finishTimeTemp = newProject.FinishTime.Ticks;
            DigitInfo digits = DigitInfo.GetDigits(finishTimeTemp);

            var rarityClass = ((ItemRarity)digits.D6_Rarity).ToString().ToLower();

            var newId =
                $"{newProject.DevelopId}_custom_poq_{rarityClass}_{finishTimeTemp.ToString()}";
            return newId;
        }

        public MagnumProjectWrapper SplitItemId(string itemId)
        {
            if (itemId.Contains("_poq_"))
            {
                var newResult = itemId.Split(new string[] { "_poq_" }, StringSplitOptions.None);

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
                    fullstring = itemId,
                };
            }

            var realBaseId2 = itemId.Replace("_custom", string.Empty); // Real Base item ID
            var customId2 = realBaseId2 + "_custom"; // Custom ID

            return new MagnumProjectWrapper
            {
                id = realBaseId2,
                customId = customId2,
                rarity = "Standard",
                rarityClass = ItemRarity.Standard,
                uid = 0,
                fullstring = itemId,
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
