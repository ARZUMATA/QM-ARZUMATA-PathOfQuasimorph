using HarmonyLib;
using MGSC;
using QM_PathOfQuasimorph.Core.Records;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace QM_PathOfQuasimorph.Core
{
    internal partial class PathOfQuasimorph
    {
        [HarmonyPatch(typeof(ItemReceiptPanel), nameof(ItemReceiptPanel.Initialize), new Type[]
        {
            typeof(MagnumCargo),
            typeof(MagnumProgression),
            typeof(MagnumProjects),
            typeof(ItemProduceReceipt),
            typeof(Difficulty),
        }
        )]
        public static class ItemReceiptPanel_Initialize_Patch
        {
            public static void Postfix(ItemReceiptPanel __instance, MagnumCargo magnumCargo, MagnumProgression magnumSpaceship, MagnumProjects projects, ItemProduceReceipt receipt, Difficulty difficulty)
            {
                // Add rarity backgrounds
                var rarityComponent = ApplyItemRarityBackground(__instance);

                CompositeItemRecord compositeItemRecord = Data.Items.GetRecord(receipt.OutputItem, true) as CompositeItemRecord;
                SynthraformerRecord record = compositeItemRecord.GetRecord<SynthraformerRecord>();

                if (record != null)
                {
                    ApplyBackground(rarityComponent, record.Rarity);
                }
                else
                {
                    rarityComponent.EnableDisableComponent(false);
                }
            }

            private static RarityBackgroundComponent ApplyItemRarityBackground(ItemReceiptPanel component)
            {
                var iconBorder = component.transform.Find("IconBorder");
                var rarityComponent = AddRarityBackgroundComponent(iconBorder.gameObject);
                return rarityComponent;
            }
        }
    }
}
