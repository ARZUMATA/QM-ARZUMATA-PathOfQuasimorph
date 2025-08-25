using HarmonyLib;
using MGSC;
using QM_PathOfQuasimorph.Controllers;
using QM_PathOfQuasimorph.PoQHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static QM_PathOfQuasimorph.Contexts.PathOfQuasimorph;
using static QM_PathOfQuasimorph.Controllers.CreaturesControllerPoq;

namespace QM_PathOfQuasimorph.Core
{
    internal partial class PathOfQuasimorph
    {
        [HarmonyPatch(typeof(CreatureSystem), "SetBareHandSlot")]
        public static class CreatureSystem_SetBareHandSlot_Patch
        {
            public static bool Prefix(CreatureData creatureData)
            {
                CleanupSystem.CleanCreatureData(creatureData);
                return true;
            }
        }

        //This generates creature data without uniqueId
        [HarmonyPatch(typeof(CreatureSystem), "GenerateMonster")]
        public static class CreatureSystem_GenerateMonster_Patch
        {
            public static void Postfix(ref CreatureData __result)
            {
                Plugin.Logger.Log($"CreatureSystem_GenerateMonster_Patch");

                if (!Plugin.Config.EnableMobs)
                {
                    return;
                }

                Plugin.Logger.Log($"MonsterTierWoundSlotEffectsAdd {Plugin.Config.MonsterTierWoundSlotEffectsAdd}");
                Plugin.Logger.Log($"MobContext.ProcesingMobRarity {MobContext.ProcesingMobRarity}");
                Plugin.Logger.Log($"MobContext.Rarity {MobContext.Rarity}");
                Plugin.Logger.Log($"MobContext.CurrentMobId {MobContext.CurrentMobId}");


                // Can adjust mobs before here too
                if (MobContext.ProcesingMobRarity && Plugin.Config.MonsterTierWoundSlotEffectsAdd)
                {
                    if (MobContext.Rarity != CreaturesControllerPoq.MonsterMasteryTier.None)
                    {
                        foreach (var slotkey in __result.WoundSlotMap.ToList())
                        {
                            var randomUidInjected = PathOfQuasimorph.itemRecordsControllerPoq.GenerateUid();
                            MetadataWrapper wrapper;
                            string newId;
                            string boostedParamString = string.Empty;

                            ItemRecordsControllerPoq.GetNewId(slotkey.Key, randomUidInjected, false, out wrapper, out newId);

                            newId = $"{slotkey.Key}_{newId}";
                            Plugin.Logger.Log($"\t new name will be {newId}");

                            var woundSlotRecord = Data.WoundSlots.GetRecord(slotkey.Key);
                            WoundSlotRecord woundSlotRecordNew = ItemRecordHelpers.CloneWoundSlotRecord(woundSlotRecord, $"{newId}");
                            itemRecordsControllerPoq.woundSlotRecordProcessorPoq.Init(woundSlotRecordNew, (ItemRarity)MobContext.Rarity + 1, true, false, $"{newId}", slotkey.Key);
                            itemRecordsControllerPoq.woundSlotRecordProcessorPoq.ProcessRecord(ref boostedParamString);
                            itemRecordsControllerPoq.woundSlotRecordProcessorPoq.FillMobContextEffects(MobContext.Rarity, woundSlotRecord.ImplicitBonusEffects, woundSlotRecord.ImplicitPenaltyEffects);


                            __result.WoundSlotMap[newId] = __result.WoundSlotMap[slotkey.Key];
                            __result.WoundSlotMap.Remove(slotkey.Key);

                            Data.WoundSlots._records[newId] = woundSlotRecordNew;

                            RecordCollection.WoundSlotRecords[newId] = woundSlotRecordNew;
                            Localization.DuplicateKey("woundslot." + slotkey.Key + ".name", "woundslot." + newId + ".name");
                        }

                        AugmentationSystem.ConfigureImplicitEffects(__result);
                    }
                }
            }
        }

        // UniqueID assigned here
        [HarmonyPatch(typeof(CreatureSystem), "SpawnMonsterFromMobClass")]
        public static class CreatureSystem_SpawnMonsterFromMobClass_Patch
        {
            static CreatureDataPoq creatureDataPoq;

            public static bool Prefix(Difficulty difficulty, Creatures creatures, RaidMetadata raidMetadata, TurnController turnController, string mobClassId, CellPosition pos, int groupIndex = -1, bool spawnEquipment = true, int customActionPoints = -1, bool isLeader = false, int techLevelLimit = -1, int skinIndex = -1, string specificBodyTypeId = null, string hairType = "", string hairColor = "", string factionId = "", Creature actAfter = null)
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
                    MobContext.CurrentMobId = raidMetadata.CreatureIdCounter + 1;

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
