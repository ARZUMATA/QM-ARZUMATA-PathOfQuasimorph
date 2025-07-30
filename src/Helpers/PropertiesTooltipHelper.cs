using MGSC;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Color = UnityEngine.Color;

namespace QM_PathOfQuasimorph.PoqHelpers
{
    internal class PropertiesTooltipHelper
    {
        internal static void SetCaption1(PropertiesTooltip _tooltip, string localizedCaption, Color firstLetterColor, string color)
        {
            var firstLetter = localizedCaption.Substring(0, 1);
            var restOfString = $"<color={color}>{localizedCaption.Substring(1)}</color>";
            var firstLetterColored = firstLetter.ColorFirstLetter(firstLetterColor);

            Console.WriteLine($"{firstLetterColored}{restOfString}");
            _tooltip._caption1.text = $"{firstLetterColored}{restOfString}";
            Localization.ActualizeFontAndSize(_tooltip._caption1, TextContext.TooltipCaption);
        }

        internal static void SetCaption2(PropertiesTooltip _tooltip, string localizedCaption, string color)
        {
            var coloredString = $"<color={color}>{localizedCaption.FirstLetterToUpperCase()}</color>";
            _tooltip._caption2.text = coloredString;
            Localization.ActualizeFontAndSize(_tooltip._caption2);
        }
    }
}
