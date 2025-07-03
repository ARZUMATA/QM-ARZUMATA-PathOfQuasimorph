using System;
using System.Net;
using MGSC;

namespace QM_PathOfQuasimorph.Core
{
    internal partial class MagnumPoQProjectsController
    {
        public class MagnumProjectWrapper
        {
            public string Id { get; set; }
            public string CustomId { get; set; }
            //public string Rarity { get; set; }
            public ItemRarity RarityClass { get; set; }
            public DateTime StartTime { get; set; }
            public DateTime FinishTime { get; set; }
            public bool PoqItem { get; set; }

            public MagnumProjectWrapper(MagnumProject newProject)
            {
                // Generate metadata
                this.Id = newProject.DevelopId;

                // This is our project based on time.
                if (MagnumPoQProjectsController.IsPoqProject(newProject))
                {
                    PoqItem = true;
                }
                else
                {
                    PoqItem = false;
                }

                if (PoqItem)
                {
                    CustomId = $"{Id}_custom_poq";
                    var digitinfo = DigitInfo.GetDigits(newProject.FinishTime.Ticks);
                    RarityClass = (ItemRarity)digitinfo.Rarity;
                    //Rarity = RarityClass.ToString().ToLower();
                    StartTime = newProject.StartTime;
                    FinishTime = newProject.FinishTime;

                }
                else
                {
                    CustomId = $"{Id}_custom";
                    RarityClass = ItemRarity.Standard;
                    //Rarity = RarityClass.ToString().ToLower();
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
                    this.CustomId = $"{id}_custom_poq";
                }
                else
                {
                    this.CustomId = $"{id}_custom";
                }

                var digitinfo = DigitInfo.GetDigits(finishTime.Ticks);
                this.RarityClass = (ItemRarity)digitinfo.Rarity;
                //this.Rarity = RarityClass.ToString().ToLower();
                this.StartTime = startTime;
                this.FinishTime = finishTime;
                this.PoqItem = poqItem;

                // Excessive logging
                //Plugin.Logger.Log($"Created MagnumProjectWrapper with id: {this.Id}, CustomId: {this.CustomId}, Rarity: {this.Rarity}, RarityClass: {this.RarityClass}, StartTime: {this.StartTime.Ticks}, FinishTime: {this.FinishTime.Ticks}, PoqItem: {this.PoqItem}");
            }

            public string ReturnItemUid(bool originalId = false)
            {
                if (originalId)
                {
                    return $"{this.Id}";
                }

                if (PoqItem)
                {
                    return $"{this.CustomId}_{this.StartTime.Ticks.ToString()}_{this.FinishTime.Ticks.ToString()}";
                    //return $"{this.CustomId}_{this.Rarity}_{this.StartTime.Ticks.ToString()}_{this.FinishTime.Ticks.ToString()}";
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
                        startTime: new DateTime(Int64.Parse(suffixParts[0])),
                        finishTime: new DateTime(Int64.Parse(suffixParts[1]))
                        );

                    return wrapper;
                }

                var realBaseId2 = uid.Replace("_custom", string.Empty); // Real Base item ID
                var customId2 = realBaseId2 + "_custom"; // Custom ID

                return new MagnumProjectWrapper
                {
                    Id = realBaseId2,
                    CustomId = customId2,
                    //Rarity = "Standard",
                    RarityClass = ItemRarity.Standard,
                    StartTime = DateTime.MinValue,
                    FinishTime = DateTime.MinValue,
                    PoqItem = false
                };
            }
        }
    }
}
