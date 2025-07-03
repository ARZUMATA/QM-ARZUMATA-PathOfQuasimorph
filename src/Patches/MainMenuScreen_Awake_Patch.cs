using HarmonyLib;
using MGSC;
using QM_PathOfQuasimorph.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QM_PathOfQuasimorph.Core
{
    internal partial class PathOfQuasimorph
    {
        [HarmonyPatch(typeof(MainMenuScreen), "Awake")]
        public static class MainMenuScreen_Awake_Patch
        {
            public static void Postfix()
            {
                LocalizationHelpers.LocadlocalizationData();
            }
        }
    }
}
