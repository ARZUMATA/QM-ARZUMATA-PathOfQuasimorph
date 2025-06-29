using HarmonyLib;
using MGSC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QM_PathOfQuasimorph.Core
{
    internal partial class PathOfQuasimorph
    {

        private static void WaitForDevelopId(TimeSpan timeout, MagnumProject newProject)
        {
            var startTime = DateTime.UtcNow;

            while (DateTime.UtcNow - startTime < timeout)
            {
                if (Data.Items.Ids.Contains(newProject.DevelopId))
                {
                    Plugin.Logger.Log($"DevelopId found in Data.Items.Ids: {newProject.DevelopId}");
                    return;
                }

                if (Data.Items.Records.Any(r => r.Id == newProject.DevelopId))
                {
                    Plugin.Logger.Log($"DevelopId found in Data.Items.Records: {newProject.DevelopId}");
                    return;
                }

                Plugin.Logger.Log($"Awaiting: {newProject.DevelopId}");

                System.Threading.Thread.Sleep(1000); // Wait 1 second before checking again
            }

            Plugin.Logger.Log($"Timeout: DevelopId {newProject.DevelopId} not found within {timeout.TotalSeconds} seconds.");
        }

        // Not used. Debug purposes.
        [HarmonyPatch(typeof(ItemDropSystem), nameof(ItemDropSystem.Randomize))]
        public static class ItemDropSystem_Randomize_Patch
        {
            public static void Postfix(ref string __result)
            {
                //Plugin.Logger.Log($"ItemDropSystem_Randomize_Patch");
                //Plugin.Logger.Log($"\tresult: {__result}");
            }
        }

        // Not used. Debug purposes.
        [HarmonyPatch(typeof(ItemDropSystem), nameof(ItemDropSystem.GenerateItems))]
        public static class ItemDropSystem_GenerateItems_Patch
        {
            public static void Postfix(ref List<GeneratedItemData> __result)
            {
                //Plugin.Logger.Log($"ItemDropSystem_GenerateItems_Patch");
                foreach (GeneratedItemData item in __result)
                {
                    //Plugin.Logger.Log($"\titem: {item.ItemId}");
                    //Plugin.Logger.Log($"\tpoints: {item.Points}");

                }
            }
        }

        [HarmonyPatch(typeof(MagnumDevelopmentSystem), nameof(MagnumDevelopmentSystem.InjectProjectRecord))]
        public static class MagnumDevelopmentSystems_InjectProjectRecord_Patch
        {
            public static bool Prefix(MagnumProject project)
            {
                //Plugin.Logger.Log($" MagnumDevelopmentSystems_InjectProjectRecord_Patch : Prefix");
                return true;
            }

            public static void Postfix(MagnumProject project)
            {
                //Plugin.Logger.Log($" MagnumDevelopmentSystems_InjectProjectRecord_Patch : Postfix");
            }
        }


        // This injects our created projects.
        [HarmonyPatch(typeof(MagnumDevelopmentSystem), nameof(MagnumDevelopmentSystem.InjectProjectRecords))]
        public static class MagnumDevelopmentSystems_InjectProjectRecords_Patch
        {
            public static bool Prefix(MagnumProjects projects)
            {
                return true;
                List<string> outputItemList = new List<string>(Data.ProduceReceipts.Count);

                foreach (ItemProduceReceipt itemProduceReceipt in Data.ProduceReceipts)
                {
                    outputItemList.Add(itemProduceReceipt.OutputItem);
                }

                foreach (string text in outputItemList)
                {
                    Plugin.Logger.Log($" text : {text}");
                    MagnumProjectType itemProjectType = MagnumDevelopmentSystem.GetItemProjectType(text);
                    if (itemProjectType != MagnumProjectType.None)
                    {
                        Plugin.Logger.Log($"\t\t itemProjectType : {itemProjectType}");
                        Plugin.Logger.Log($"\t\t text : {text}");

                        MagnumDevelopmentSystem.InjectItemRecord(new MagnumProject(itemProjectType, text));
                    }
                }


                //foreach (MagnumProject magnumProject in projects.Values)
                //{
                //    list.Remove(magnumProject.DevelopId);
                //    list2.Remove(magnumProject.DevelopId);
                //    MagnumDevelopmentSystem.InjectProjectRecord(magnumProject);
                //}
                //foreach (string text in list)
                //{
                //    MagnumProjectType itemProjectType = MagnumDevelopmentSystem.GetItemProjectType(text);
                //    if (itemProjectType != MagnumProjectType.None)
                //    {
                //        MagnumDevelopmentSystem.InjectItemRecord(new MagnumProject(itemProjectType, text));
                //    }
                //}
                //foreach (string developId in list2)
                //{
                //    MagnumDevelopmentSystem.InjectMercenaryProfileRecord(new MagnumProject(MagnumProjectType.Mercenary, developId));
                //}
                return true;

            }




            public static void Postfix(MagnumProjects projects)
            {
                //Plugin.Logger.Log("MagnumDevelopmentSystems_InjectProjectRecords_Patch :: Postfix");

                //foreach (MagnumProject magnumProject in projects.Values)
                //{
                //    Plugin.Logger.Log($" Get : {magnumProject.DevelopId}");
                //    Plugin.Logger.Log($" Get _customRecord: {magnumProject._customRecord?.Id}");
                //}

                return;

                List<string> list = new List<string>(Data.ProduceReceipts.Count);
                Plugin.Logger.Log("List<string> list");
                Plugin.Logger.Log($"Data.ProduceReceipts.Count {Data.ProduceReceipts.Count}");
                Plugin.Logger.Log($"list.Count {list.Count}");

                foreach (string text in list)
                {
                    Plugin.Logger.Log($" text : {text}");
                    MagnumProjectType itemProjectType = MagnumDevelopmentSystem.GetItemProjectType(text);
                    if (itemProjectType != MagnumProjectType.None)
                    {

                        //MagnumDevelopmentSystem.InjectItemRecord(new MagnumProject(itemProjectType, text));
                    }
                    else
                    {
                        Plugin.Logger.Log($" itemProjectType : {itemProjectType}");

                    }
                }

            }



        }







    }
}
