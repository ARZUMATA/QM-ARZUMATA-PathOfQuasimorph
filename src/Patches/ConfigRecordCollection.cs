using HarmonyLib;
using MGSC;
using QM_PathOfQuasimorph.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using static UnityEngine.UI.Image;
using Type = System.Type;

namespace QM_PathOfQuasimorph.Core
{
    internal partial class PathOfQuasimorph
    {
        [HarmonyPatch]

        public static class ConfigRecordCollectionGetRecord_Patch
        {
            public static MethodBase TargetMethod()
            {
                var collectionType = typeof(ItemsCollection).BaseType; // = ConfigRecordCollection<BasePickupItemRecord>
                return AccessTools.Method(collectionType, "GetRecord", new Type[] { typeof(string), typeof(bool) });
            }

            public static bool Prefix(ref string id, bool ignoreLog = true)
            {
                if (id.Contains("synthraformer_poq"))
                {
                    id = SynthraformerController.FixOldId(id);
                }

                return true;
            }
        }
    }
}
