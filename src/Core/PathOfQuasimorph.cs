using HarmonyLib;
using MGSC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Policy;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;

namespace QM_PathOfQuasimorph.Core
{
    internal partial class PathOfQuasimorph
    {
        private static bool isInitialized = false;
        private static bool enabled = false;
        private static MagnumPoQProjectsController magnumProjectsController;
        private static bool done = false;

        public static void UpdateKey(string lookupstr, string prefix, string suffix)
        {
            foreach (KeyValuePair<Localization.Lang, Dictionary<string, string>> languageToDict in Singleton<Localization>.Instance.db)
            {
                if (languageToDict.Value.ContainsKey(lookupstr))
                {
                    languageToDict.Value[lookupstr] = prefix + languageToDict.Value[lookupstr] + suffix;
                }
            }
        }

        /* All magnum project are recipes that are always available in the game. You get access to exact recipe via chip.
         * Mod projects are just derivatives from that
         * By game design all items are either project or generic.
         * Postfix _custom used create item records procedurally, these records are required for magnum projects modifications.
         * New project has same postfix but replaces old one if updated.
         * They create procedural record, then modify it via reflection, so for entire game it’s valid record.
         * 
         * This is where we intercept that logic and create our items.
         */

    }
}