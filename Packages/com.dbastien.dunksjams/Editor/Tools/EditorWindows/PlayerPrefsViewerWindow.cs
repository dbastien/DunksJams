using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class PlayerPrefsViewerWindow : EditorWindow
{
    private Vector2 _scrollPosition;
    private string _searchFilter = "";
    private List<PlayerPrefEntry> _entries = new();
    private PlayerPrefEntry _editingEntry;
    private string _editingValue;
    private string _newKey = "";
    private string _newValue = "";
    private PrefType _newType = PrefType.String;

    private enum PrefType { String, Int, Float }

    private class PlayerPrefEntry
    {
        public string Key;
        public object Value;
        public PrefType Type;
    }

    [MenuItem("‽/Tools/PlayerPrefs Viewer")]
    public static void ShowWindow()
    {
        var window = GetWindow<PlayerPrefsViewerWindow>("PlayerPrefs Viewer");
        window.minSize = new Vector2(600, 400);
        window.Show();
    }

    private void OnEnable()
    {
        RefreshEntries();
    }

    private void OnGUI()
    {
        DrawToolbar();
        DrawAddNew();
        DrawEntries();
    }

    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

        // Search field
        EditorGUI.BeginChangeCheck();
        _searchFilter = GUILayout.TextField(_searchFilter, EditorStyles.toolbarSearchField, GUILayout.Width(200));
        if (EditorGUI.EndChangeCheck())
        {
            RefreshEntries();
        }

        if (GUILayout.Button("×", EditorStyles.toolbarButton, GUILayout.Width(20)))
        {
            _searchFilter = "";
            RefreshEntries();
            GUI.FocusControl(null);
        }

        GUILayout.FlexibleSpace();

        // Refresh button
        if (GUILayout.Button("Refresh", EditorStyles.toolbarButton))
        {
            RefreshEntries();
        }

        // Save button (forces Unity to flush PlayerPrefs)
        if (GUILayout.Button("Save", EditorStyles.toolbarButton))
        {
            PlayerPrefs.Save();
            EditorUtility.DisplayDialog("PlayerPrefs Saved", "PlayerPrefs have been saved to disk.", "OK");
        }

        // Delete all button
        GUI.backgroundColor = new Color(1f, 0.5f, 0.5f);
        if (GUILayout.Button("Delete All", EditorStyles.toolbarButton))
        {
            if (EditorUtility.DisplayDialog("Delete All PlayerPrefs",
                "Are you sure you want to delete ALL PlayerPrefs? This cannot be undone!",
                "Delete All", "Cancel"))
            {
                PlayerPrefs.DeleteAll();
                PlayerPrefs.Save();
                RefreshEntries();
            }
        }
        GUI.backgroundColor = Color.white;

        EditorGUILayout.EndHorizontal();
    }

    private void DrawAddNew()
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Add New PlayerPref", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.LabelField("Key:", GUILayout.Width(40));
        _newKey = EditorGUILayout.TextField(_newKey, GUILayout.Width(200));

        GUILayout.Space(10);

        EditorGUILayout.LabelField("Type:", GUILayout.Width(40));
        _newType = (PrefType)EditorGUILayout.EnumPopup(_newType, GUILayout.Width(80));

        GUILayout.Space(10);

        EditorGUILayout.LabelField("Value:", GUILayout.Width(40));
        _newValue = EditorGUILayout.TextField(_newValue);

        if (GUILayout.Button("Add", GUILayout.Width(60)))
        {
            AddNewPlayerPref();
        }

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
    }

    private void AddNewPlayerPref()
    {
        if (string.IsNullOrWhiteSpace(_newKey))
        {
            EditorUtility.DisplayDialog("Invalid Key", "Key cannot be empty.", "OK");
            return;
        }

        if (PlayerPrefs.HasKey(_newKey))
        {
            if (!EditorUtility.DisplayDialog("Key Exists",
                $"Key '{_newKey}' already exists. Overwrite?", "Overwrite", "Cancel"))
            {
                return;
            }
        }

        try
        {
            switch (_newType)
            {
                case PrefType.String:
                    PlayerPrefs.SetString(_newKey, _newValue);
                    break;

                case PrefType.Int:
                    if (int.TryParse(_newValue, out int intValue))
                        PlayerPrefs.SetInt(_newKey, intValue);
                    else
                    {
                        EditorUtility.DisplayDialog("Invalid Value", "Value must be an integer.", "OK");
                        return;
                    }
                    break;

                case PrefType.Float:
                    if (float.TryParse(_newValue, out float floatValue))
                        PlayerPrefs.SetFloat(_newKey, floatValue);
                    else
                    {
                        EditorUtility.DisplayDialog("Invalid Value", "Value must be a float.", "OK");
                        return;
                    }
                    break;
            }

            PlayerPrefs.Save();
            _newKey = "";
            _newValue = "";
            RefreshEntries();
        }
        catch (Exception e)
        {
            EditorUtility.DisplayDialog("Error", $"Failed to add: {e.Message}", "OK");
        }
    }

    private void DrawEntries()
    {
        if (_entries == null || _entries.Count == 0)
        {
            EditorGUILayout.HelpBox("No PlayerPrefs found.\n\nNote: PlayerPrefs are read from registry/plist. Type detection is approximate.", MessageType.Info);
            return;
        }

        EditorGUILayout.BeginVertical();
        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

        // Header
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUILayout.Label("Key", EditorStyles.boldLabel, GUILayout.Width(300));
        GUILayout.Label("Type", EditorStyles.boldLabel, GUILayout.Width(60));
        GUILayout.Label("Value", EditorStyles.boldLabel);
        GUILayout.Label("Actions", EditorStyles.boldLabel, GUILayout.Width(120));
        EditorGUILayout.EndHorizontal();

        // Entries
        for (var i = 0; i < _entries.Count; i++)
        {
            PlayerPrefEntry entry = _entries[i];
            DrawEntry(entry, i);
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();

        // Status bar
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUILayout.Label($"Showing {_entries.Count} entries", EditorStyles.miniLabel);
        GUILayout.FlexibleSpace();
        GUILayout.Label("Location: " + GetPlayerPrefsLocation(), EditorStyles.miniLabel);
        EditorGUILayout.EndHorizontal();
    }

    private void DrawEntry(PlayerPrefEntry entry, int index)
    {
        Color bgColor = index % 2 == 0 ? new Color(0.2f, 0.2f, 0.2f, 0.3f) : Color.clear;

        EditorGUILayout.BeginHorizontal(GUILayout.Height(20));

        if (Event.current.type == EventType.Repaint)
        {
            Rect rect = GUILayoutUtility.GetLastRect();
            EditorGUI.DrawRect(rect, bgColor);
        }

        // Key
        EditorGUILayout.LabelField(entry.Key, GUILayout.Width(300));

        // Type
        EditorGUILayout.LabelField(entry.Type.ToString(), GUILayout.Width(60));

        // Value (editable)
        if (_editingEntry == entry)
        {
            GUI.SetNextControlName($"EditField_{index}");
            _editingValue = EditorGUILayout.TextField(_editingValue);

            if (GUILayout.Button("Save", GUILayout.Width(50)))
            {
                SaveEdit(entry);
            }

            if (GUILayout.Button("Cancel", GUILayout.Width(60)))
            {
                _editingEntry = null;
            }
        }
        else
        {
            EditorGUILayout.LabelField(entry.Value?.ToString() ?? "null");

            // Edit button
            if (GUILayout.Button("Edit", GUILayout.Width(50)))
            {
                _editingEntry = entry;
                _editingValue = entry.Value?.ToString() ?? "";
                EditorGUI.FocusTextInControl($"EditField_{index}");
            }

            // Delete button
            GUI.backgroundColor = new Color(1f, 0.7f, 0.7f);
            if (GUILayout.Button("Delete", GUILayout.Width(60)))
            {
                if (EditorUtility.DisplayDialog("Delete PlayerPref",
                    $"Delete key '{entry.Key}'?", "Delete", "Cancel"))
                {
                    PlayerPrefs.DeleteKey(entry.Key);
                    PlayerPrefs.Save();
                    RefreshEntries();
                }
            }
            GUI.backgroundColor = Color.white;
        }

        EditorGUILayout.EndHorizontal();
    }

    private void SaveEdit(PlayerPrefEntry entry)
    {
        try
        {
            switch (entry.Type)
            {
                case PrefType.String:
                    PlayerPrefs.SetString(entry.Key, _editingValue);
                    break;

                case PrefType.Int:
                    if (int.TryParse(_editingValue, out int intValue))
                        PlayerPrefs.SetInt(entry.Key, intValue);
                    else
                    {
                        EditorUtility.DisplayDialog("Invalid Value", "Value must be an integer.", "OK");
                        return;
                    }
                    break;

                case PrefType.Float:
                    if (float.TryParse(_editingValue, out float floatValue))
                        PlayerPrefs.SetFloat(entry.Key, floatValue);
                    else
                    {
                        EditorUtility.DisplayDialog("Invalid Value", "Value must be a float.", "OK");
                        return;
                    }
                    break;
            }

            PlayerPrefs.Save();
            _editingEntry = null;
            RefreshEntries();
        }
        catch (Exception e)
        {
            EditorUtility.DisplayDialog("Error", $"Failed to save: {e.Message}", "OK");
        }
    }

    private void RefreshEntries()
    {
        _entries.Clear();

#if UNITY_EDITOR_WIN
        RefreshEntriesWindows();
#elif UNITY_EDITOR_OSX
        RefreshEntriesMac();
#elif UNITY_EDITOR_LINUX
        RefreshEntriesLinux();
#else
        EditorUtility.DisplayDialog("Unsupported Platform",
            "PlayerPrefs viewer is only supported on Windows, Mac, and Linux.", "OK");
#endif

        // Apply search filter
        if (!string.IsNullOrEmpty(_searchFilter))
        {
            _entries = _entries.Where(e =>
                e.Key.IndexOf(_searchFilter, StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();
        }

        // Sort by key
        _entries = _entries.OrderBy(e => e.Key).ToList();
    }

#if UNITY_EDITOR_WIN
    private void RefreshEntriesWindows()
    {
        try
        {
            string companyName = Application.companyName;
            string productName = Application.productName;
            string registryPath = $"Software\\{companyName}\\{productName}";

            using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(registryPath);
            if (key == null) return;

            foreach (string valueName in key.GetValueNames())
            {
                object value = key.GetValue(valueName);

                // Detect type based on registry value type
                if (value is string strVal)
                {
                    _entries.Add(new PlayerPrefEntry
                    {
                        Key = valueName,
                        Value = PlayerPrefs.GetString(valueName),
                        Type = PrefType.String
                    });
                }
                else if (value is int intVal)
                {
                    _entries.Add(new PlayerPrefEntry
                    {
                        Key = valueName,
                        Value = PlayerPrefs.GetInt(valueName),
                        Type = PrefType.Int
                    });
                }
                else if (value is byte[] bytes && bytes.Length == 4)
                {
                    _entries.Add(new PlayerPrefEntry
                    {
                        Key = valueName,
                        Value = PlayerPrefs.GetFloat(valueName),
                        Type = PrefType.Float
                    });
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to read PlayerPrefs from registry: {e.Message}");
        }
    }
#endif

#if UNITY_EDITOR_OSX
    private void RefreshEntriesMac()
    {
        try
        {
            string companyName = Application.companyName;
            string productName = Application.productName;
            string bundleIdentifier = $"unity.{companyName}.{productName}";

            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "defaults",
                    Arguments = $"read {bundleIdentifier}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogWarning($"PlayerPrefs read warning: {error}");
            }

            // Parse defaults output - simplified
            ParseMacDefaults(output);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to read PlayerPrefs from plist: {e.Message}");
        }
    }

    private void ParseMacDefaults(string output)
    {
        // Simple parser for defaults output format
        var lines = output.Split('\n');
        foreach (string line in lines)
        {
            string trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed) || trimmed == "{" || trimmed == "}")
                continue;

            // Format is usually: key = value;
            int equalIndex = trimmed.IndexOf(" = ", StringComparison.Ordinal);
            if (equalIndex < 0) continue;

            string key = trimmed.Substring(0, equalIndex).Trim();
            string valueStr = trimmed.Substring(equalIndex + 3).TrimEnd(';').Trim();

            // Try to detect type
            if (valueStr.StartsWith("\"") && valueStr.EndsWith("\""))
            {
                string strValue = valueStr.Substring(1, valueStr.Length - 2);
                _entries.Add(new PlayerPrefEntry
                {
                    Key = key,
                    Value = strValue,
                    Type = PrefType.String
                });
            }
            else if (int.TryParse(valueStr, out int intValue))
            {
                _entries.Add(new PlayerPrefEntry
                {
                    Key = key,
                    Value = intValue,
                    Type = PrefType.Int
                });
            }
            else if (float.TryParse(valueStr, out float floatValue))
            {
                _entries.Add(new PlayerPrefEntry
                {
                    Key = key,
                    Value = floatValue,
                    Type = PrefType.Float
                });
            }
        }
    }
#endif

#if UNITY_EDITOR_LINUX
    private void RefreshEntriesLinux()
    {
        try
        {
            string companyName = Application.companyName;
            string productName = Application.productName;
            string prefsPath = $"{Environment.GetEnvironmentVariable("HOME")}/.config/unity3d/{companyName}/{productName}/prefs";

            if (!System.IO.File.Exists(prefsPath))
            {
                Debug.LogWarning($"PlayerPrefs file not found at: {prefsPath}");
                return;
            }

            // Read Unity prefs file (XML format)
            string content = System.IO.File.ReadAllText(prefsPath);

            // Simple XML parsing - could be improved
            Debug.LogWarning("Linux PlayerPrefs parsing needs enhancement for accurate type detection");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to read PlayerPrefs from Linux prefs file: {e.Message}");
        }
    }
#endif

    private string GetPlayerPrefsLocation()
    {
#if UNITY_EDITOR_WIN
        return $"Registry: HKCU\\Software\\{Application.companyName}\\{Application.productName}";
#elif UNITY_EDITOR_OSX
        return $"~/Library/Preferences/unity.{Application.companyName}.{Application.productName}.plist";
#elif UNITY_EDITOR_LINUX
        return $"~/.config/unity3d/{Application.companyName}/{Application.productName}/prefs";
#else
        return "Unknown";
#endif
    }
}
