using HarmonyLib;
using MGSC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static QM_PathOfQuasimorph.Core.CreaturesControllerPoq;

namespace QM_PathOfQuasimorph.Core
{
    internal partial class PathOfQuasimorph
    {
        public static class MobContext
        {
            [ThreadStatic]
            public static int CurrentMobId = -1;
        }

        //This generates creature data without uniqueId
        [HarmonyPatch(typeof(CreatureSystem), "GenerateMonster")]
         public static class CreatureSystem_GenerateMonster_Patch
        {
            public static void Postfix(ref CreatureData __result)
            {
                Plugin.Logger.Log($"CreatureSystem_GenerateMonster_Patch");
            }
        }

        // UniqueID assigned here
        [HarmonyPatch(typeof(CreatureSystem), "SpawnMonsterFromMobClass")]
        public static class CreatureSystem_SpawnMonsterFromMobClass_Patch
        {
            public static void Postfix(ref Monster __result)
            {
                Plugin.Logger.Log($"SpawnMonsterFromMobClass: creatureUniqueId: {__result.CreatureData.UniqueId}");

                if (!Plugin.Config.EnableMobs)
                {
                    return;
                }

                // Check if mob has UltimateSkullItemId
                if (__result.CreatureData.UltimateSkullItemId == string.Empty && __result.CreatureData.Perks.Count == 0)
                {
                    // All processed mobs have either serialized data here or they contain real ID.
                    // Also all processed mobs have some perks, so if its zero then it's a new mob
                    // If it's empty its 'our client', so we can overwrite value
                    
                    if (PathOfQuasimorph.creaturesControllerPoq.creatureDataPoq.ContainsKey(__result.CreatureData.UniqueId) == true)
                    {
                        Plugin.Logger.Log($"\t\t removing old creatudetada with {__result.CreatureData.UniqueId}");

                        PathOfQuasimorph.creaturesControllerPoq.creatureDataPoq.Remove(__result.CreatureData.UniqueId);
                    }
                }

                if (PathOfQuasimorph.creaturesControllerPoq.creatureDataPoq.ContainsKey(__result.CreatureData.UniqueId) == false)
                {
                    var creatureDataPoq = new CreatureDataPoq();
                    creatureDataPoq.rarity = PathOfQuasimorph.creaturesControllerPoq.SelectRarity();

                    // For debug purposes
                    //if (creatureDataPoq.rarity == ItemRarity.Standard || creatureDataPoq.rarity == ItemRarity.Enhanced)
                    //{
                    //    creatureDataPoq.rarity = ItemRarity.Quantum;
                    //}
                    
                    MobContext.CurrentMobId = __result.CreatureData.UniqueId;

                    PathOfQuasimorph.creaturesControllerPoq.creatureDataPoq.Add(__result.CreatureData.UniqueId, creatureDataPoq);
                    __result.CreatureData.UltimateSkullItemId = creatureDataPoq.SerializeData();

                    PathOfQuasimorph.creaturesControllerPoq.ApplyStatsFromRarity(ref __result, creatureDataPoq.rarity);

                    Plugin.Logger.Log($"\t\t UniqueId: {__result.CreatureData.UniqueId} rarity: {creatureDataPoq.rarity} ");
                }
            }
        }
    }
}
