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

            public static bool Prefix(Difficulty difficulty, Creatures creatures, TurnController turnController, string mobClassId, CellPosition pos, int groupIndex = -1, bool spawnEquipment = true, int customActionPoints = -1, bool isLeader = false, int techLevelLimit = -1, int skinIndex = -1, string specificBodyTypeId = null, string hairType = "", string hairColor = "", string factionId = "", Creature actAfter = null)
            {
                if (!Plugin.Config.EnableMobs)
                {
                    return true;
                }

                Plugin.Logger.Log($"Prefix");
                Plugin.Logger.Log($"spawnEquipment {spawnEquipment}");

                // Decide mob rarity only if spawnEquipment is true
                if (spawnEquipment)
                {
                    creatureDataPoq = new CreatureDataPoq();
                    creatureDataPoq.rarity = PathOfQuasimorph.creaturesControllerPoq.SelectRarity();
                    MobContext.Rarity = creatureDataPoq.rarity;

                    // Upcoming creature Id
                    MobContext.CurrentMobId = creatures.CreatureIdCounter + 1;

                    MobContext.ProcesingMobRarity = true;

                    Plugin.Logger.Log($"SpawnMonsterFromMobClass: Upcoming creature Id: {MobContext.CurrentMobId}");
                    Plugin.Logger.Log($"SpawnEquipment is TRUE. Assigned rarity: {creatureDataPoq.rarity}");

                }
                else
                {
                    Plugin.Logger.Log("spawnEquipment is FALSE - Skipping rarity assignment.");
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

                    if (MobContext.ProcesingMobRarity)
                    {
                    PathOfQuasimorph.creaturesControllerPoq.creatureDataPoq.Add(__result.CreatureData.UniqueId, creatureDataPoq);
                    __result.CreatureData.UltimateSkullItemId = creatureDataPoq.SerializeDataBase64();

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
}
