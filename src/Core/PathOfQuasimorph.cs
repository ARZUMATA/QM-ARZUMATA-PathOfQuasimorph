using HarmonyLib;
using MGSC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using static MGSC.Localization;
using static QM_PathOfQuasimorph.Core.MagnumPoQProjectsController;
using static System.Net.Mime.MediaTypeNames;

namespace QM_PathOfQuasimorph.Core
{
    internal partial class PathOfQuasimorph
    {
        private static bool isInitialized = false;
        private static bool enabled = false;
        private static MagnumPoQProjectsController magnumProjectsController;
        private static bool done = false;

        /* All magnum project are recipes that are always available in the game. You get access to exact recipe via chip.
        * Mod projects are just derivatives from that
        * By game design all items are either project or generic.
        * Postfix _custom used create item records procedurally, these records are required for magnum projects modifications.
        * New project has same postfix but replaces old one if updated.
        * They create procedural record, then modify it via reflection, so for entire game it’s valid record.
        * 
        * This is where we intercept that logic and create our items.
        */

        [Hook(ModHookType.AfterSpaceLoaded)]
        public static void CleanupModeAfterSpaceLoaded(IModContext context)
        {
            CleanupSystem.CleanObsoleteProjects(context);
        }

        [Hook(ModHookType.DungeonFinished)]
        public static void CleanupModeDungeonFinished(IModContext context)
        {
            CleanupSystem.CleanObsoleteProjects(context, true);
        }
    }
}