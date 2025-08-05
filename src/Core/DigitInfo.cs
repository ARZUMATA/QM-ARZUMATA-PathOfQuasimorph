using System;
using System.Runtime.CompilerServices;

namespace QM_PathOfQuasimorph.Core
{
    public class DigitInfo
    {
        private int _serializedStorage;
        public string LeftPart { get; set; }
        public int SerializedStorage // X (0 or 1)
        {
            get => _serializedStorage;
            set => _serializedStorage = value;
        }
        public int UnusedData2 { get; set; } // XX
        public int BoostedParam { get; set; } // XX
        public int Rarity { get; set; } // X

        public bool IsSerialized
        {
            get => _serializedStorage == 1;
            set => _serializedStorage = value ? 1 : 0;
        }

        public DigitInfo(string leftPart, int serialized, int boostedParam, int unusedData2, int rarity)
        {
            LeftPart = leftPart;
            IsSerialized = serialized == 1;
            BoostedParam = boostedParam;
            UnusedData2 = unusedData2;
            Rarity = rarity;
        }

        public void FillZeroes()
        {
            IsSerialized = false;
            BoostedParam = 0;
            UnusedData2 = 0;
            Rarity = 0;
        }

        public string ReturnUID()
        {
            return $"{LeftPart}{SerializedStorage:D1}{BoostedParam:D2}{UnusedData2:D2}{Rarity:D1}";
        }

        public static DigitInfo GetRandomDigits()
        {
            return GetDigits(Helpers.UniqueIDGenerator.GenerateRandomIDWith16Characters());
        }

        // Safe, low-allocation parser
        public static DigitInfo GetDigits(long ticks)
        {
            string ticksStr = ticks.ToString();

            // If less than 6 digits, can't extract metadata
            if (ticksStr.Length < 6)
            {
                return new DigitInfo(ticksStr, 0, 0, 0, 0);
            }

            string leftPart = ticksStr.Substring(0, ticksStr.Length - 6);

            // Extract last 6 digits as substring
            string last6 = ticksStr.Substring(ticksStr.Length - 6);

            // Parse each part safely
            int serialized = ParseDigit(last6[0]);          // 1st of last 6
            int boostedParam = ParseInt(last6, 1, 2);       // 2nd + 3rd
            int unusedData2 = ParseInt(last6, 3, 2);        // 4th + 5th
            int rarity = ParseDigit(last6[5]);              // 6th

            return new DigitInfo(leftPart, serialized, boostedParam, unusedData2, rarity);
        }

        // Helper: parse N-length int from string starting at index
        private static int ParseInt(string s, int startIndex, int length)
        {
            int result = 0;
            int endIndex = startIndex + length;

            for (int i = startIndex; i < endIndex; i++)
            {
                result = result * 10 + (s[i] - '0');
            }

            return result;
        }

        // Fast field extractors (no object creation)

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsSerializedStorage(long ticks)
        {
            return GetSerializedFlag(ticks) == 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetSerializedFlag(long ticks)
        {
            return (int)((ticks / 100000) % 10); // 6th from end
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetBoostedParam(long ticks)
        {
            return (int)((ticks / 1000) % 100); // 3rd and 4th from end
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetUnusedData2(long ticks)
        {
            return (int)((ticks / 10) % 100); // 2nd from end (two digits)
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetRarity(long ticks)
        {
            return (int)(ticks % 10); // Last digit
        }

        // Helper to just get ItemRarity enum
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ItemRarity GetRarityClass(long ticks)
        {
            var rarityDigit = GetRarity(ticks);
            return (ItemRarity)rarityDigit;
        }

        // Fast digit parser (safe fallback)
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int ParseDigit(char c)
        {
            return c >= '0' && c <= '9' ? c - '0' : 0;
        }
    }
}