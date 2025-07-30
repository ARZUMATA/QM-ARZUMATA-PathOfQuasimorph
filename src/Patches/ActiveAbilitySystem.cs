using HarmonyLib;
using MGSC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using static UnityEngine.UI.Image;
using Type = System.Type;

namespace QM_PathOfQuasimorph.Core
{
    internal partial class PathOfQuasimorph
    {
        [HarmonyPatch(typeof(ActiveAbilitySystem), "ProcessAbility", new Type[] {
            typeof(Creature),
            typeof(Perk),
            typeof(MapGrid),
            typeof(MapRenderer),
            typeof(TurnController),
            typeof(MapController),
            typeof(MapObstacles),
            typeof(MapEntities),
            typeof(Creatures),
            typeof(ItemsOnFloor),
            typeof(FireController),
            typeof(Visibilities),
        }
        )]

        public static class ActiveAbilitySystem_ProcessAbility_TranspilerPatch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                // Get original instructions
                var original = instructions.ToList();

                var getBaseId = AccessTools.Method(typeof(MetadataWrapper), nameof(MetadataWrapper.GetBaseId));

                var matcher = new CodeMatcher(original)
                       .MatchEndForward(
                           // string id = perkRecord.Id;
                           new CodeMatch(OpCodes.Ldarg_1),
                           new CodeMatch(OpCodes.Ldfld, AccessTools.Method(typeof(MGSC.Perk), nameof(MGSC.Perk.PerkId))),
                           new CodeMatch(OpCodes.Stloc_0),
                           new CodeMatch(OpCodes.Ldloc_0)
                       )
                       .ThrowIfNotMatch("Did not find the first match.");

                // Insert code at the matched block
                matcher.Advance(0) // Don't advance, just for clarity as we need to insert before Ldloc_S
                    .Insert(
                        new CodeInstruction(OpCodes.Ldloc_0),
                        new CodeInstruction(OpCodes.Call, getBaseId),       // Call MetadataWrapper.GetBaseId
                        new CodeInstruction(OpCodes.Stloc_0)
                    );

                // Log the resulting opcode list for debugging
                foreach (var ci in matcher.InstructionEnumeration())
                {
                    Plugin.Logger.Log($"{ci.opcode} {(ci.operand == null ? "" : $", {ci.operand}")}");
                }

                return matcher.InstructionEnumeration().ToList();
            }
        }

        public static class ActiveAbilitySystem_ProcessAbility_Patch
        {
            public static bool Prefix(Creature creature, Perk ability, MapGrid mapGrid, MapRenderer mapRenderer, TurnController turnController, MapController mapController, MapObstacles mapObstacles, MapEntities mapEntities, Creatures creatures, ItemsOnFloor itemsOnFloor, FireController fireController, Visibilities visibilities)
            {
                string perkId = ability.PerkId;

                // Check if it is our perk that is same as default but with metadata, if we have a match we can call our method or strip metadata.
                // For now as we don't have custom perks and abilities and we can strip metadata via transplier patch.

                if (MetadataWrapper.IsPoqItemUid(perkId))
                {
                    //var wrapper = MetadataWrapper.TryGetBaseId(perkId, out perkId);
                    // call my method here

                    // return false;
                }

                return true;
            }
        }
    }
}
