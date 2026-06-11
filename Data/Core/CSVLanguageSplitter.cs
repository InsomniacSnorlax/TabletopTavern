#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

public class CSVLanguageSplitter : MonoBehaviour
{
    [SerializeField] private string tableName = "MainLocalizationTable";

#if UNITY_EDITOR
    [ContextMenu("Split CSV by Language")]
    private void SplitCSV()
    {
        // Path to the input CSV (TextAsset in Unity project)
        string csvAssetPath = $"Assets/Data/Localization/Localizor/{tableName}.csv";
        TextAsset csvAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(csvAssetPath);

        if (csvAsset == null)
        {
            Debug.LogError($"CSV file not found at {csvAssetPath}. Please ensure the file exists.");
            return;
        }

        // Output directory (relative to project root)
        string outputDir = "Assets/Data/Localization/Localizor/SplitCSVs";
        Directory.CreateDirectory(outputDir); // Ensure output directory exists

        // Parse CSV
        List<string[]> rows = ParseCSV(csvAsset.text);
        if (rows.Count < 2)
        {
            Debug.LogError("CSV file is empty or has no data rows.");
            return;
        }

        // Split header and data
        string[] header = rows[0];
        List<string[]> dataRows = rows.Skip(1).Where(row => row.Length > 0).ToList();

        // Debug: Log header
        Debug.Log($"Header: {string.Join(", ", header)}");

        // Identify language columns (exclude Key, Id, Shared Comments, and any Comments columns)
        var languageColumns = header
            .Select((h, i) => new { Name = h, Index = i })
            .Where(h => h.Index > 2 && !h.Name.EndsWith("Comments") && !h.Name.Contains("Shared Comments"))
            .ToList();

        if (languageColumns.Count == 0)
        {
            Debug.LogError("No language columns found in CSV header.");
            return;
        }

        // Create a CSV for each language
        foreach (var lang in languageColumns)
        {
            string langName = lang.Name;
            int colIndex = lang.Index;

            // Clean language name for filename
            string langClean = langName.Replace(" ", "_").Replace("(", "").Replace(")", "");
            string outputPath = Path.Combine(outputDir, $"{langClean}.csv");

            // Debug: Log language and column index
            Debug.Log($"Processing language: {langName}, Column Index: {colIndex}, Output: {outputPath}");

            using (StreamWriter writer = new StreamWriter(outputPath, false, System.Text.Encoding.UTF8))
            {
                foreach (var row in dataRows)
                {
                    if (row.Length > colIndex && row.Length > 0) // Ensure row has key and language column
                    {
                        // Remove quotes from key and translation if present
                        string key = row[0].Trim('"');
                        string translation = row[colIndex].Trim('"');
                        writer.WriteLine($"{key},{translation}"); // Write Key, Translation
                    }
                    else
                    {
                        Debug.LogWarning($"Skipping malformed row for language {langName}: {string.Join(",", row)}");
                    }
                }
            }

            Debug.Log($"Created CSV for {langName}: {outputPath}");
        }

        // Refresh Unity's AssetDatabase to show new files
        AssetDatabase.Refresh();

        Debug.Log($"Split {languageColumns.Count} language CSVs into {outputDir}");
    }

    // Custom CSV parser to handle quoted fields and commas
    private List<string[]> ParseCSV(string csvText)
    {
        var rows = new List<string[]>();
        var lines = csvText.Split('\n').Select(line => line.Trim()).Where(line => !string.IsNullOrEmpty(line)).ToArray();
        if (lines.Length == 0) return rows;

        foreach (string line in lines)
        {
            var fields = new List<string>();
            bool inQuotes = false;
            string currentField = "";

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    inQuotes = !inQuotes;
                    currentField += c; // Keep quotes in field for now
                }
                else if (c == ',' && !inQuotes)
                {
                    fields.Add(currentField);
                    currentField = "";
                }
                else
                {
                    currentField += c;
                }
            }

            // Add the last field
            fields.Add(currentField);

            // Validate row length against header
            if (rows.Count == 0 || fields.Count == rows[0].Length)
            {
                rows.Add(fields.ToArray());
            }
            else
            {
                Debug.LogWarning($"Skipping row with mismatched columns: {line}");
            }
        }

        return rows;
    }
#endif
}