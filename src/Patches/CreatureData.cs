using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UI;
using UnityEngine;
using MGSC;
using HarmonyLib;
using System.Xml;
using Random = System.Random;
using static QM_PathOfQuasimorph.Core.CreaturesControllerPoq;
using System.Drawing;

namespace QM_PathOfQuasimorph.Core
{
    [HarmonyPatch(typeof(CreatureData), "OnAfterLoad")]
    public static class CreatureData_OnAfterLoad_Patch
    {
        public static void Postfix(CreatureData __instance)
        {
            // Base64 and load only if if available
            // UltimateSkullItemId used only for Mercenaries so it's null for mobs
            // We can use this as our unique id placeholder

            // We have string entry we use for our embedded data
            if (__instance.UltimateSkullItemId != null)// && __instance.UltimateSkullItemId.EndsWith("=="))
            {
                Plugin.Logger.Log($"CreatureData_OnAfterLoad_Patch");
                Plugin.Logger.Log($"\t\t UltimateSkullItemId: {__instance.UltimateSkullItemId}");

                // Try deserialize first
                var creatureDataPoq = CreatureDataPoq.DeserializeDataBase64(__instance.UltimateSkullItemId);
                if (creatureDataPoq != null)
                {
                    if (PathOfQuasimorph.creaturesControllerPoq.creatureDataPoq.ContainsKey(__instance.UniqueId) == false)
                    {
                        Plugin.Logger.Log($"\t\t creatureDataPoq rarity: {creatureDataPoq.rarity}");

                        PathOfQuasimorph.creaturesControllerPoq.creatureDataPoq.Add(__instance.UniqueId, creatureDataPoq);
                    }
                }
                else
                {
                    Plugin.Logger.Log($"\t\t Failed decoding.");

                }
            }
        }
    }
}

