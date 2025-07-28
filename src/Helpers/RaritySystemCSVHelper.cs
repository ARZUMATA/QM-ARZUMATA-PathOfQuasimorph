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
            string newFilePath = embeddedFile;
            bool hasChanged = false;

            var oldLines = File.ReadAllLines(oldFilePath).ToList();
            var newLines = new List<string>();
            var resultLines = new List<string>();

            // Read embedded file
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(newFilePath))
            using (StreamReader reader = new StreamReader(stream))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    newLines.Add(line);
                }
            }

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
                    hasChanged |= MergeSection(oldLines, newLines, resultLines, ref oldIndex, ref newIndex);
                }
                else if (oldLine.StartsWith("Arbitrary Values") && newLine.StartsWith("Arbitrary Values"))
                {
                    hasChanged |= MergeArbitraryValues(oldLines, newLines, resultLines, ref oldIndex, ref newIndex);
                }
                else if (!oldLine.Contains(",") || !newLine.Contains(","))
                {
                    // If not data lines, just copy new line and continue
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
                        string mergedRow = MergeRows(oldLine, newLine);
                        resultLines.Add(mergedRow);

                        // Check if the merged row is different from the old one
                        if (mergedRow != oldLine)
                            hasChanged = true;

                        oldIndex++;
                        newIndex++;
                    }
                    else
                    {
                        // Add new line as-is
                        resultLines.Add(newLine);
                        newIndex++;
                        hasChanged = true; // New key introduced
                    }
                }
            }

            // Append remaining new lines (if any)
            while (newIndex < newLines.Count)
            {
                resultLines.Add(newLines[newIndex]);
                hasChanged = true; // New lines added
                newIndex++;
            }

            // If changes were made, do backup and write
            if (hasChanged)
            {
                string oldFileBackupPath = oldFilePath + ".backup";
                if (File.Exists(oldFileBackupPath))
                    File.Delete(oldFileBackupPath);

                File.Copy(oldFilePath, oldFileBackupPath);
                File.WriteAllLines(oldFilePath, resultLines);
                Console.WriteLine("Migration completed with changes. Backup created.");
            }
            else
            {
                Console.WriteLine("No changes detected. Migration skipped.");
            }
        }

        private static bool MergeSection(List<string> oldLines, List<string> newLines, List<string> resultLines, ref int oldIndex, ref int newIndex)
        {
            bool sectionChanged = false;

            // Add the section header from new file
            resultLines.Add(newLines[newIndex]);
            newIndex++;
            oldIndex++;

            var oldData = new Dictionary<string, string>();

            // Read old section data
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

            // Read new section data
            while (newIndex < newLines.Count && !newLines[newIndex].StartsWith("Arbitrary Values"))
            {
                string newLine = newLines[newIndex];
                string[] newSplit = newLine.Split(',');

                if (!string.IsNullOrWhiteSpace(newLine) && newSplit.Length > 0 && !string.IsNullOrEmpty(newSplit[0]))
                {
                    if (oldData.TryGetValue(newSplit[0], out string oldEntry))
                    {
                        string mergedRow = MergeRows(oldEntry, newLine);
                        resultLines.Add(mergedRow);
                        if (mergedRow != oldEntry)
                            sectionChanged = true;
                    }
                    else
                    {
                        resultLines.Add(newLine);
                        sectionChanged = true;
                    }
                }
                newIndex++;
            }

            return sectionChanged;
        }

        private static bool MergeArbitraryValues(List<string> oldLines, List<string> newLines, List<string> resultLines, ref int oldIndex, ref int newIndex)
        {
            bool changed = false;

            // Add header
            resultLines.Add(newLines[newIndex]);
            newIndex++;
            oldIndex++;

            var oldKvp = new Dictionary<string, string>();

            // Read old arbitrary values
            while (oldIndex < oldLines.Count && oldLines[oldIndex].Contains(","))
            {
                string[] parts = oldLines[oldIndex].Split(',');
                if (parts.Length > 0 && !string.IsNullOrEmpty(parts[0]))
                    oldKvp[parts[0]] = parts.Length > 1 ? parts[1] : "";
                oldIndex++;
            }

            // Merge with new arbitrary values
            while (newIndex < newLines.Count && newLines[newIndex].Contains(","))
            {
                string[] parts = newLines[newIndex].Split(',');
                if (parts.Length > 0 && !string.IsNullOrEmpty(parts[0]))
                {
                    string newValue = oldKvp.TryGetValue(parts[0], out string v) ? v : (parts.Length > 1 ? parts[1] : "");

                    string mergedLine = $"{parts[0]},{newValue}";
                    resultLines.Add(mergedLine);

                    if (!oldKvp.ContainsKey(parts[0]) || (parts.Length > 1 && oldKvp[parts[0]] != parts[1]))
                        changed = true;
                }
                newIndex++;
            }

            return changed;
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
