using System;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;
using Random = System.Random;

internal static class Helpers
{
    public static string FaceColorToHex(Color color) => $"#{color.r:F0}{color.g:F0}{color.b:F0}";

    public static string AlphaAwareColorToHex(Color color) =>
        $"#{(int)(color.r * 255):X2}{(int)(color.g * 255):X2}{(int)(color.b * 255):X2}{(int)(color.a * 255):X2}";

    public static string GetMd5HashFromFilePath(string filePath)
    {
        using (var md5 = MD5.Create())
        using (var stream = File.OpenRead(filePath))
        {
            return BitConverter
                .ToString(md5.ComputeHash(stream))
                .Replace("-", "")
                .ToLowerInvariant();
        }
    }

    public static string GetMd5HashFromStream(Stream stream)
    {
        using (var md5 = MD5.Create())
        {
            return BitConverter
                .ToString(md5.ComputeHash(stream))
                .Replace("-", "")
                .ToLowerInvariant();
        }
    }

    public static class UniqueIDGenerator
    {
        public static long GenerateRandomID()
        {
            // Start from DateTime.MinValue
            DateTime start = DateTime.MinValue;

            // Add 10 years to get the end of the range
            DateTime end = start.AddYears(10);

            // Get the number of ticks from min to max
            long minTicks = start.Ticks;
            long maxTicks = end.Ticks;

            // Calculate the total range in ticks
            long range = maxTicks - minTicks;

            // Create a random number generator
            Random random = new Random();

            // Generate a random ulong within the range
            long randomTicks = GenerateRandomInt64(0, range);

            // Final ID is within the range of the 10-year window
            long randomID = minTicks + randomTicks;
            return randomID;
        }

        private static long GenerateRandomInt64(long min, long max)
        {
            Random random = new Random();

            // Calculate how many 32-bit chunks are needed to cover the full 64-bit range
            long range = max - min;
            long result = 0;
            long mask = 0x7FFFFFFF; // Mask for 31 bits (safe for 32-bit Random.Next())

            while (true)
            {
                result = 0;
                // Use Random.Next() twice to get a 63-bit number (safe for 64-bit long)
                for (int i = 0; i < 2; i++)
                {
                    result = (result << 31) | (random.Next() & mask);
                }

                // If the result is within the desired range, return it
                if (result >= 0 && result <= range)
                {
                    return result;
                }
            }
        }
    }
}
