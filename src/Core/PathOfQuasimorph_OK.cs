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
        /* RGP themed weapon tiers
           1. **Standard**
           2. **Enhanced** // Magical
           3. **Advanced** // Rare
           4. **Premium** // Epic
           5. **Prototype** // Legendary
           6. **Quantum** // Mythic

        //Arcane
        //Exotic
        //Mythic
        //Relic
        //Premium

        * Name project cleanedDevId: devID_custom_poq_prototype_rndhash
       */
        public enum RarityClass
        {
            Standard,
            Enchanced,
            Advanced,
            Premium,
            Prototype,
            Quantum
        }

        public RarityClass GetRarity(string itemId)
        {
            if (itemId.Contains("_poq_"))
            {
                // Example: "pmc_shotgun_1_poq_e_1234567890"

                var newResult = itemId.Split(new string[] { "_poq_" }, 2, StringSplitOptions.None);
                var realBaseId = newResult[0]; // Real Base item ID

                if (newResult.Length > 1)
                {
                    var suffixParts = newResult[1].Split(new string[] { "_" }, 2, StringSplitOptions.None);
                    if (suffixParts.Length > 0)
                    {
                        string rarityName = suffixParts[0]; // e.g., "Prototype"
                        string hash = suffixParts.Length > 1 ? suffixParts[1] : null; // "1234567890"
                                                                                     
                        // Try to parse the rarity name into the enum
                        if (Enum.TryParse(rarityName, true, out RarityClass rarityClass))
                        {
                            return rarityClass;
                        }
                    }
                }
            }

            return RarityClass.Standard; // Default
        }

        /*
         * Our naming structure is different, so we check for the presence of "_poq_" and split the string to get the base.Id without our additions on top of default method.
         */
        [HarmonyPatch(typeof(PickupItem), "get_RenderId")]
        public static class PickupItem_RenderId_Patch
        {
            public static void Postfix(PickupItem __instance, ref string __result)
            {
                if (__result.Contains("_poq_"))
                {
                    var newResult = __result.Split('_');
                    __result = newResult[0]; // Return real base.Id
                }
            }
        }
    }
}
