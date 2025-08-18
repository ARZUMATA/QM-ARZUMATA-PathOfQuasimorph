using HarmonyLib;
using MGSC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static QM_PathOfQuasimorph.Controllers.CreaturesControllerPoq;

namespace QM_PathOfQuasimorph.Core
{
    internal partial class PathOfQuasimorph
    {
        // [HarmonyPatch(typeof(CreatureFactory), "CreateMonster", new Type[] { typeof(CreatureData) })]
        // public static class CreatureFactory_CreateMonster_Patch
        // {
        //     public static void Postfix(CreatureFactory __instance, Monster __result)
        //     {
        //         Plugin.Logger.Log($"CreatureFactory_CreateMonster_Patch");
        //     }
        // }

        // [HarmonyPatch(typeof(CreatureFactory), "CreateMonsterFromMobClass")]
        // public static class CreatureFactory_CreateMonsterFromMobClass_Patch
        // {
        //     public static void Postfix(CreatureFactory __instance, ref Monster __result)
        //     {
        //         Plugin.Logger.Log($"CreatureFactory :: CreateMonsterFromMobClass :: Patch");
        //     }
        // }
    }
}
