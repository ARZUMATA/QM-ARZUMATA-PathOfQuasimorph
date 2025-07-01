namespace QM_PathOfQuasimorph.Core
{
    internal partial class MagnumPoQProjectsController
    {
        public class DigitInfo
        {
            public string LeftPart { get; set; }
            public int D1 { get; set; }
            public int D2 { get; set; }
            public int D3 { get; set; }
            public int D4 { get; set; }
            public int D5 { get; set; }
            public int D6_Rarity { get; set; }
            public string UID { get; set; }

            public DigitInfo(string leftPart, int d1, int d2, int d3, int d4, int d5, int d6)
            {
                // Use last 6 digits of as identifier
                LeftPart = leftPart;
                D1 = d1; // 0
                D2 = d2; // 0
                D3 = d3; // 0
                D4 = d4; // 0
                D5 = d5; // 0
                D6_Rarity = d6; // ItemRarity
            }

            public void FillZeroes()
            {
                D1 = 0;
                D2 = 0;
                D3 = 0;
                D4 = 0;
                D5 = 0;
                D6_Rarity = 0;
            }

            public string ReturnUID()
            {
                // Rebuild the six digits as a string
                string modifiedSixDigits = $"{D1}{D2}{D3}{D4}{D5}{D6_Rarity}";

                // Reconstruct the full UID using the left part and the modified six digits
                string modifiedUidStr = LeftPart + modifiedSixDigits;

                return modifiedUidStr;
            }

            public static DigitInfo GetDigits(long uid)
            {
                // Convert to string
                string uidStr = uid.ToString();

                // Extract the left part (all digits except the last six)
                string leftPart = uidStr.Substring(0, uidStr.Length - 6);

                // Extract last six digits as a substring
                string lastSixDigits = uidStr.Substring(uidStr.Length - 6);

                // Parse each digit into integers
                int d1 = int.Parse(lastSixDigits[0].ToString());
                int d2 = int.Parse(lastSixDigits[1].ToString());
                int d3 = int.Parse(lastSixDigits[2].ToString());
                int d4 = int.Parse(lastSixDigits[3].ToString());
                int d5 = int.Parse(lastSixDigits[4].ToString());
                int d6 = int.Parse(lastSixDigits[5].ToString());

                return new DigitInfo(leftPart, d1, d2, d3, d4, d5, d6);
            }
        }
    }
}
