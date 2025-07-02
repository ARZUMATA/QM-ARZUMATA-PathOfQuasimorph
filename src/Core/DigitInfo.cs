using System.IO.Ports;

namespace QM_PathOfQuasimorph.Core
{
    internal partial class MagnumPoQProjectsController
    {
        public class DigitInfo
        {
            // I don't like how this works. Byte array seems much better. But it works so.
            public string LeftPart { get; set; }
            public int UnusedData { get; set; } // X
            public int RandomizedPrefix { get; set; } // XX
            public int BoostedParam { get; set; } // XX
            public int Rarity { get; set; } // X

            public DigitInfo(string leftPart, int unusedData, int boostedParam, int randomizedPrefix, int rarity)
            {
                // Use last 6 digits of as identifier
                LeftPart = leftPart;
                UnusedData = unusedData;
                RandomizedPrefix = randomizedPrefix;
                BoostedParam = boostedParam;
                Rarity = rarity; // ItemRarity
            }

            public void FillZeroes()
            {
                UnusedData = 0;
                BoostedParam = 0;
                RandomizedPrefix = 0;
                Rarity = 0;
            }

            public string ReturnUID()
            {
                return $"{LeftPart}{UnusedData:D1}{BoostedParam:D2}{RandomizedPrefix:D2}{Rarity:D1}";
            }

            public static DigitInfo GetDigits(long uid)
            {
                // Convert to string
                string uidStr = uid.ToString();

                // Extract the left part (all digits except the last six)
                string leftPart = uidStr.Substring(0, uidStr.Length - 6);

                // Extract last six digits as a substring
                string lastSixDigits = uidStr.Substring(uidStr.Length - 6);

                // Structure
                // Parse each digit into integers

                int unusedData = int.Parse(lastSixDigits.Substring(0, 1));
                int boostedParam = int.Parse(lastSixDigits.Substring(1, 2));
                int randomizedPrefix = int.Parse(lastSixDigits.Substring(3, 2));
                int rarity = int.Parse(lastSixDigits.Substring(5, 1));

                return new DigitInfo(leftPart, unusedData, boostedParam, randomizedPrefix, rarity);
            }
        }
    }
}
