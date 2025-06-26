using System;
using System.IO;
using UnityEngine;
using System.Security.Cryptography;

internal static class Helpers
{
    public static string FaceColorToHex(Color color) => $"#{color.r:F0}{color.g:F0}{color.b:F0}";
    public static string AlphaAwareColorToHex(Color color) => $"#{(int)(color.r * 255):X2}{(int)(color.g * 255):X2}{(int)(color.b * 255):X2}{(int)(color.a * 255):X2}";

    public static string GetMd5HashFromFilePath(string filePath)
    {
        using (var md5 = MD5.Create())
        using (var stream = File.OpenRead(filePath))
        {
            return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
        }
    }

    public static string GetMd5HashFromStream(Stream stream)
    {
        using (var md5 = MD5.Create())
        {
            return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
        }
    }
}
