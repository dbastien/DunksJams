using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using Utilities;

public class BenchmarkWindow : EditorWindow
{
    Assembly[] _assemblies;
    List<Type> _availableClasses = new();
    List<MethodInfo> _methods = new();
    HashSet<MethodInfo> _selectedMethods = new();
    HashSet<string> _favoriteAssemblies = new();
    string _assemblyFilter = "", _classFilter = "", _methodFilter = "", _globalClassSearch = "";
    int _selectedAssemblyIndex, _iterations = 100000;
    Vector2 _assemblyScrollPos, _classScrollPos, _methodScrollPos;
    Type _targetClassType;
    Dictionary<string, bool> _assemblyFoldouts = new();
    Dictionary<string, Assembly[]> _cachedAssemblyGroups;
    string _lastAssemblyFilter;
    public static void ShowWindow() => GetWindow<BenchmarkWindow>("Method Benchmark");

    void OnEnable()
    {
        LoadAssemblies();
        LoadFavorites();
    }

    void OnGUI()
    {
        using (new GUIScope(fontSize: 14))
            GUILayout.Label("Benchmark Methods", EditorStyles.boldLabel);

        _globalClassSearch = EditorGUILayout.TextField("Global Class Search", _globalClassSearch);
        if (!string.IsNullOrEmpty(_globalClassSearch))
        {
            ShowGlobalClassSearchResults();
            return;
        }

        _assemblyFilter = EditorGUILayout.TextField("Assembly Filter", _assemblyFilter);
        if (_cachedAssemblyGroups == null || _assemblyFilter != _lastAssemblyFilter)
        {
            _cachedAssemblyGroups = GetAssemblyGroups();
            _lastAssemblyFilter = _assemblyFilter;
        }

        _assemblyScrollPos = EditorGUILayout.BeginScrollView(_assemblyScrollPos, GUILayout.Height(150));

        foreach (var group in _cachedAssemblyGroups)
        {
            _assemblyFoldouts[group.Key] = EditorGUILayout.Foldout(
                _assemblyFoldouts.ContainsKey(group.Key) && _assemblyFoldouts[group.Key],
                $"{group.Key} ({group.Value.Length})"
            );

            if (!_assemblyFoldouts[group.Key]) continue;

            foreach (var assembly in group.Value)
            {
                using (new GUIHorizontalScope())
                {
                    if (GUILayout.Button($"{(IsFavorite(assembly) ? "★ " : "")}{assembly.GetName().Name}", EditorStyles.miniButton))
                    {
                        _selectedAssemblyIndex = Array.IndexOf(_assemblies, assembly);
                        LoadClassesFromAssembly();
                        _classScrollPos = Vector2.zero;
                    }
                    if (GUILayout.Button("☆", GUILayout.Width(20))) ToggleFavorite(assembly);
                }
            }
        }

        EditorGUILayout.EndScrollView();

        if (_availableClasses.Count == 0) return;

        _classFilter = EditorGUILayout.TextField("Class Filter", _classFilter);
        using (new GUIScope(fontSize: 12))
            GUILayout.Label($"Classes ({_availableClasses.Count})");

        _classScrollPos = EditorGUILayout.BeginScrollView(_classScrollPos, GUILayout.Height(150));
        for (int i = 0; i < _availableClasses.Count; ++i)
        {
            var type = _availableClasses[i];
            if (!type.Name.Contains(_classFilter, StringComparison.OrdinalIgnoreCase) ||
                !GUILayout.Button(type.FullName, EditorStyles.miniButton)) continue;
            _targetClassType = type;
            LoadMethods();
            _methodScrollPos = Vector2.zero;
        }
        EditorGUILayout.EndScrollView();

        if (_methods.Count == 0) return;

        _methodFilter = EditorGUILayout.TextField("Method Filter", _methodFilter);
        using (new GUIScope(fontSize: 12))
            GUILayout.Label($"Methods ({_methods.Count})");

        ShowMultiSelect(_methods);

        _iterations = EditorGUILayout.IntField("Iterations", Mathf.Max(1, _iterations));
        if (GUILayout.Button("Run") && _selectedMethods.Count > 0) RunBenchmark();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void LoadAssemblies()
    {
        _assemblies = AppDomain.CurrentDomain.GetAssemblies();
        _cachedAssemblyGroups = null;
    }

    void LoadClassesFromAssembly()
    {
        var selectedAssembly = _assemblies[_selectedAssemblyIndex];
        _availableClasses.Clear();
        foreach (var type in selectedAssembly.GetTypes())
            if (type.IsClass && type.GetMethods(BindingFlags.Public | BindingFlags.Static).Length > 0 && !type.Name.StartsWith("<"))
                _availableClasses.Add(type);
    }

    void LoadMethods()
    {
        _methods.Clear();
        _methods.AddRange(_targetClassType.GetMethods(BindingFlags.Public | BindingFlags.Static));
        _selectedMethods.Clear();
    }

    Dictionary<string, Assembly[]> GetAssemblyGroups()
    {
        var groups = new Dictionary<string, List<Assembly>>();
        foreach (var assembly in _assemblies)
        {
            if (!assembly.GetName().Name.Contains(_assemblyFilter, StringComparison.OrdinalIgnoreCase)) continue;
            var groupName = assembly.GetName().Name.Split('.')[0];
            if (!groups.ContainsKey(groupName)) groups[groupName] = new List<Assembly>();
            groups[groupName].Add(assembly);
        }
        return groups.ToDictionary(g => g.Key, g => g.Value.ToArray());
    }

    void ShowGlobalClassSearchResults()
    {
        using (new GUIScope(fontSize: 12))
            GUILayout.Label("Global Search Results");

        _classScrollPos = EditorGUILayout.BeginScrollView(_classScrollPos, GUILayout.Height(150));
        foreach (var type in _assemblies.SelectMany(a => a.GetTypes()))
        {
            if (!type.Name.Contains(_globalClassSearch, StringComparison.OrdinalIgnoreCase) ||
                !GUILayout.Button(type.FullName, EditorStyles.miniButton)) continue;
            _targetClassType = type;
            LoadMethods();
            _methodScrollPos = Vector2.zero;
        }
        EditorGUILayout.EndScrollView();
    }

    void ToggleFavorite(Assembly assembly)
    {
        if (!_favoriteAssemblies.Remove(assembly.GetName().Name)) _favoriteAssemblies.Add(assembly.GetName().Name);
        SaveFavorites();
    }

    void LoadFavorites() => _favoriteAssemblies = new HashSet<string>(EditorPrefs.GetString("BenchmarkFavorites", "").Split(';'));
    void SaveFavorites() => EditorPrefs.SetString("BenchmarkFavorites", string.Join(";", _favoriteAssemblies));
    bool IsFavorite(Assembly assembly) => _favoriteAssemblies.Contains(assembly.GetName().Name);

    void ShowMultiSelect(List<MethodInfo> allMethods)
    {
        _methodScrollPos = EditorGUILayout.BeginScrollView(_methodScrollPos, GUILayout.ExpandHeight(true));
        foreach (var method in allMethods)
        {
            bool isSelected = _selectedMethods.Contains(method);
            if (EditorGUILayout.ToggleLeft(method.Name, isSelected) == isSelected) continue;
            if (isSelected) _selectedMethods.Remove(method);
            else _selectedMethods.Add(method);
        }
        EditorGUILayout.EndScrollView();
    }

    void RunBenchmark()
    {
        foreach (var method in _selectedMethods)
        {
            string result = BenchmarkMethod(method);
            DLog.Log($"Benchmark Result for {method.Name}:\n{result}");
        }
    }

    string BenchmarkMethod(MethodInfo method)
    {
        object[] parameters = method.GetParameters().Select(p => ReflectionUtils.GetDefault(p.ParameterType)).ToArray();

        PrecisionStopwatch sw = new();
        sw.Start();

        for (int i = 0; i < _iterations; ++i)
            ReflectionUtils.InvokeMethod(null, method, parameters);

        sw.Stop();
        return $"{method.Name}: {sw.ElapsedMilliseconds} ms over {_iterations} iterations";
    }
}