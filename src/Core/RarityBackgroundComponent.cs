using MGSC;
using System;
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

            // Set the Z position to a value that ensures it appears behind the item (negative)
            // But if we want it to appear on top as overlay, use a positive Z value
            //transform.position = new Vector3(transform.position.x, transform.position.y, 100f); // Adjust Z as needed to stay on top

            // Set the Z position to make it appear before the item
            //rarityBackgroundRectTransform.SetAsLastSibling();

            // Set the Z position to make it appear behind the item
            rarityBackgroundRectTransform.SetAsFirstSibling();
        }

        public void SetRarityColor(Color color)
        {
            color.a = 0.2f;
            imageComponent.color = color;
        }

        public void EnableDisableComponent(bool active)
        {
            imageComponent.enabled = active;
            rarityBackgroundGameObject.SetActive(active);
        }
    }
}
