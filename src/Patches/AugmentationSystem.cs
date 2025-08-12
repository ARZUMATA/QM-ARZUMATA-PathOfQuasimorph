using HarmonyLib;
using MGSC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using TMPro;

namespace QM_PathOfQuasimorph.Core
{
    internal partial class PathOfQuasimorph
    {
        [HarmonyPatch(typeof(AugmentationSystem), 
            nameof(AugmentationSystem.RemoveAugmentation),
            new Type[]
            {
                typeof(Mercenary),
                typeof(string),
                typeof(ItemStorage),
                typeof(bool),
            }
        )]
        public static class AugmentationSystem_RemoveAugmentation_Patch
        {
            public static bool Prefix(Mercenary mercenary, string woundSlotId, ItemStorage activeCargo, bool isItemSpawn = false)
            {
                /*
                 * Determine if we need to apply rarity to the item that is about to be produced by removing augment
                 * if an item is standard system will try to apply poq rarity, we don't need it
                 */

                if (isItemSpawn)
                {
                    Plugin.Logger.Log("AugmentationSystem_RemoveAugmentation_Patch");
                    Plugin.Logger.Log("Item is isItemSpawn. CanDo = false.");
                    ItemFactoryContext.CanDo = false;
                    ItemFactoryContext.Context = "RemoveAugmentation";
                }

                return true;
            }
        }
    }

    internal partial class PathOfQuasimorph
    {
        [HarmonyPatch(typeof(AugmentationSystem),
            nameof(AugmentationSystem.RemoveImplant),
            new Type[]
            {
                typeof(CreatureData),
                typeof(ImplantSocketData),
                typeof(string),
                typeof(ItemStorage),
                typeof(bool),
            }
        )]
        public static class AugmentationSystem_RemoveImplant_Patch
        {
            public static bool Prefix(CreatureData creatureData, ImplantSocketData socketData, string implantId, ItemStorage activeCargo, bool isItemSpawn = false)
            {
                if (isItemSpawn)
                {
                    Plugin.Logger.Log("AugmentationSystem_RemoveImplant_Patch");
                    Plugin.Logger.Log("Item is isItemSpawn. CanDo = false.");
                    ItemFactoryContext.CanDo = false;
                    ItemFactoryContext.Context = "RemoveImplant";
                }

                return true;
            }
        }
    }
}
