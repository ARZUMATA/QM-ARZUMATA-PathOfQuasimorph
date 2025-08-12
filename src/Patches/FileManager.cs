using HarmonyLib;
using MGSC;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace QM_PathOfQuasimorph.Core
{
    internal partial class PathOfQuasimorph
    {
        [HarmonyPatch(typeof(FileManager), "SaveFile", new Type[]
        {
            typeof(string),
            typeof(string) }
        )]
        public static class FileManager_SaveFile_Patch
        {
            public static bool Prefix(FileManager __instance, string filename, string data)
            {
                // This is temp fix
                data = data.Replace("_custom_custom_poq", "_custom_poq");
                return true;
            }
        }


        [HarmonyPatch(typeof(FileManager), "LoadTextFile", new Type[]
        {
            typeof(string)}
       )]
        public static class FileManager_LoadTextFile_Patch
        {
            public static void Postfix(FileManager __instance, ref string __result, string filename)
            {
                // This is temp fix
                __result = __result.Replace("_custom_custom_poq", "_custom_poq");
            }
        }
    }
}
