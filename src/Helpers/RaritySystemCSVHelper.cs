using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace QM_PathOfQuasimorph.PoqHelpers
{
    internal class RaritySystemCSVHelper
    {
        internal static void DoMerge(string weightPath, string embeddedFile)
        {
            string oldFilePath = weightPath;
            string backupFilePath = oldFilePath + ".backup";
            string outputFilePath = oldFilePath; // Same as oldFilePath
            string newFilePath = embeddedFile;

            // Backup the old file
            if (File.Exists(oldFilePath))
            {
                File.Copy(oldFilePath, backupFilePath, true);
            }

            var oldLines = File.ReadAllLines(oldFilePath).ToList();
            var newLines = new List<string>();

            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(embeddedFile))
            using (StreamReader reader = new StreamReader(stream))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    newLines.Add(line);
                }
            }

            var resultLines = new List<string>();

            int oldIndex = 0;
            int newIndex = 0;

            while (oldIndex < oldLines.Count && newIndex < newLines.Count)
            {
                string oldLine = oldLines[oldIndex];
                string newLine = newLines[newIndex];

                if (string.IsNullOrWhiteSpace(oldLine) || string.IsNullOrWhiteSpace(newLine))
                {
                    // Skip empty lines
                    if (string.IsNullOrWhiteSpace(oldLine)) oldIndex++;
                    if (string.IsNullOrWhiteSpace(newLine)) newIndex++;
                    continue;
                }

                // Check if both lines belong to the same section
                if (oldLine.StartsWith("MonsterMasteries") && newLine.StartsWith("MonsterMasteries"))
                {
                    MergeSection(oldLines, newLines, resultLines, ref oldIndex, ref newIndex);
                }
                else if (oldLine.StartsWith("Arbitrary Values") && newLine.StartsWith("Arbitrary Values"))
                {
                    MergeArbitraryValues(oldLines, newLines, resultLines, ref oldIndex, ref newIndex);
                }
                else if (!oldLine.Contains(",") || !newLine.Contains(","))
                {
                    // If not data lines, just copy and continue
                    resultLines.Add(newLine);
                    oldIndex++;
                    newIndex++;
                }
                else
                {
                    string oldKey = oldLine.Split(',')[0];
                    string newKey = newLine.Split(',')[0];

                    if (oldKey == newKey)
                    {
                        resultLines.Add(MergeRows(oldLine, newLine));
                        oldIndex++;
                        newIndex++;
                    }
                    else
                    {
                        // Key exists only in new file
                        resultLines.Add(newLine);
                        newIndex++;
                    }
                }
            }

            // Append any remaining lines from new file
            while (newIndex < newLines.Count)
            {
                resultLines.Add(newLines[newIndex]);
                newIndex++;
            }

            // Overwrite the old file with migrated data
            File.WriteAllLines(outputFilePath, resultLines);
            Console.WriteLine("Migration completed successfully.");
        }

        private static void MergeSection(List<string> oldLines, List<string> newLines, List<string> resultLines, ref int oldIndex, ref int newIndex)
        {
            // Add the section header from new file
            resultLines.Add(newLines[newIndex]);
            oldIndex++;
            newIndex++;

            Dictionary<string, string> oldData = new Dictionary<string, string>();

            // Read the current section from old file
            while (oldIndex < oldLines.Count && !oldLines[oldIndex].StartsWith("Arbitrary Values"))
            {
                string oldLine = oldLines[oldIndex];
                if (!string.IsNullOrWhiteSpace(oldLine) && oldLine.Contains(","))
                {
                    string[] oldSplit = oldLine.Split(',');
                    if (!string.IsNullOrEmpty(oldSplit[0]))
                        oldData[oldSplit[0]] = oldLine;
                }
                oldIndex++;
            }

            // Read the current section from new file
            while (newIndex < newLines.Count && !newLines[newIndex].StartsWith("Arbitrary Values"))
            {
                string newLine = newLines[newIndex];
                if (!string.IsNullOrWhiteSpace(newLine) && newLine.Contains(","))
                {
                    string[] newSplit = newLine.Split(',');
                    if (!string.IsNullOrEmpty(newSplit[0]))
                    {
                        if (oldData.TryGetValue(newSplit[0], out string oldEntry))
                            resultLines.Add(MergeRows(oldEntry, newLine));
                        else
                            resultLines.Add(newLine);
                    }
                }
                newIndex++;
            }
        }

        private static void MergeArbitraryValues(List<string> oldLines, List<string> newLines, List<string> resultLines, ref int oldIndex, ref int newIndex)
        {
            // Add the Arbitrary Values header from new file
            resultLines.Add(newLines[newIndex]);
            newIndex++;
            oldIndex++;

            Dictionary<string, string> oldKeyValuePairs = new Dictionary<string, string>();

            // Read key-value pairs from old Arbitrary Values section
            while (oldIndex < oldLines.Count && oldLines[oldIndex].Contains(","))
            {
                string[] parts = oldLines[oldIndex].Split(',');
                if (!string.IsNullOrEmpty(parts[0]))
                    oldKeyValuePairs[parts[0]] = parts[1];
                oldIndex++;
            }

            // Merge with new Arbitrary Values section
            while (newIndex < newLines.Count && newLines[newIndex].Contains(","))
            {
                string[] parts = newLines[newIndex].Split(',');
                if (parts.Length > 0 && !string.IsNullOrEmpty(parts[0]))
                {
                    string value = oldKeyValuePairs.TryGetValue(parts[0], out string v) ? v : (parts.Length > 1 ? parts[1] : "");
                    resultLines.Add($"{parts[0]},{value}");
                }
                newIndex++;
            }
        }

        private static string MergeRows(string oldRow, string newRow)
        {
            var oldFields = oldRow.Split(',');
            var newFields = newRow.Split(',');

            var resultFields = new List<string>();

            for (int i = 0; i < newFields.Length; i++)
            {
                if (i < oldFields.Length && !string.IsNullOrWhiteSpace(oldFields[i]))
                    resultFields.Add(oldFields[i]);
                else
                    resultFields.Add(newFields[i]);
            }

            return string.Join(",", resultFields);
        }













    }
}
