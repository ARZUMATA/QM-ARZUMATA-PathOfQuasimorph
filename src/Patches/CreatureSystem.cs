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
        private static int mapMetadataMonstersCount;

        // This generates creature data without uniqueId
        // [HarmonyPatch(typeof(CreatureSystem), "GenerateMonster")]
        // public static class CreatureSystem_GenerateMonster_Patch
        // {
        //     public static void Postfix(ref CreatureData __result)
        //     {

        //     }
        // }

        [HarmonyPatch(typeof(CreatureSystem), "SpawnMonsterFromMobClass")]
        public static class CreatureFactory_SpawnMonsterFromMobClass_Patch
        {
            public static void Postfix(ref Monster __result)
            {
                if (PathOfQuasimorph.creaturesControllerPoq.creatureDataPoq.ContainsKey(__result.CreatureData.UniqueId) == false)
                {
                    var creatureDataPoq = new CreatureDataPoq();
                    creatureDataPoq.rarity = PathOfQuasimorph.raritySystem.SelectRarity();

                    if (creatureDataPoq.rarity == ItemRarity.Standard || creatureDataPoq.rarity == ItemRarity.Enhanced)
                    {
                        creatureDataPoq.rarity = ItemRarity.Quantum;
                    }

                    PathOfQuasimorph.creaturesControllerPoq.creatureDataPoq.Add(__result.CreatureData.UniqueId, creatureDataPoq);
                    __result.CreatureData.UltimateSkullItemId = creatureDataPoq.SerializeData();

                    PathOfQuasimorph.creaturesControllerPoq.ApplyStatsFromRarity(ref __result, creatureDataPoq.rarity);

                    Plugin.Logger.Log($"CreatureSystem: creatureUniqueId: {__result.CreatureData.UniqueId} rarity: {creatureDataPoq.rarity} ");
                }
            }
        }
    }
}
