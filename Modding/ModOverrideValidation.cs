using System;
using System.Globalization;
using System.IO;
using UnityEngine;

namespace TJ
{
    // Shared warn-and-skip validation helpers for mod override loaders. A malformed or unknown
    // value must never throw past a loader - it gets logged and the previous value is kept.
    public static class ModOverrideValidation
    {
        public static string GetModLabel(string modFolderPath)
        {
            return Path.GetFileName(modFolderPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        }

        // Runs loadAndApply if the file exists, catching and logging any exception so one bad
        // mod file can never crash TabletopTavernData.Awake().
        public static bool TryLoadFile(string path, Action loadAndApply, string contextLabel)
        {
            if (!File.Exists(path)) return false;
            try
            {
                loadAndApply();
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[ModOverride] {contextLabel}: failed to load '{path}' ({e.GetType().Name}: {e.Message}). Skipping this file.");
                return false;
            }
        }

        // Absent/empty raw values are treated as "no override" and return false silently.
        // A present-but-unparseable value is a real authoring mistake and gets a warning.
        public static bool TryParseEnumOrWarn<TEnum>(string raw, string fieldName, string context, out TEnum value) where TEnum : struct
        {
            value = default;
            if (string.IsNullOrEmpty(raw)) return false;
            if (Enum.TryParse(raw, out value)) return true;
            Debug.LogWarning($"[ModOverride] {context}: unknown {fieldName} value '{raw}', ignoring.");
            return false;
        }

        public static bool TryParseIntOrWarn(string raw, string fieldName, string context, out int value)
        {
            value = default;
            if (string.IsNullOrEmpty(raw)) return false;
            if (int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out value)) return true;
            Debug.LogWarning($"[ModOverride] {context}: invalid {fieldName} value '{raw}', ignoring.");
            return false;
        }

        public static bool TryParseFloatOrWarn(string raw, string fieldName, string context, out float value)
        {
            value = default;
            if (string.IsNullOrEmpty(raw)) return false;
            if (float.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out value)) return true;
            Debug.LogWarning($"[ModOverride] {context}: invalid {fieldName} value '{raw}', ignoring.");
            return false;
        }

        public static bool TryParseBoolOrWarn(string raw, string fieldName, string context, out bool value)
        {
            value = default;
            if (string.IsNullOrEmpty(raw)) return false;
            if (raw.Equals("true", StringComparison.OrdinalIgnoreCase) || raw == "1") { value = true; return true; }
            if (raw.Equals("false", StringComparison.OrdinalIgnoreCase) || raw == "0") { value = false; return true; }
            Debug.LogWarning($"[ModOverride] {context}: invalid {fieldName} value '{raw}' (expected true/false), ignoring.");
            return false;
        }

        public static int ValidatePositive(int parsedValue, int previousValue, string fieldName, string context)
        {
            if (parsedValue > 0) return parsedValue;
            Debug.LogWarning($"[ModOverride] {context}: {fieldName} override value {parsedValue} is not valid (must be positive), keeping previous value {previousValue}.");
            return previousValue;
        }
    }
}
