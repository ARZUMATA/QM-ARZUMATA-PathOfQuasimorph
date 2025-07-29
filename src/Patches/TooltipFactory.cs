using HarmonyLib;
using MGSC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static UnityEngine.UI.Image;
using Type = System.Type;

namespace QM_PathOfQuasimorph.Core
{
    internal partial class PathOfQuasimorph
    {
        [HarmonyPatch(typeof(TooltipFactory), "AddActiveAbilityDesc", new Type[] {
            typeof(PerkRecord),
            typeof(Mercenary),
            typeof(string),
        }
        )]

        public static class TooltipFactory_AddActiveAbilityDesc_TranspilerPatch
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
                   new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(MGSC.ConfigTableRecord), "get_Id")),
                   // Match any stloc opcode
                   new CodeMatch(i =>
                       i.opcode == OpCodes.Stloc_0 ||
                       i.opcode == OpCodes.Stloc_1 ||
                       i.opcode == OpCodes.Stloc_2 ||
                       i.opcode == OpCodes.Stloc_S),
                   // Match any ldloc opcode
                   new CodeMatch(i =>
                       i.opcode == OpCodes.Ldloc_0 ||
                       i.opcode == OpCodes.Ldloc_1 ||
                       i.opcode == OpCodes.Ldloc_2 ||
                       i.opcode == OpCodes.Ldloc_S)
                   )
               .ThrowIfNotMatch("Did not find the first match.");

                // Capture the local variable index from the original method
                // This assumes that the 'Stloc_S' instruction stores to V_8
                byte idLocalIndex = matcher.GetLocalIndex();

                Plugin.Logger.Log($"idLocalIndex : {idLocalIndex}");

                // Insert code the matched block:
                // string id = MetadataWrapper.GetBaseId(perkRecord);
                matcher.Advance(0) // Don't advance, just for clarity as we need to insert before Ldloc_S
                .Insert(
                    new CodeInstruction(OpCodes.Ldloc_S, idLocalIndex), // Load the id (V_8)
                     new CodeInstruction(OpCodes.Call, getBaseId),      // Call MetadataWrapper.GetBaseId
                     new CodeInstruction(OpCodes.Stloc_S, idLocalIndex)  // Store the result back to id (V_8)
                );

                // Log the resulting opcode list for debugging
                foreach (var ci in matcher.InstructionEnumeration())
                {
                    Plugin.Logger.Log($"{ci.opcode} {(ci.operand == null ? "" : $", {ci.operand}")}");
                }

                return matcher.InstructionEnumeration().ToList();
            }
        }
    }
}
