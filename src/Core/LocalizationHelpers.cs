using MGSC;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using static MGSC.Localization;

namespace QM_PathOfQuasimorph.Core
{
    internal static class LocalizationHelpers
    {
        private static bool localizationDataLoaded = false;

        public static void LoadLocalizationData(string resourceName = "QM_PathOfQuasimorph.Files.Localization.csv")
        {
            if (localizationDataLoaded)
            {
                Plugin.Logger.Log($"localizationDataLoaded: exiting.");
                return;
            }

            // foreach (KeyValuePair<Localization.Lang, Dictionary<string, string>> languageToDict in Singleton<Localization>.Instance.db)
            var localizationDb = new Dictionary<Lang, Dictionary<string, string>>();

            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                string headerLine = reader.ReadLine(); // Read the header line
                if (headerLine == null)
                {
                    return; // No data in CSV, exit early
                }

                string[] headers = headerLine.Split(','); // Split the headers

                // Map the language names to Lang enum
                Dictionary<string, Lang> languageMap = new Dictionary<string, Lang>();
                for (int i = 1; i < headers.Length; i++)
                {
                    if (Enum.TryParse(headers[i], out Lang lang))
                    {
                        languageMap[headers[i]] = lang;
                    }
                }

                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] parts = ParseCsvLine(line); // Call custom CSV parser for quoted fields

                    if (parts.Length >= headers.Length)
                    {
                        string id = parts[0];

                        // Assign each language string to the dictionary using the header
                        for (int i = 1; i < headers.Length; i++)
                        {
                            string langName = headers[i];
                            string value = parts[i];

                            // Map language name to Lang enum if possible and add it to the db
                            if (languageMap.TryGetValue(langName, out Lang lang))
                            {
                                //Console.WriteLine($"try add {lang} / {langName}   {id}   {value}");
                                Singleton<Localization>.Instance.db[lang].Add(id, value);
                            }
                            else
                            {
                                // Ignore unknown languages like Id
                                Plugin.Logger.Log($"Unknown language '{langName}' in Localization.csv");
                            }
                        }
                    }
                }
            }

            localizationDataLoaded = true;
        }

        private static string[] ParseCsvLine(string line)
        {
            List<string> fields = new List<string>();
            bool inQuotes = false;
            StringBuilder field = new StringBuilder();
            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                    if (i + 1 < line.Length && line[i + 1] == '"')
                    {
                        i++; // Skip the next quote (treat two quotes as one)
                        field.Append(c);
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    fields.Add(field.ToString());
                    field.Clear();
                }
                else
                {
                    field.Append(c);
                }
            }
            fields.Add(field.ToString()); // Add the last field
            return fields.ToArray();
        }
    }
}