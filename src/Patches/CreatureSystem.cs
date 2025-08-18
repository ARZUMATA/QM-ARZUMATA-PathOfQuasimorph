using HarmonyLib;
using MGSC;
using QM_PathOfQuasimorph.Controllers;
using QM_PathOfQuasimorph.PoQHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
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
                Dictionary<string, (string, string)> replacedData = new Dictionary<string, (string, string)>();

                // This is old save safe-load if we have some missing record so we reset id and effects to the generics.

                foreach (string WoundSlotMapKey in creatureData.WoundSlotMap.Keys.ToList())
                {
                    WoundSlotRecord record = Data.WoundSlots.GetRecord(WoundSlotMapKey, true);

                    if (record == null)
                    {
                        // We missing wouldslot record, we can't reset to baseline as this record used along the json,
                        // so need to get a baseline and clone the record.
                        Plugin.Logger.LogWarning($"Record missing for {WoundSlotMapKey} FIXME");

                        var baseIdExist = MetadataWrapper.TryGetBaseId(WoundSlotMapKey, out string WoundSlot_BaseId);
                        var strArray = WoundSlot_BaseId.Split('_');
                        WoundSlot_BaseId = strArray[0];
                        var Item_BaseId = strArray[0];

                        creatureData.WoundSlotMap[WoundSlot_BaseId] = creatureData.WoundSlotMap[WoundSlotMapKey];
                        creatureData.WoundSlotMap.Remove(WoundSlotMapKey);

                        if (!replacedData.ContainsKey(WoundSlotMapKey))
                        {
                            replacedData.Add(WoundSlotMapKey, (WoundSlot_BaseId, Item_BaseId));
                        }

                        Plugin.Logger.LogWarning($"Reverting to baseid WoundSlotMap Key: {WoundSlot_BaseId}");
                    }
                }

                foreach (string AugmentationMapKey in creatureData.AugmentationMap.Keys.ToList())
                {
                    WoundSlotRecord record = Data.WoundSlots.GetRecord(AugmentationMapKey, true);
                    bool replaced = false;

                    if (record == null)
                    {
                        // We missing wouldslot record, we can't reset to baseline as this record used along the json,
                        // so need to get a baseline and clone the record.
                        Plugin.Logger.LogWarning($"Record missing for {AugmentationMapKey} FIXME");

                        var baseIdExist = MetadataWrapper.TryGetBaseId(AugmentationMapKey, out string WoundSlot_BaseId);
                        var strArray = WoundSlot_BaseId.Split('_');
                        WoundSlot_BaseId = strArray[0];
                        var Item_BaseId = strArray[0];

                        var AugmentationMapKeyValue_BaseId = string.Join("_", strArray.Skip(1));

                        Plugin.Logger.LogWarning($"AugmentationMapKeyValue_BaseId: {AugmentationMapKeyValue_BaseId}");

                        creatureData.AugmentationMap[WoundSlot_BaseId] = AugmentationMapKeyValue_BaseId;
                        creatureData.AugmentationMap.Remove(AugmentationMapKey);

                        if (!replacedData.ContainsKey(AugmentationMapKey))
                        {
                            replacedData.Add(AugmentationMapKey, (WoundSlot_BaseId, Item_BaseId));
                        }

                        Plugin.Logger.LogWarning($"Reverting to baseid AugmentationMap Key: {WoundSlot_BaseId}");
                        Plugin.Logger.LogWarning($"Reverting to baseid AugmentationMap Value: {AugmentationMapKeyValue_BaseId}");
                    }
                    else
                    {
                        if (creatureData.AugmentationMap[AugmentationMapKey] == null)
                        {
                            Plugin.Logger.LogWarning($"creatureData.AugmentationMap[AugmentationMapKey] is NULL.");
                            var drops = record.AmputatedDrop;

                            foreach (var drop in drops)
                            {
                                Plugin.Logger.LogWarning($"checking drop: {drop.Item2}");

                                var itemRec = Data.Items.GetRecord(drop.Item2) as CompositeItemRecord;
                                Plugin.Logger.LogWarning($"itemRec {itemRec == null}");

                                foreach (var rec in itemRec.Records)
                                {
                                    var augRec = rec as AugmentationRecord;

                                    if (augRec != null)
                                    {
                                        Plugin.Logger.LogWarning($"itemRec is AugmentationRecord");
                                        creatureData.AugmentationMap[AugmentationMapKey] = augRec.Id;
                                        replaced = true;
                                        break;
                                    }
                                }

                                if (replaced)
                                {
                                    break;
                                }
                            }

                            if (replaced == false)
                            {
                                creatureData.AugmentationMap.Remove(AugmentationMapKey);

                            }
                        }
                    }
                }

                foreach (var effect in creatureData.EffectsController.Effects)
                {
                    if (effect is WoundEffect woundEffect)
                    {
                        // Now you can access SlotType and ParentWoundId directly
                        // Example:
                        // woundEffect.SlotType = "newSlot";
                        // woundEffect.ParentWoundId = "newWoundId";

                        if (replacedData.ContainsKey(woundEffect.ParentWoundId))
                        {
                            var (woundslot, item) = replacedData[woundEffect.ParentWoundId];
                            woundEffect.ParentWoundId = woundslot;
                        }

                        if (replacedData.ContainsKey(woundEffect.SlotType))
                        {
                            var (woundslot, item) = replacedData[woundEffect.SlotType];
                            woundEffect.SlotType = woundslot;
                        }

                    }
                    else if (effect is ImplicitAugEffect implicitEffect)
                    {
                        if (replacedData.ContainsKey(implicitEffect._woundSlotId))
                        {
                            var (woundslot, item) = replacedData[implicitEffect._woundSlotId];
                            implicitEffect._woundSlotId = woundslot;
                        }
                    }

                }

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
