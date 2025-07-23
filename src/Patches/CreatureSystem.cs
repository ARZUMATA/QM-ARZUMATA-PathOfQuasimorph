using HarmonyLib;
using MGSC;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static QM_PathOfQuasimorph.Core.CreaturesControllerPoq;

namespace QM_PathOfQuasimorph.Core
{
    internal partial class PathOfQuasimorph
    {
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
            static CreatureDataPoq creatureDataPoq;

            public static bool Prefix(bool spawnEquipment, Creatures creatures)
            {
                if (!Plugin.Config.EnableMobs)
                {
                    return true;
                }

                // Decide mob rarity
                if (spawnEquipment)
                {
                    var creatureDataPoq = new CreatureDataPoq();
                    creatureDataPoq.rarity = PathOfQuasimorph.creaturesControllerPoq.SelectRarity();
                    MobContext.Rarity = creatureDataPoq.rarity;

                    // Upcoming creature Id
                    MobContext.CurrentMobId = creatures.CreatureIdCounter + 1;

                    MobContext.ProcesingMobRarity = true;

                    Plugin.Logger.Log($"SpawnMonsterFromMobClass: Upcoming creature Id: {MobContext.CurrentMobId}");
                }
                else
                {
                    MobContext.Rarity = CreaturesControllerPoq.MonsterMasteryTier.None;
                    MobContext.CurrentMobId = -1;
                    MobContext.ProcesingMobRarity = false;
                }

                return true;
            }

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


                    PathOfQuasimorph.creaturesControllerPoq.creatureDataPoq.Add(__result.CreatureData.UniqueId, creatureDataPoq);
                    __result.CreatureData.UltimateSkullItemId = creatureDataPoq.SerializeData();

                    PathOfQuasimorph.creaturesControllerPoq.ApplyStatsFromRarity(ref __result, creatureDataPoq.rarity);

                    Plugin.Logger.Log($"\t\t UniqueId: {__result.CreatureData.UniqueId} rarity: {creatureDataPoq.rarity} ");

                    MobContext.Rarity = CreaturesControllerPoq.MonsterMasteryTier.None;
                    MobContext.CurrentMobId = -1;
                    MobContext.ProcesingMobRarity = false;
                }
            }
        }
    }
}
