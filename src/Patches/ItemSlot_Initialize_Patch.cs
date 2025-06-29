using HarmonyLib;
using MGSC;
using QM_PathOfQuasimorph.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QM_PathOfQuasimorph.Core;
using UnityEngine;
using UnityEngine.UI;

namespace QM_PathOfQuasimorph.Core
{
    [Serializable]
    internal class RarityBackgroundComponent : MonoBehaviour
    {
        [SerializeField]
        public GameObject rarityBackgroundGameObject;

        [SerializeField]
        public Image imageComponent;

        private RarityBackgroundComponent()
        {
            // Try to get the RectTransform of the parent GameObject — this is the main ItemSlot
            RectTransform itemSlotRectTransform = transform.parent.GetComponent<RectTransform>();

            // Set this component's transform as a child of the ItemSlot's RectTransform
            transform.SetParent(itemSlotRectTransform, false);

            // Create a new GameObject to hold the rarity background imageComponent
            rarityBackgroundGameObject = new GameObject("RarityBackground");

            // Make the new GameObject a child of this component's GameObject
            rarityBackgroundGameObject.transform.SetParent(transform, false);

            // Add an Image component to the container
            imageComponent = rarityBackgroundGameObject.AddComponent<Image>();
            imageComponent.raycastTarget = false;  // Makes it click-through

            // If the Image component failed to add, log a warning and stop
            if (imageComponent == null)
            {
                Plugin.Logger.LogWarning($"No Image component found on parent of {name}. Cannot apply rarity color.");
                return;
            }

            // Get the RectTransform of the new container
            RectTransform rarityBackgroundRectTransform = rarityBackgroundGameObject.GetComponent<RectTransform>();

            // Set the anchors to stretch from 0 to 1 on both X and Y
            // This means it will take up the full size of its parent
            rarityBackgroundRectTransform.anchorMin = Vector2.zero;
            rarityBackgroundRectTransform.anchorMax = Vector2.one;

            // Set the pivot point to match the anchor for even and consistent layout
            rarityBackgroundRectTransform.pivot = new Vector2(0f, 0f);

            // Set the sizeDelta to 0 to match the parent's size exactly and so it doesn't resize beyond anchor settings
            rarityBackgroundRectTransform.sizeDelta = Vector2.zero;

            // Set the position to (0,0), centered in the parent
            rarityBackgroundRectTransform.anchoredPosition = Vector2.zero;

            // Set the Z position to make it appear behind the item
            rarityBackgroundRectTransform.SetAsFirstSibling();
        }

        public void SetRarityColor(Color color)
        {
            color.a = 0.35f;
            imageComponent.color = color;
        }

        public void EnableDisableComponent(bool active)
        {
            imageComponent.enabled = active;
        }
    }

    internal partial class PathOfQuasimorph
    {
        public static void ApplyItemRarityBackground(ItemSlot __instance, BasePickupItem item)
        {
            if (item != null)
            {
                Plugin.Logger.Log($"ApplyItemRarityBackground");
                Plugin.Logger.Log($"\t\t {item.Id}");

                /* Note to self and explanation:
                __instance is the ItemSlot component
                __instance.gameObject is the actual UI element (a GameObject) that the item is displayed on.
                ItemSlot is attached to GameObject.
                We check if the GameObject already has a RarityBackgroundComponent attached to it, which is a separate MonoBehaviour.
                */

                // Check if the ItemSlot GameObject already has our RarityBackgroundComponent component.
                RarityBackgroundComponent rarityComponent = __instance.gameObject.GetComponent<RarityBackgroundComponent>();

                if (rarityComponent == null)
                {
                    rarityComponent = __instance.gameObject.AddComponent<RarityBackgroundComponent>();
                }

                // Update or create imageComponent on the GameObject
                var rarity = magnumProjectsController.SplitItemId(item.Id).rarityClass;

                if (rarity != ItemRarity.Standard)
                {
                    rarityComponent.SetRarityColor(magnumProjectsController.raritySystem.RarityToUnityColor(rarity));
                    rarityComponent.EnableDisableComponent(true);
                }
                else
                {
                    rarityComponent.EnableDisableComponent(false);
                }
            }
        }

        [HarmonyPatch(typeof(ItemSlot), nameof(ItemSlot.Initialize))]
        public static class ItemSlot_Initialize_Patch
        {
            public static void Postfix(ItemSlot __instance, BasePickupItem item, ItemStorage itemStorage)
            {
                ApplyItemRarityBackground(__instance, item);
            }
        }
    }
}
