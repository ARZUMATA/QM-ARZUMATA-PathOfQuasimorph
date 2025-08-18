using HarmonyLib;
using MGSC;
using QM_PathOfQuasimorph.Controllers;
using QM_PathOfQuasimorph.Records;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using static UnityEngine.UI.Image;
using Type = System.Type;

namespace QM_PathOfQuasimorph.Core
{
    internal partial class PathOfQuasimorph
    {
        [HarmonyPatch(typeof(ItemInteractionSystem), nameof(ItemInteractionSystem.CanRepair))]

        // TODO: IL patch
        public static class ItemInteractionSystem_CanRepair_Patch
        {
            public static bool Prefix(BasePickupItem target, BasePickupItem repair, ref bool __result)
            {
                //Plugin.Logger.Log($"ItemInteractionSystem_CanRepair_Patch");

                //if (PathOfQuasimorph.GameLoopGroup != GameLoopGroup.Space)
                //{
                //    // Do original method
                //    return true;
                //}

                if (!repair.Is<SynthraformerRecord>() || target.Locked)
                {
                    // Do original method
                    return true;
                }
                else
                {
                    //Plugin.Logger.Log($"ItemInteractionSystem_CanRepair_Patch");

                    RepairRecord repairRecord = repair.Record<RepairRecord>();
                    SynthraformerRecord synthraformerRecord = repair.Record<SynthraformerRecord>();
                    __result = synthraformerRecord.IsValidTarget((PickupItem)target, synthraformerRecord);
                    //__result = repairRecord.IsValidCategory(target);

                    return false;
                }
            }
        }

        [HarmonyPatch(typeof(ItemInteractionSystem), nameof(ItemInteractionSystem.Repair))]

        public static class ItemInteractionSystem_Repair_Patch
        {
            public static bool Prefix(BasePickupItem target, BasePickupItem repair, Inventory inventory, out bool disassembleToTrash, ref bool __result)
            {
                //Plugin.Logger.Log($"ItemInteractionSystem_Repair_Patch");

                disassembleToTrash = false;

                //if (PathOfQuasimorph.GameLoopGroup != GameLoopGroup.Space)
                //{
                //    // Do original method
                //    return true;
                //}

                var synthraformerRecord = repair.Record<SynthraformerRecord>();
                Plugin.Logger.Log($"synthraformerRecord null {synthraformerRecord == null} synthraformerRecord:{synthraformerRecord}");
                Plugin.Logger.Log($"synthraformerRecord repair.id {repair.Id}");

                if (synthraformerRecord == null)
                {
                    return true;
                }
                else
                {
                    if (synthraformerRecord.IsValidTarget((PickupItem)target, synthraformerRecord))
                    {
                        Plugin.Logger.Log($"synthraformerRecord IsValidTarget");
                        PathOfQuasimorph.synthraformerController.Apply(target, repair, synthraformerRecord, ref __result);

                        if (__result)
                        {
                            ItemInteractionSystem.ConsumeItem(repair);

                        }
                    }

                    return false;
                }
            }
        }


        [HarmonyPatch(typeof(ItemInteractionSystem), nameof(ItemInteractionSystem.Disassemble))]

        public static class ItemInteractionSystem_Disassemble_Patch
        {
            public static void Postfix(ref bool __result, BasePickupItem item, Inventory inventory, ref List<BasePickupItem> itemsWithoutStorage, int toDisassembleCount = -1, bool guaranteed = false)
            {
                if (__result)
                {
                    if (RecordCollection.MetadataWrapperRecords.TryGetValue(item.Id, out var metadata))
                    {
                        Plugin.Logger.Log($"metadata ok");

                        // We can use transformation record but i want it random, so.
                        if (metadata.RarityClass != ItemRarity.Standard)
                        {
                            Plugin.Logger.Log($"metadata.RarityClass: {metadata.RarityClass}");

                            var droptems = SynthraformerController.GetAdditionalDroptems(item, metadata);

                            if (droptems.Count > 0)
                            {
                                Plugin.Logger.Log($"SynthraformerController.GetAdditionalDroptems Count: {droptems.Count}");

                                foreach (var entry in droptems)
                                {
                                    BasePickupItem basePickupItem = SingletonMonoBehaviour<ItemFactory>.Instance.CreateForInventory(entry, false);
                                    if (inventory == null || !inventory.TakeOrEquip(basePickupItem, false, true))
                                    {
                                        itemsWithoutStorage.Add(basePickupItem);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        Plugin.Logger.Log($"RecordCollection.MetadataWrapperRecords.TryGetValue FAILED for item {item.Id}");
                    }
                }
            }
        }
    }
}
