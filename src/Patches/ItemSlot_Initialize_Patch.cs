using HarmonyLib;
using MGSC;
using QM_PathOfQuasimorph.Core;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QM_PathOfQuasimorph.Core;
using static QM_PathOfQuasimorph.Core.MagnumPoQProjectsController;

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

                ApplyItemRarityBackground(__instance, item);
            }
        }

        public static void ApplyItemRarityBackground(ItemSlot __instance, BasePickupItem item)
        {
            // Check if the ItemSlot GameObject already has our RarityBackgroundComponent component.
            RarityBackgroundComponent rarityComponent = __instance.gameObject.GetComponent<RarityBackgroundComponent>();

            if (rarityComponent == null)
            {
                rarityComponent = __instance.gameObject.AddComponent<RarityBackgroundComponent>();
            }

            if (item == null)
            {
                rarityComponent.EnableDisableComponent(false);
            }

            if (item != null)
            {
                /* Note to self and explanation:
                __instance is the ItemSlot component
                __instance.gameObject is the actual UI element (a GameObject) that the item is displayed on.
                ItemSlot is attached to GameObject.
                We check if the GameObject already has a RarityBackgroundComponent attached to it, which is a separate MonoBehaviour.
                */

                // Update or create imageComponent on the GameObject
                var rarity = MagnumProjectWrapper.SplitItemUid(item.Id).RarityClass;

                if (rarity != ItemRarity.Standard)
                {
                    rarityComponent.SetRarityColor(MagnumPoQProjectsController.raritySystem.RarityToUnityColor(rarity));
                    rarityComponent.EnableDisableComponent(true);
                }
                else
                {
                    rarityComponent.EnableDisableComponent(false);
                }
            }
        }
    }
}
