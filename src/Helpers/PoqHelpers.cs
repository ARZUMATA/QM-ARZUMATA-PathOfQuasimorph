using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QM_PathOfQuasimorph.PoqHelpers
{
    internal class PoqHelpers
    {
        private static readonly string[] BodyParts =
        {
            "Shoulder", "Stomach", "Thigh", "Chest", "Feet", "Head", "Arm", "Knee", "Tail", "Body"
        };

        public static (string baseId, string bodyPart) StripBodyPart(string input)
        {
            foreach (var part in BodyParts)
            {
                if (input.EndsWith(part, StringComparison.OrdinalIgnoreCase))
                {
                    var baseId = input.Substring(0, input.Length - part.Length);
                    return (baseId, part);
                }
            }
            return (input, null); // No match
        }

    }
}
