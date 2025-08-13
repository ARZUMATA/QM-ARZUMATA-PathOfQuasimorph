using HarmonyLib;
using MGSC;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static QM_PathOfQuasimorph.Controllers.MagnumPoQProjectsController;
using System;
using UnityEngine.UI;
using UnityEngine;
using QM_PathOfQuasimorph.Components;
using QM_PathOfQuasimorph.Records;

namespace QM_PathOfQuasimorph.Core
{
    internal partial class PathOfQuasimorph
    {
        [HarmonyPatch(typeof(ItemSlot), nameof(ItemSlot.Initialize))]
        public static class ItemSlot_Initialize_Patch
        {
            public static void Postfix(ItemSlot __instance, BasePickupItem item, ItemStorage itemStorage)
            {
                // If mod not enabled, don't create any more backgrounds.
                if (!Plugin.Config.Enable)
                {
                    return;
                }

                ApplyItemRarityBackground(__instance.gameObject, item);
            }
        }

        private static RarityBackgroundComponent AddRarityBackgroundComponent(GameObject gameObject)
        {
            // Check if the ItemSlot GameObject already has our RarityBackgroundComponent component.
            RarityBackgroundComponent rarityComponent = gameObject.GetComponent<RarityBackgroundComponent>();

            if (rarityComponent == null)
            {
                rarityComponent = gameObject.AddComponent<RarityBackgroundComponent>();
            }

            return rarityComponent;

        }

        public static void ApplyItemRarityBackground(GameObject gameObject, BasePickupItem item)
        {
            var rarityComponent = AddRarityBackgroundComponent(gameObject);

            if (item == null)
            {
                rarityComponent.EnableDisableComponent(false);
            }

            if (item != null)
            {
                /* Note to self and explanation:
                gameObject is the ItemSlot component
                gameObject.gameObject is the actual UI element (a GameObject) that the item is displayed on.
                ItemSlot is attached to GameObject.
                We check if the GameObject already has a RarityBackgroundComponent attached to it, which is a separate MonoBehaviour.
                */

                // Update or create imageComponent on the GameObject
                var rarity = MetadataWrapper.SplitItemUid(item.Id).RarityClass;

                var synthraformerRecord = item.Record<SynthraformerRecord>();

                if (synthraformerRecord != null)
                {
                    rarity = synthraformerRecord.Rarity;
                }

                ApplyBackground(rarityComponent, rarity);
            }
        }

        private static void ApplyBackground(RarityBackgroundComponent rarityComponent, ItemRarity rarity)
        {
            if (rarity != ItemRarity.Standard)
            {
                rarityComponent.SetRarityColor(PathOfQuasimorph.raritySystem.RarityToUnityColor(rarity));
                rarityComponent.EnableDisableComponent(true);
            }
            else
            {
                rarityComponent.EnableDisableComponent(false);
            }
        }
    }
}
