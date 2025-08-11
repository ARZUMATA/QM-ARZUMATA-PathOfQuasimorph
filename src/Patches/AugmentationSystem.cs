using HarmonyLib;
using MGSC;
using System;
using System.Collections.Generic;
using System.Linq;
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
            public static void Prefix(Mercenary mercenary, string woundSlotId, ItemStorage activeCargo, bool isItemSpawn = false)
            {
                /*
                 * Determine if we need to apply rarity to the item that is about to be produced by removing augment
                 * if an item is standard system will try to apply poq rarity, we don't need it
                 */

                if (isItemSpawn)
                {
                    ItemFactoryContext.CanDo = false;
                }
            }
        }
    }
}
