using HarmonyLib;
using MGSC;
using QM_PathOfQuasimorph.Core.Processors;
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
        internal static ItemRecordsControllerPoq itemRecordsControllerPoq = new ItemRecordsControllerPoq();
        private static TooltipGeneratorPoq tooltipGeneratorPoq = new TooltipGeneratorPoq();
        internal static AmplifierController amplifierController = new AmplifierController();
        public static RaritySystem raritySystem = new RaritySystem();
        internal static DungeonGameMode dungeonGameMode = null;
        internal static GameCamera gameCamera = null;
        internal static Camera camera = null;
        internal static PixelizatorCameraAttachment pixelizatorCameraAttachment = null;
        private static Logger _logger = new Logger(null, typeof(PathOfQuasimorph));
        public static IModContext _context;

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
            //Logger._excludedTypes.Add(typeof(CleanupSystem));
            //Logger._excludedTypes.Add(typeof(TooltipGeneratorPoq));
            //Logger._excludedTypes.Add(typeof(MagnumPoQProjectsController));
            //Logger._excludedTypes.Add(typeof(AffixManager));
            //Logger._excludedTypes.Add(typeof(RaritySystem));
            //Logger._excludedTypes.Add(typeof(ItemRecordsControllerPoq));
            //Logger._excludedTypes.Add(typeof(CreaturesControllerPoq));
            //Logger._excludedTypes.Add(typeof(RecordCollection));

            // Record processors
            //Logger._excludedTypes.Add(typeof(ArmorRecordProcessorPoq));
            //Logger._excludedTypes.Add(typeof(AugmentationRecordProcessorPoq));
            //Logger._excludedTypes.Add(typeof(BootsRecordProcessorPoq));
            //Logger._excludedTypes.Add(typeof(HelmetRecordProcessorPoq));
            //Logger._excludedTypes.Add(typeof(ImplantRecordProcessorPoq));
            //Logger._excludedTypes.Add(typeof(LeggingsRecordProcessorPoq));
            //Logger._excludedTypes.Add(typeof(WeaponRecordProcessorPoq));
            //Logger._excludedTypes.Add(typeof(WoundSlotRecordProcessorPoq));

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
            _logger.Log($"AfterSpaceLoaded");
        }

        [Hook(ModHookType.MainMenuStarted)]
        public static void MainMenuStarted(IModContext context)
        {
            _context = context;
            _logger.Log($"MainMenuStarted");
            perkFactoryState = context.State.Get<PerkFactory>();
        }

        [Hook(ModHookType.MainMenuDestroyed)]
        public static void MainMenuDestroyed(IModContext context)
        {
            _logger.Log($"MainMenuDestroyed");
            ApplyModConfigs();
        }
        private static void ApplyModConfigs()
        {
            raritySystem.ApplyColors();
            creaturesControllerPoq.ApplyColors();
            tooltipGeneratorPoq.ApplyColors();
        }

        [Hook(ModHookType.DungeonStarted)]
        public static void DungeonStarted(IModContext context)
        {
            _logger.Log($"DungeonStarted");
            Initialize();
        }

        [Hook(ModHookType.SpaceStarted)]
        public static void SpaceStarted(IModContext context)
        {
            _logger.Log($"SpaceStarted");
            CleanupSystem.CleanObsoleteProjects(context, true);
            creaturesControllerPoq.CleanCreatureDataPoq();
        }

        [Hook(ModHookType.DungeonFinished)]
        public static void DungeonFinished(IModContext context)
        {
            _logger.Log($"DungeonFinished");
            //CleanupSystem.CleanObsoleteProjects(context, true); // Can't clean here as it triggers on every floor change
            isInitialized = false;
            dungeonGameMode = null;
            gameCamera = null;
            camera = null;
            pixelizatorCameraAttachment = null;
        }
    }
}