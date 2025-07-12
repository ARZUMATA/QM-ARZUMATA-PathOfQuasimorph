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
    internal static partial class PathOfQuasimorph
    {
        private static bool isInitialized = false;
        private static bool enabled = false;
        internal static MagnumPoQProjectsController magnumProjectsController;
        internal static CreaturesControllerPoq creaturesControllerPoq = new CreaturesControllerPoq();
        private static TooltipGeneratorPoq tooltipGeneratorPoq = new TooltipGeneratorPoq();
        public static RaritySystem raritySystem = new RaritySystem();
        internal static DungeonGameMode dungeonGameMode = null;
        internal static GameCamera gameCamera = null;
        internal static Camera camera = null;
        internal static PixelizatorCameraAttachment pixelizatorCameraAttachment = null;

        public static PerkFactory perkFactoryState { get; private set; }

        /* All magnum project are recipes that are always available in the game. You get access to exact recipe via chip.
        * Mod projects are just derivatives from that
        * By game design all items are either project or generic.
        * Postfix _custom used create item records procedurally, these records are required for magnum projects modifications.
        * New project has same postfix but replaces old one if updated.
        * They create procedural record, then modify it via reflection, so for entire game it’s valid record.
        * 
        * This is where we intercept that logic and create our items.
        */

        // Logging exclusions
        static PathOfQuasimorph()
        {
            // Predefined types to exclude
            Logger._excludedTypes.Add(typeof(CleanupSystem));
            Logger._excludedTypes.Add(typeof(TooltipGeneratorPoq));
            Logger._excludedTypes.Add(typeof(MagnumPoQProjectsController));
            Logger._excludedTypes.Add(typeof(AffixManager));
            Logger._excludedTypes.Add(typeof(RaritySystem));
        }

        public static void Initialize()
        {
            if (!isInitialized)
            {
                if (dungeonGameMode == null || gameCamera == null || camera == null)
                {
                    dungeonGameMode = GameObject.FindObjectOfType<DungeonGameMode>(true);
                    gameCamera = dungeonGameMode._camera;
                    camera = dungeonGameMode._camera.GetComponent<Camera>();

                    if (pixelizatorCameraAttachment == null)
                    {
                        pixelizatorCameraAttachment = camera.GetComponent<PixelizatorCameraAttachment>();
                    }
                }
            }
        }

        [Hook(ModHookType.AfterSpaceLoaded)]
        public static void CleanupModeAfterSpaceLoaded(IModContext context)
        {
            CleanupSystem.CleanObsoleteProjects(context);
        }

        [Hook(ModHookType.MainMenuStarted)]
        public static void BeforeDungeonLoaded(IModContext context)
        {
            perkFactoryState = context.State.Get<PerkFactory>();
        }

        [Hook(ModHookType.DungeonStarted)]
        public static void DungeonStarted(IModContext context)
        {
            Initialize();
        }

        [Hook(ModHookType.DungeonFinished)]
        public static void CleanupModeDungeonFinished(IModContext context)
        {
            CleanupSystem.CleanObsoleteProjects(context, true);
            isInitialized = false;
            dungeonGameMode = null;
            gameCamera = null;
            camera = null;
            pixelizatorCameraAttachment = null;
        }
    }
}