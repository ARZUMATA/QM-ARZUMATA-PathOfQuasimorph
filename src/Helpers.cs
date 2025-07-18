using QM_PathOfQuasimorph;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Unity.Profiling;
using UnityEngine;
using Random = System.Random;

internal static class Helpers
{
    public static string FaceColorToHex(Color color) => $"#{color.r:F0}{color.g:F0}{color.b:F0}";

    public static string AlphaAwareColorToHex(Color color) =>
        $"#{(int)(color.r * 255):X2}{(int)(color.g * 255):X2}{(int)(color.b * 255):X2}{(int)(color.a * 255):X2}";

    public static Color HexStringToUnityColor(string hex, int alpha = 255)
    {
        if (string.IsNullOrEmpty(hex) || !hex.StartsWith("#"))
        {
            throw new ArgumentException("Invalid color format", nameof(hex));
        }

        // Check the length of the input string to determine if it has an alpha channel
        if (hex.Length == 7)  // No alpha channel provided, assume full opacity
        {
            hex = "#" + hex.Substring(1) + alpha.ToString("X2");
        }
        else if (hex.Length != 9)
        {
            throw new ArgumentException("Invalid color format", nameof(hex));
        }

        // Parse the R, G, B, and A values from the hex string
        int r = int.Parse(hex.Substring(1, 2), System.Globalization.NumberStyles.HexNumber);
        int g = int.Parse(hex.Substring(3, 2), System.Globalization.NumberStyles.HexNumber);
        int b = int.Parse(hex.Substring(5, 2), System.Globalization.NumberStyles.HexNumber);
        int a = int.Parse(hex.Substring(7, 2), System.Globalization.NumberStyles.HexNumber);

        // Convert to normalized float values
        return new Color(r / 255f, g / 255f, b / 255f, a / 255f);
    }

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

        public static long GenerateRandomIDWith16Characters()
        {
            // Convert to string and ensure it is at least 16 characters long
            long randomID = GenerateRandomID();
            string randomIDStr = randomID.ToString();

            if (randomIDStr.Length < 16)
            {
                randomIDStr = randomIDStr.PadLeft(16, '1');
                randomID = long.Parse(randomIDStr);
            }

            return randomID;
        }

        private static long GenerateRandomInt64(long min, long max)
        {
            Random random = new Random();

            // Calculate how many 32-bit chunks are needed to cover the full 64-bit range
            long range = max - min;
            long result = 0;
            long mask = 0x7FFFFFFF; // Mask for 31 bits (safe for 32-bit StandardRandom.Next())

            while (true)
            {
                result = 0;

                // Use StandardRandom.Next() twice to get a 63-bit number (safe for 64-bit long)
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

    public static Sprite LoadSpriteFromEmbeddedBundle(string bundleResourceName, string assetName)
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        using (Stream stream = assembly.GetManifestResourceStream(bundleResourceName))
        {
            if (stream == null)
            {
                Plugin.Logger.LogError($"Embedded AssetBundle resource '{bundleResourceName}' not found.");
                return null;
            }

            byte[] bundleData = new byte[stream.Length];
            stream.Read(bundleData, 0, bundleData.Length);

            AssetBundle bundle = AssetBundle.LoadFromMemory(bundleData);
            if (bundle == null)
            {
                Plugin.Logger.LogError($"Failed to load AssetBundle from memory for resource '{bundleResourceName}'.");
                return null;
            }

            Sprite mySprite = bundle.LoadAsset<Sprite>(assetName);
            bundle.Unload(false); // Unload to prevent memory leaks

            return mySprite;
        }
    }

    public static Sprite FindSpriteByName(string spriteName)
    {
        return Resources.FindObjectsOfTypeAll<Sprite>()
            .FirstOrDefault(s => s.name == spriteName);
    }
}
