using HarmonyLib;
using MGSC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace QM_PathOfQuasimorph.Patches
{
    internal class PathOfQuasimorph
    {
        static GameObject statsButton_miw = null;
        static GameObject statsPage_miw;
        static Transform background_miw;
        static Transform character_miw;
        static InspectWindowHeaderButton statsButtonComponent_miw;
        private static GameObject scrollView_miw;
        private static GameObject commonScrollBar_miw;
        private static GameObject classIcon_miw;
        private static Mercenary mercenary;

        private static Dictionary<string, object> AllReferences = new Dictionary<string, object>
        {
            { "StatsButton", statsButton_miw },
            { "StatsPage", statsPage_miw },
            { "Background", background_miw },
            { "Character", character_miw },
            { "ScrollView", scrollView_miw },
            { "CommonScrollBar", commonScrollBar_miw },
            { "ClassIcon", classIcon_miw },
            { "StatsButtonComponent", statsButtonComponent_miw }
        };

        [HarmonyPatch(typeof(MonsterInspectWindow), "BackpackButtonOnSelected")]
        public static class MonsterInspectWindow_BackpackButtonOnSelected_Patch
        {
            public static void Postfix(MonsterInspectWindow __instance)
            {
                Plugin.Logger.Log($"MonsterInspectWindow_BackpackButtonOnSelected_Patch");

            }
        }

        [HarmonyPatch(typeof(MonsterInspectWindow), "OnEnable")]
        public static class MonsterInspectWindow_OnEnable_Patch
        {
            public static void Postfix(MonsterInspectWindow __instance)
            {
                Plugin.Logger.Log($"MonsterInspectWindow_OnEnable_Patch");

            }

        }
        [HarmonyPatch(typeof(MonsterInspectWindow), "Configure")]
        public static class MonsterInspectWindow_Configure_Patch
        {

            public static bool Prefix(MonsterInspectWindow __instance, Creature creature)
            {
                return true;
            }

            public static void Postfix(MonsterInspectWindow __instance, Creature creature)
            {
                Plugin.Logger.Log($"MonsterInspectWindow_Configure_Patch");
                Plugin.Logger.Log($"{__instance.transform.name}");

                // We check for stats button, if it's missing then we add it and all relevant info.
                // We do it once as it's a one time thing.
                if (statsButton_miw == null)
                {
                    ConfigureMiw(__instance);
                }

                /* Hierarchy
                 * MonsterInspectWindow
                 *   Content
                 *     Background
                 *       BackpackPage
                 *         Character
                 *       StatsPage (new)
                 *     Buttons
                */

                InitializePerkSlots(__instance, creature);
            }

            private static void ConfigureMiw(MonsterInspectWindow __instance)
            {
                var content_miw = __instance.transform.Find("Content");
                Plugin.Logger.Log($"content_miw is null: {content_miw == null}");

                if (content_miw != null)
                {

                    // Find the Background and Buttons
                    Plugin.Logger.Log($"background_miw is null: {background_miw == null}");
                    background_miw = content_miw.transform.Find("Background");

                    if (background_miw != null)
                    {
                        var backpackPage_miw = background_miw.transform.Find("BackpackPage");
                        //character_miw = backpackPage_miw.transform.Find("Character");

                        Plugin.Logger.Log($"backpackPage_miw is null: {backpackPage_miw == null}");
                        Plugin.Logger.Log($"character_miw is null: {character_miw == null}");

                        // Find button make copy of it, create stuff
                        var buttons_miw = background_miw.Find("Buttons");

                        Plugin.Logger.Log($"buttons_miw is null: {buttons_miw == null}");

                        if (buttons_miw != null)
                        {

                            // Create new StatsPage
                            statsPage_miw = new GameObject("StatsPage");
                            RectTransform statsPage_rt = statsPage_miw.AddComponent<RectTransform>();
                            // statsPage_miw.transform.SetParent(background_miw.transform, false);
                            statsPage_rt.SetParent(background_miw.transform, false);

                            // Copy rt data from backpack page rt
                            if (backpackPage_miw != null && backpackPage_miw.GetComponent<RectTransform>() != null)
                            {
                                RectTransform backpackPage_rt = backpackPage_miw.GetComponent<RectTransform>();
                                statsPage_rt.anchorMin = backpackPage_rt.anchorMin;
                                statsPage_rt.anchorMax = backpackPage_rt.anchorMax;
                                statsPage_rt.pivot = backpackPage_rt.pivot;
                                statsPage_rt.offsetMin = backpackPage_rt.offsetMin;
                                statsPage_rt.offsetMax = backpackPage_rt.offsetMax;
                                statsPage_rt.sizeDelta = backpackPage_rt.sizeDelta;
                                statsPage_rt.anchoredPosition = backpackPage_rt.anchoredPosition;
                                statsPage_rt.localScale = Vector3.one;
                            }

                            // Find ItemsButton and BodyPartsButton
                            var itemsButton_miw = buttons_miw.Find("ItemsButton");
                            var bodyPartsButton_miw = buttons_miw.Find("BodyPartsButton");
                            var itemsButtonComponent_miw = itemsButton_miw.GetComponent<InspectWindowHeaderButton>();
                            var bodyPartsButtonnComponent_miw = bodyPartsButton_miw.GetComponent<InspectWindowHeaderButton>();

                            Plugin.Logger.Log($"itemsButton_miw is null: {itemsButton_miw == null}");
                            if (itemsButton_miw != null)
                            {
                                // Clone the button and rename it to StatsButton
                                statsButton_miw = GameObject.Instantiate(itemsButton_miw.gameObject, itemsButton_miw.parent);
                                statsButton_miw.name = "StatsButton";

                                // Add event handlers
                                statsButtonComponent_miw = statsButton_miw.GetComponent<InspectWindowHeaderButton>();

                                Plugin.Logger.Log($"statsButtonComponent_miw is null: {statsButtonComponent_miw == null}");

                                if (statsButtonComponent_miw != null)
                                {
                                    // Add existing buttons to list (dupes, but not that big of a deal)
                                    statsButtonComponent_miw._otherButtons.Add(itemsButtonComponent_miw);
                                    statsButtonComponent_miw._otherButtons.Add(bodyPartsButtonnComponent_miw);

                                    // Let other buttons know about this button
                                    itemsButtonComponent_miw._otherButtons.Add(statsButtonComponent_miw);
                                    bodyPartsButtonnComponent_miw._otherButtons.Add(statsButtonComponent_miw);

                                    // Add related page and event handlers
                                    statsButtonComponent_miw._relatedPage = statsPage_miw;
                                    statsButtonComponent_miw.OnSelected -= () => __instance.BackpackButtonOnSelected();
                                    statsButtonComponent_miw.OnUnselected -= () => __instance.BackpackButtonOnUnselected();
                                    statsButtonComponent_miw.OnSelected += () => StatsOnSelected(__instance);
                                    statsButtonComponent_miw.OnUnselected += () => StatsOnUnselected(__instance);

                                    // Replace sprites
                                    Sprite activeIcon = Helpers.LoadSpriteFromEmbeddedBundle("QM_PathOfQuasimorph.Files.AssetBundles.pathofquasimorph", "statsandperksTabIcon_on");
                                    Sprite inactiveIcon = Helpers.LoadSpriteFromEmbeddedBundle("QM_PathOfQuasimorph.Files.AssetBundles.pathofquasimorph", "statsandperksTabIcon_off");
                                    Plugin.Logger.Log($"activeIcon is null: {activeIcon == null}");
                                    Plugin.Logger.Log($"activeIcon is null: {activeIcon == null}");

                                    if (activeIcon != null)
                                    {
                                        statsButtonComponent_miw._activeIconSprite = activeIcon;
                                    }

                                    if (inactiveIcon != null)
                                    {
                                        statsButtonComponent_miw._inactiveIconSprite = inactiveIcon;

                                    }

                                    // We need some objects from mercenary class screen
                                    FindMercenaryClassScreen();
                                }
                            }

                            // Position viewport
                            //var viewport_miw = scrollView_miw.transform.Find("Viewport");
                            //var viewport_rt = viewport_miw.GetComponent<RectTransform>();
                            //RectTransform statsButton_rt = statsPage_miw.GetComponent<RectTransform>();
                            //Vector2 viewportAnchoredPos = viewport_rt.anchoredPosition;
                            //viewportAnchoredPos.y = -26;
                            //viewport_rt.anchoredPosition = viewportAnchoredPos;

                            // Connect slider bar
                            //var slidingArea_miw_rt = commonScrollBar_miw.transform.Find("Sliding Area").GetComponent<RectTransform>();
                            //var handle_miw_rt = slidingArea_miw_rt.Find("Handle").GetComponent<RectTransform>();
                            //var scrollbarCommonScrollBarComponent = commonScrollBar_miw.GetComponent<CommonScrollBar>();
                            //scrollbarCommonScrollBarComponent._containerRect = slidingArea_miw_rt;
                            //scrollbarCommonScrollBarComponent._handleRect = handle_miw_rt;
                            //scrollbarCommonScrollBarComponent._scrollRect = scrollView_miw.GetComponent<ScrollRect>();

                            // Find new sprite
                            var classIconImage_miw = classIcon_miw.GetComponent<UnityEngine.UI.Image>();
                            //classIconImage_miw.color = new Color(1, 0, 0, 0.25f); // Red
                            //classIconImage_miw.color = Helpers.HexStringToUnityColor("#380714", 64);
                            //classIconImage_miw.color = Helpers.HexStringToUnityColor("#260B0B", 64);
                            classIconImage_miw.color = new Color(0.709f, 0.1231f, 0.2831f, 0.151f);

                            //var newSprite = Helpers.FindSpriteByName("Pirates_factionIcon_mouseOut");
                            //classIconImage_miw.sprite = newSprite;

                            Sprite newSprite = Helpers.LoadSpriteFromEmbeddedBundle("QM_PathOfQuasimorph.Files.AssetBundles.pathofquasimorph", "classIcon_Predator");
                            classIconImage_miw.sprite = newSprite;

                            // Move it behind in hierarchy
                            classIcon_miw.transform.SetSiblingIndex(0);

                            // Reposition perk slots

                            //List<PerkSlot> perkSlots = new List<PerkSlot>();
                            //foreach (var perkSlot in classIcon_miw.GetComponentsInChildren<PerkSlot>())
                            //{
                            //    //perkSlots.Add(perkSlot);

                            //}


                            //Vector2 perkSlotAnchrPos = new Vector2(46, 46);

                            // Reposition perk slots
                            // Sort perkSlots if needed, based on their current order or any other criteria
                            // perkSlots = perkSlots.OrderBy(p => p.transform.GetSiblingIndex()).ToList();
                            // 
                            // Vector2 startingAnchoredPosition = new Vector2(46, 46);
                            // 
                            // for (int i = 0; i < perkSlots.Count; i++)
                            // {
                            //     PerkSlot perkSlot = perkSlots[i];
                            //     RectTransform rectTransform = perkSlot.GetComponent<RectTransform>();
                            // 
                            //     if (rectTransform == null)
                            //     {
                            //         continue; // skip if no RectTransform found
                            //     }
                            // 
                            //     float slotHeight = rectTransform.rect.height;
                            // 
                            //     Vector2 newPosition = new Vector2(startingAnchoredPosition.x, startingAnchoredPosition.y - i * slotHeight);
                            // 
                            //     rectTransform.anchoredPosition = newPosition;
                            // }


                        }
                    }
                }
            }

            private static void InitializePerkSlots(MonsterInspectWindow __instance, Creature creature)
            {
                mercenary = __instance._creatures.Player.Mercenary;

                // Get all components including disabled objects (true argument)
                List<PerkSlot> perkSlots = classIcon_miw.GetComponentsInChildren<PerkSlot>(true).ToList();

                if (creature.CreatureData.Perks.Count == 0)
                {
                    foreach (var perkSlot in perkSlots)
                    {
                        perkSlot.gameObject.SetActive(false);
                    }

                    return;
                }

                Plugin.Logger.Log($"creature.CreatureData.Perks.Count: {creature.CreatureData.Perks.Count}");
                Plugin.Logger.Log($"perkSlots.Count: {perkSlots.Count}");

                // Init talent perk
                //perkSlots[0].Initialize(mercenary, creature.CreatureData.Perks[0], Data.Perks.GetRecord(creature.CreatureData.Perks[0].PerkId, true));

                // Add rank perk

                // Add ultimate perk
                perkSlots[0].Initialize(mercenary, creature.CreatureData.Perks[1], Data.Perks.GetRecord(creature.CreatureData.Perks[1].PerkId, true));

                //if (creature.CreatureData.Perks.Count <= perkSlots.Count)
                {
                    int b = 2; // We skip first object, as talent and second as rank, third is ultimate but we dont use it
                    for (int i = 0; i < perkSlots.Count; i++)
                    {
                        if (i == 0)
                        {
                            if (creature.CreatureData.Perks[i].PerkId != "talent_the_man_who_sold_the_world")
                            {
                                perkSlots[i].gameObject.SetActive(true);

                            }
                            else
                            {
                                perkSlots[i].gameObject.SetActive(false);
                            }

                            continue;
                        }

                        if (b >= creature.CreatureData.Perks.Count)
                        {
                            break; // Safe exit if index is out of range
                        }

                        perkSlots[i].Initialize(mercenary, creature.CreatureData.Perks[b], Data.Perks.GetRecord(creature.CreatureData.Perks[b].PerkId, true));
                        perkSlots[i].gameObject.SetActive(true);
                        b++;
                    }
                }
            }
        }

        private static void FindMercenaryClassScreen()
        {
            var mercenaryClassScreen = GameObject.FindObjectOfType<MercenaryClassScreen>(true);

            Plugin.Logger.Log($"mercenaryClassScreen is null: {mercenaryClassScreen == null}");

            if (mercenaryClassScreen != null)
            {
                // Find the MercenaryClassWindow
                var mercenaryClassWindow = mercenaryClassScreen.transform.Find("MercenaryClassWindow");

                Plugin.Logger.Log($"mercenaryClassWindow is null {mercenaryClassWindow == null}");

                if (mercenaryClassWindow != null)
                {
                    // Navigate to the desired elements
                    var background_mcs = mercenaryClassWindow.transform.Find("Background");
                    var scrollview_mcs = background_mcs.transform.Find("Scroll View");
                    var commonscrollbar_mcs = background_mcs.transform.Find("CommonScrollBar");
                    var classicon_mcs = background_mcs.transform.Find("ClassIcon");

                    Plugin.Logger.Log($"background_mcs is null {background_mcs == null}");
                    Plugin.Logger.Log($"scrollview_mcs is null {scrollview_mcs == null}");
                    Plugin.Logger.Log($"commonscrollbar_mcs is null {commonscrollbar_mcs == null}");
                    Plugin.Logger.Log($"classicon_mcs is null {classicon_mcs == null}");

                    // Clone and reparent the found objects under StatsPage
                    //if (character_miw != null)
                    //{
                    //    GameObject characterClone = GameObject.Instantiate(character_miw.gameObject, statsPage_miw.transform);
                    //    characterClone.name = "Character";
                    //}

                    //if (scrollview_mcs != null)
                    //{
                    //    scrollView_miw = GameObject.Instantiate(scrollview_mcs.gameObject, statsPage_miw.transform);
                    //    scrollView_miw.name = "ScrollView";
                    //}

                    //if (commonscrollbar_mcs != null)
                    //{
                    //    commonScrollBar_miw = GameObject.Instantiate(commonscrollbar_mcs.gameObject, statsPage_miw.transform);
                    //    commonScrollBar_miw.name = "CommonScrollbar";
                    //}

                    if (classicon_mcs != null)
                    {
                        classIcon_miw = GameObject.Instantiate(classicon_mcs.gameObject, statsPage_miw.transform);
                        classIcon_miw.name = "ClassIcon";
                    }
                }
            }
        }

        // Event handler methods
        private static void StatsOnSelected(MonsterInspectWindow __instance)
        {
            __instance._currentPage = MonsterInspectWindow.PageType.None;

            // Handle selected event
            Plugin.Logger.Log($"StatsOnSelected");

            if (statsPage_miw != null)
            {
                Plugin.Logger.Log($"statsPage_miw.SetActive(true)");

                statsPage_miw.SetActive(true);
            }
        }

        private static void StatsOnUnselected(MonsterInspectWindow __instance)
        {
            // Handle unselected event
            statsPage_miw.SetActive(false);
            Plugin.Logger.Log($"StatsOnUnselected");
        }
    }
}
