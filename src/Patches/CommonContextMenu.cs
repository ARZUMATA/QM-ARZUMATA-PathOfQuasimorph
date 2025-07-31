using HarmonyLib;
using MGSC;
using QM_PathOfQuasimorph.Core.Records;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEngine.UI.Image;
using Type = System.Type;

namespace QM_PathOfQuasimorph.Core
{
    internal partial class PathOfQuasimorph
    {
        [HarmonyPatch(typeof(CommonContextMenu), nameof(CommonContextMenu.SetupCommand))]
        public static class CommonContextMenu_SetupCommand_Patch
        {
            public static void Postfix(CommonContextMenu __instance, int bindedVal, bool interactable)
            {
                Plugin.Logger.Log($"CommonContextMenu_SetupCommand_Patch");
                Plugin.Logger.Log($"interactable {interactable}");
                Plugin.Logger.Log($"AmplifierContext.Process {AmplifierContext.Process}");

                if (!interactable || ((ContextMenuCommand)bindedVal != ContextMenuCommand.Repair))
                {
                    return;
                }

                CommonButton commandButton = __instance._activeButtonsList.Last<CommonButton>();
                Plugin.Logger.Log($"commandButton {commandButton.name}");
                
                // Compatibility with QM_ContextMenuHotkeys by NKB_RedSpy

                if (AmplifierContext.Process)
                {
                    string localizedEnchant = Localization.Get("ui.context.poq.Amplify");
                    string temp = ExtractAndReplace(commandButton.captionText.text, localizedEnchant);
                    commandButton.captionText.text = temp;
                    return;
                }

                if (RecombinatorContext.Process)
                {
                    string localizedEnchant = Localization.Get("ui.context.poq.Recombinate");
                    string temp = ExtractAndReplace(commandButton.captionText.text, localizedEnchant);
                    commandButton.captionText.text = temp;
                    return;
                }
            }
        }

        private static string ExtractAndReplace(string input, string newText)
        {
            // Match text after optional <color> tag, up to semicolon

            var match = System.Text.RegularExpressions.Regex.Match(input, @"^(?<prefix><color=""[^""]+"">)?(?<text>[^;]+);");

            if (match.Success)
            {
                string text = match.Groups["text"].Value.Trim();
                string prefix = match.Groups["prefix"].Value;
                return $"{prefix}{newText}";
            }

            // Fallback: just return new text if parsing fails
            return $"{newText}";
        }
    }
}
