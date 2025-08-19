using HarmonyLib;
using MGSC;
using ModConfigMenu;
using QM_PathOfQuasimorph.Controllers;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using static QM_PathOfQuasimorph.Controllers.MagnumPoQProjectsController;

namespace QM_PathOfQuasimorph.Core
{
    internal partial class PathOfQuasimorph
    {
        [HarmonyPatch(typeof(MagnumDevelopmentSystem), nameof(MagnumDevelopmentSystem.InjectItemRecord))]
        public static class MagnumDevelopmentSystems_InjectItemRecord_TranspilerPatch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                // Get original instructions
                var original = new List<CodeInstruction>(instructions);

                var getPoqItemId = AccessTools.Method(typeof(MetadataWrapper), nameof(MetadataWrapper.GetPoqItemIdFromProject));
                var getItemTransformationRecord = AccessTools.Method(typeof(MagnumPoQProjectsController), nameof(MagnumPoQProjectsController.GetItemTransformationRecord));

                // Using Codematcher to find instruction sequence we need.
                List<CodeInstruction> result = new CodeMatcher(original)
                .MatchEndForward(
                    // Match opcode start for line
                    //string text = project.DevelopId + "_custom";
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(
                        typeof(MGSC.MagnumProject),
                        nameof(MGSC.MagnumProject.DevelopId))
                    ),
                    new CodeMatch(OpCodes.Ldstr),
                    new CodeMatch(OpCodes.Call),
                    new CodeMatch(OpCodes.Stloc_0)
                // Match opcode end
                // For explanation:
                // 0	0000	ldarg.0 >>> load argument 0 in args aka MagnumProject project
                // 1	0001	ldfld	string MGSC.MagnumProject::DevelopId >>> load DevelopId field from project
                // 2	0006	ldstr	"_custom" >>> load string "_custom"
                // 3	000B	call	string [mscorlib]System.String::Concat(string, string) >>> call String.Concat(string, string)
                // 4	0010	stloc.0 >>> store result in str index 0
                )
                .ThrowIfNotMatch("Did not find the first match.")
                // Insert code the matched block
                .Advance(1) // Advance to keep going through the code or we will replace last opcode in following search
                .Insert(
                   new CodeInstruction(OpCodes.Ldarg_0), // Load 'this'
                   new CodeInstruction(OpCodes.Call, getPoqItemId), // Call the method
                   new CodeInstruction(OpCodes.Stloc_0) // Store result in str index 0 effectively replacing it
                                                        //new CodeInstruction(OpCodes.Ldloc_0),
                                                        //new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(UnityEngine.Debug), "Log", new[] { typeof(object) })) // Log the result
                )
                .Advance(1) // Advance to keep going through the code
                .MatchEndForward(
                    // Match opcode start for line
                    // ItemTransformationRecord record = Data.ItemTransformation.GetRecord(project.DevelopId, true);
                    new CodeMatch(OpCodes.Call),
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(
                        typeof(MGSC.MagnumProject),
                        nameof(MGSC.MagnumProject.DevelopId))
                    ),
                    new CodeMatch(OpCodes.Ldc_I4_1),
                    new CodeMatch(OpCodes.Callvirt),
                    new CodeMatch(OpCodes.Stloc_1)
                    )
                .ThrowIfNotMatch("Did not find the second match.")
                .Advance(1) // Advance to keep going through the code or we will replace last opcode in following search
                .Insert(
                    new CodeInstruction(OpCodes.Ldloc_1),
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Call, getItemTransformationRecord),
                    new CodeInstruction(OpCodes.Stloc_1)
                )
                .InstructionEnumeration()
                .ToList();

                return result;
            }
        }

        [HarmonyPatch(typeof(MagnumDevelopmentSystem), nameof(MagnumDevelopmentSystem.InjectItemRecord))]
        public static class MagnumDevelopmentSystems_InjectItemRecord_Patch
        {
            public static void Postfix(MagnumProject project)
            {
                // Pickup the project since we already got item record
                // Move it to our dict too

                // As we have some leftovers for items in previous codebase that used magnum projects we need to add appropriate record to new system
                var itemId = MetadataWrapper.GetPoqItemIdFromProject(project);

                if (!RecordCollection.MetadataWrapperRecords.TryGetValue(itemId, out MetadataWrapper wrapper))
                {
                    // We need to add our records
                    wrapper = new MetadataWrapper(project);

                    if (wrapper.PoqItem)
                    {
                        Plugin.Logger.Log($"MagnumDevelopmentSystems_InjectItemRecord_Patch");
                        Plugin.Logger.Log($"itemId GetPoqItemIdFromProject: {itemId}");

                        // Since record already injected
                        itemId = wrapper.ReturnItemUid();

                        Plugin.Logger.Log($"itemId ReturnItemUid: {itemId}");
                        if (!wrapper.SerializedStorage)
                        {
                            // Since we no longer use boostedParam in ticks metadata and relay on MetaData, we need to migrate this as well.
                            var boostedParam = DigitInfo.GetBoostedParam(wrapper.FinishTime.Ticks);
                            if (boostedParam != 99)
                            {
                                _logger.Log($"\t boostedParam: {boostedParam} for {itemId}");
                                wrapper.BoostedString = RaritySystem.ParamIdentifiers[boostedParam];
                            }
                        }

                        var record = Data.Items.GetRecord(itemId) as CompositeItemRecord;
                        RecordCollection.ItemRecords.Add(itemId, record);
                        RecordCollection.MetadataWrapperRecords.Add(itemId, wrapper);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(MagnumDevelopmentSystem), nameof(MagnumDevelopmentSystem.InjectProjectRecords))]
        public static class MagnumDevelopmentSystems_InjectItemRecords_Patch
        {
            public static void Postfix(MagnumProjects projects)
            {
                Plugin.Logger.Log($"MagnumDevelopmentSystems_InjectItemRecords_Patch");
                Plugin.Logger.Log($"magnumProjects Count: {projects.Values.Count}");

                // Add our own item records
                PathOfQuasimorph.magnumProjectsController.AddItemRecords(DataSerializerHelper._jsonSettingsPoq);
                PathOfQuasimorph.synthraformerController.AddItems();
            }
        }
    }
}
