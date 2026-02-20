using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class EditorPrefsViewerWindow : EditorWindow
{
    private Vector2 _scrollPosition;
    private string _searchFilter = "";
    private List<EditorPrefEntry> _entries = new();
    private bool _showInternalPrefs = false;
    private EditorPrefEntry _editingEntry;
    private string _editingValue;

    private enum PrefType { String, Int, Float, Bool }

    private class EditorPrefEntry
    {
        public string Key;
        public object Value;
        public PrefType Type;
        public bool IsInternal => Key.StartsWith("unity.", StringComparison.OrdinalIgnoreCase) ||
                                   Key.StartsWith("UnityEditor.", StringComparison.OrdinalIgnoreCase);
    }

    [MenuItem("‽/Tools/EditorPrefs Viewer")]
    public static void ShowWindow()
    {
        var window = GetWindow<EditorPrefsViewerWindow>("EditorPrefs Viewer");
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

        GUILayout.Space(10);

        // Show internal prefs toggle
        EditorGUI.BeginChangeCheck();
        _showInternalPrefs = GUILayout.Toggle(_showInternalPrefs, "Show Unity Internal", EditorStyles.toolbarButton);
        if (EditorGUI.EndChangeCheck())
        {
            RefreshEntries();
        }

        GUILayout.FlexibleSpace();

        // Refresh button
        if (GUILayout.Button("Refresh", EditorStyles.toolbarButton))
        {
            RefreshEntries();
        }

        // Delete all button
        GUI.backgroundColor = new Color(1f, 0.5f, 0.5f);
        if (GUILayout.Button("Delete All", EditorStyles.toolbarButton))
        {
            if (EditorUtility.DisplayDialog("Delete All EditorPrefs",
                "Are you sure you want to delete ALL EditorPrefs? This cannot be undone!",
                "Delete All", "Cancel"))
            {
                EditorPrefs.DeleteAll();
                RefreshEntries();
            }
        }
        GUI.backgroundColor = Color.white;

        EditorGUILayout.EndHorizontal();
    }

    private void DrawEntries()
    {
        if (_entries == null || _entries.Count == 0)
        {
            EditorGUILayout.HelpBox("No EditorPrefs found.", MessageType.Info);
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
            EditorPrefEntry entry = _entries[i];
            DrawEntry(entry, i);
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();

        // Status bar
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUILayout.Label($"Showing {_entries.Count} entries", EditorStyles.miniLabel);
        EditorGUILayout.EndHorizontal();
    }

    private void DrawEntry(EditorPrefEntry entry, int index)
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
                if (EditorUtility.DisplayDialog("Delete EditorPref",
                    $"Delete key '{entry.Key}'?", "Delete", "Cancel"))
                {
                    EditorPrefs.DeleteKey(entry.Key);
                    RefreshEntries();
                }
            }
            GUI.backgroundColor = Color.white;
        }

        EditorGUILayout.EndHorizontal();
    }

    private void SaveEdit(EditorPrefEntry entry)
    {
        try
        {
            switch (entry.Type)
            {
                case PrefType.String:
                    EditorPrefs.SetString(entry.Key, _editingValue);
                    break;

                case PrefType.Int:
                    if (int.TryParse(_editingValue, out int intValue))
                        EditorPrefs.SetInt(entry.Key, intValue);
                    else
                        EditorUtility.DisplayDialog("Invalid Value", "Value must be an integer.", "OK");
                    break;

                case PrefType.Float:
                    if (float.TryParse(_editingValue, out float floatValue))
                        EditorPrefs.SetFloat(entry.Key, floatValue);
                    else
                        EditorUtility.DisplayDialog("Invalid Value", "Value must be a float.", "OK");
                    break;

                case PrefType.Bool:
                    if (bool.TryParse(_editingValue, out bool boolValue))
                        EditorPrefs.SetBool(entry.Key, boolValue);
                    else
                        EditorUtility.DisplayDialog("Invalid Value", "Value must be true or false.", "OK");
                    break;
            }

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

        // Get all EditorPrefs keys using reflection
        // EditorPrefs doesn't expose a GetAll method, so we use platform-specific registry/plist access
#if UNITY_EDITOR_WIN
        RefreshEntriesWindows();
#elif UNITY_EDITOR_OSX
        RefreshEntriesMac();
#else
        EditorUtility.DisplayDialog("Unsupported Platform",
            "EditorPrefs viewer is only supported on Windows and Mac.", "OK");
#endif

        // Apply search filter
        if (!string.IsNullOrEmpty(_searchFilter))
        {
            _entries = _entries.Where(e =>
                e.Key.IndexOf(_searchFilter, StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();
        }

        // Filter internal prefs
        if (!_showInternalPrefs)
        {
            _entries = _entries.Where(e => !e.IsInternal).ToList();
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
            string registryPath = $"Software\\Unity\\UnityEditor\\{companyName}\\{productName}";

            using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(registryPath);
            if (key == null) return;

            foreach (string valueName in key.GetValueNames())
            {
                object value = key.GetValue(valueName);

                // Try to determine type and read value
                if (EditorPrefs.HasKey(valueName + "_h3320113278"))
                {
                    _entries.Add(new EditorPrefEntry
                    {
                        Key = valueName,
                        Value = EditorPrefs.GetString(valueName),
                        Type = PrefType.String
                    });
                }
                else if (value is int intVal)
                {
                    // Could be int or bool
                    if (intVal == 0 || intVal == 1)
                    {
                        _entries.Add(new EditorPrefEntry
                        {
                            Key = valueName,
                            Value = EditorPrefs.GetBool(valueName),
                            Type = PrefType.Bool
                        });
                    }
                    else
                    {
                        _entries.Add(new EditorPrefEntry
                        {
                            Key = valueName,
                            Value = EditorPrefs.GetInt(valueName),
                            Type = PrefType.Int
                        });
                    }
                }
                else if (value is byte[] bytes && bytes.Length == 4)
                {
                    _entries.Add(new EditorPrefEntry
                    {
                        Key = valueName,
                        Value = EditorPrefs.GetFloat(valueName),
                        Type = PrefType.Float
                    });
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to read EditorPrefs from registry: {e.Message}");
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
            string plistFile = $"~/Library/Preferences/com.unity3d.UnityEditor5.x.plist";

            // Use defaults command to read plist
            var process = new System.Diagnostics.Process
            {
                StartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "defaults",
                    Arguments = $"read {plistFile}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            // Parse plist output (simplified - may need improvement)
            // This is a basic implementation
            Debug.LogWarning("Mac EditorPrefs parsing needs enhancement for accurate type detection");

        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to read EditorPrefs from plist: {e.Message}");
        }
    }
#endif
}
