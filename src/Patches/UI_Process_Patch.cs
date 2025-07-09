using HarmonyLib;
using MGSC;
using System;
using TMPro;
using UnityEngine;

namespace QM_PathOfQuasimorph.Core
{
    internal partial class PathOfQuasimorph
    {
        private static GameObject gameKeyPanelPoq;
        private static GameObject hotkeyHintPoq;

        [HarmonyPatch(typeof(PropertiesTooltip), "ShowAdditionalBlock", new Type[] { })]
        public static class PropertiesTooltip_ShowAdditionalBlock_Patch
        {
            public static bool Prefix(PropertiesTooltip __instance)
            {
                if (__instance._additionalHint != null)
                {
                    var key = __instance._additionalHint.transform.Find("Key");

                    if (gameKeyPanelPoq == null)
                    {
                        var gameKeyPanel = key.transform.Find("GameKeyPanel");

                        // Clone the GameObject
                        gameKeyPanelPoq = GameObject.Instantiate(gameKeyPanel.gameObject, gameKeyPanel.parent);

                        // Our name so we can find it in inspector
                        gameKeyPanelPoq.name = "GameKeyPanelPoq";

                        // Set keyId before even it inits
                        var gameKeyPanelComponent = gameKeyPanelPoq.GetComponent<GameKeyPanel>();

                        if (gameKeyPanelComponent != null)
                        {
                            gameKeyPanelComponent._keyId = "UI_FastScroll"; // This is not quite correct as changing keybind may render it wrong.
                        }

                        // Check our hint
                        if (hotkeyHintPoq == null)
                        {
                            // HotkeyHint
                            var hotkeyHint = key.transform.Find("HotkeyHint");

                            // Clone the GameObject
                            hotkeyHintPoq = GameObject.Instantiate(hotkeyHint.gameObject, hotkeyHint.parent);
                            hotkeyHintPoq.name = "HotkeyHintPoq";

                            var hotkeyHintTextMesh = hotkeyHintPoq.transform.gameObject.GetComponent<TextMeshProUGUI>();
                            var localizableLabel = hotkeyHintPoq.transform.gameObject.GetComponent<LocalizableLabel>();
                            localizableLabel.ChangeLabel("poq.ui.tooltip.compare");
                        }
                    }
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(UI), "Process")]
        public static class UI_Process_Patch
        {
            public static void Postfix(UI __instance)
            {
                // We can do transplier patch so every time we hold SHIFT, our tooltip shows.
                TooltipGeneratorPoq.HandlePoqTooltip();
            }
        }
    }
}