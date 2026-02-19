using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

public class BenchmarkWindow : EditorWindow
{
    private Assembly[] _assemblies;
    private List<Type> _availableClasses = new();
    private List<MethodInfo> _methods = new();
    private HashSet<MethodInfo> _selectedMethods = new();
    private HashSet<string> _favoriteAssemblies = new();
    private string _assemblyFilter = "", _classFilter = "", _methodFilter = "", _globalClassSearch = "";
    private int _selectedAssemblyIndex, _iterations = 100000;
    private Vector2 _assemblyScrollPos, _classScrollPos, _methodScrollPos;
    private Type _targetClassType;
    private Dictionary<string, bool> _assemblyFoldouts = new();
    private Dictionary<string, Assembly[]> _cachedAssemblyGroups;
    private string _lastAssemblyFilter;
    public static void ShowWindow() => GetWindow<BenchmarkWindow>("Method Benchmark");

    private void OnEnable()
    {
        LoadAssemblies();
        LoadFavorites();
    }

    private void OnGUI()
    {
        using (new GUIScope(fontSize: 14)) { GUILayout.Label("Benchmark Methods", EditorStyles.boldLabel); }

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

        foreach (KeyValuePair<string, Assembly[]> group in _cachedAssemblyGroups)
        {
            _assemblyFoldouts[group.Key] = EditorGUILayout.Foldout(
                _assemblyFoldouts.ContainsKey(group.Key) && _assemblyFoldouts[group.Key],
                $"{group.Key} ({group.Value.Length})"
            );

            if (!_assemblyFoldouts[group.Key]) continue;

            foreach (Assembly assembly in group.Value)
                using (new GUIHorizontalScope())
                {
                    if (GUILayout.Button($"{(IsFavorite(assembly) ? "★ " : "")}{assembly.GetName().Name}",
                            EditorStyles.miniButton))
                    {
                        _selectedAssemblyIndex = Array.IndexOf(_assemblies, assembly);
                        LoadClassesFromAssembly();
                        _classScrollPos = Vector2.zero;
                    }

                    if (GUILayout.Button("☆", GUILayout.Width(20))) ToggleFavorite(assembly);
                }
        }

        EditorGUILayout.EndScrollView();

        if (_availableClasses.Count == 0) return;

        _classFilter = EditorGUILayout.TextField("Class Filter", _classFilter);
        using (new GUIScope(fontSize: 12)) { GUILayout.Label($"Classes ({_availableClasses.Count})"); }

        _classScrollPos = EditorGUILayout.BeginScrollView(_classScrollPos, GUILayout.Height(150));
        for (var i = 0; i < _availableClasses.Count; ++i)
        {
            Type type = _availableClasses[i];
            if (!type.Name.Contains(_classFilter, StringComparison.OrdinalIgnoreCase) ||
                !GUILayout.Button(type.FullName, EditorStyles.miniButton)) continue;
            _targetClassType = type;
            LoadMethods();
            _methodScrollPos = Vector2.zero;
        }

        EditorGUILayout.EndScrollView();

        if (_methods.Count == 0) return;

        _methodFilter = EditorGUILayout.TextField("Method Filter", _methodFilter);
        using (new GUIScope(fontSize: 12)) { GUILayout.Label($"Methods ({_methods.Count})"); }

        ShowMultiSelect(_methods);

        _iterations = EditorGUILayout.IntField("Iterations", Mathf.Max(1, _iterations));
        if (GUILayout.Button("Run") && _selectedMethods.Count > 0) RunBenchmark();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void LoadAssemblies()
    {
        _assemblies = AppDomain.CurrentDomain.GetAssemblies();
        _cachedAssemblyGroups = null;
    }

    private void LoadClassesFromAssembly()
    {
        Assembly selectedAssembly = _assemblies[_selectedAssemblyIndex];
        _availableClasses.Clear();
        foreach (Type type in selectedAssembly.GetTypes())
            if (type.IsClass &&
                type.GetMethods(BindingFlags.Public | BindingFlags.Static).Length > 0 &&
                !type.Name.StartsWith("<"))
                _availableClasses.Add(type);
    }

    private void LoadMethods()
    {
        _methods.Clear();
        _methods.AddRange(_targetClassType.GetMethods(BindingFlags.Public | BindingFlags.Static));
        _selectedMethods.Clear();
    }

    private Dictionary<string, Assembly[]> GetAssemblyGroups()
    {
        var groups = new Dictionary<string, List<Assembly>>();
        foreach (Assembly assembly in _assemblies)
        {
            if (!assembly.GetName().Name.Contains(_assemblyFilter, StringComparison.OrdinalIgnoreCase)) continue;
            string groupName = assembly.GetName().Name.Split('.')[0];
            if (!groups.ContainsKey(groupName)) groups[groupName] = new List<Assembly>();
            groups[groupName].Add(assembly);
        }

        return groups.ToDictionary(g => g.Key, g => g.Value.ToArray());
    }

    private void ShowGlobalClassSearchResults()
    {
        using (new GUIScope(fontSize: 12)) { GUILayout.Label("Global Search Results"); }

        _classScrollPos = EditorGUILayout.BeginScrollView(_classScrollPos, GUILayout.Height(150));
        foreach (Type type in _assemblies.SelectMany(a => a.GetTypes()))
        {
            if (!type.Name.Contains(_globalClassSearch, StringComparison.OrdinalIgnoreCase) ||
                !GUILayout.Button(type.FullName, EditorStyles.miniButton)) continue;
            _targetClassType = type;
            LoadMethods();
            _methodScrollPos = Vector2.zero;
        }

        EditorGUILayout.EndScrollView();
    }

    private void ToggleFavorite(Assembly assembly)
    {
        if (!_favoriteAssemblies.Remove(assembly.GetName().Name)) _favoriteAssemblies.Add(assembly.GetName().Name);
        SaveFavorites();
    }

    private void LoadFavorites() => _favoriteAssemblies =
        new HashSet<string>(EditorPrefs.GetString("BenchmarkFavorites", "").Split(';'));

    private void SaveFavorites() => EditorPrefs.SetString("BenchmarkFavorites", string.Join(";", _favoriteAssemblies));
    private bool IsFavorite(Assembly assembly) => _favoriteAssemblies.Contains(assembly.GetName().Name);

    private void ShowMultiSelect(List<MethodInfo> allMethods)
    {
        _methodScrollPos = EditorGUILayout.BeginScrollView(_methodScrollPos, GUILayout.ExpandHeight(true));
        foreach (MethodInfo method in allMethods)
        {
            bool isSelected = _selectedMethods.Contains(method);
            if (EditorGUILayout.ToggleLeft(method.Name, isSelected) == isSelected) continue;
            if (isSelected) _selectedMethods.Remove(method);
            else _selectedMethods.Add(method);
        }

        EditorGUILayout.EndScrollView();
    }

    private void RunBenchmark()
    {
        foreach (MethodInfo method in _selectedMethods)
        {
            string result = BenchmarkMethod(method);
            DLog.Log($"Benchmark Result for {method.Name}:\n{result}");
        }
    }

    private string BenchmarkMethod(MethodInfo method)
    {
        object[] parameters = method.GetParameters().Select(p => ReflectionUtils.GetDefault(p.ParameterType)).ToArray();

        PrecisionStopwatch sw = new();
        sw.Start();

        for (var i = 0; i < _iterations; ++i)
            ReflectionUtils.InvokeMethod(null, method, parameters);

        sw.Stop();
        return $"{method.Name}: {sw.ElapsedMilliseconds} ms over {_iterations} iterations";
    }
}