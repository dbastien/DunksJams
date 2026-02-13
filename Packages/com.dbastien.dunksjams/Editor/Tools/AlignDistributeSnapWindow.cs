using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class AlignDistributeSnapWindow : EditorWindow
{
    float _spacing = 1f, _snapValue = 1f;
    Vector3 _alignmentOffset = Vector3.zero;
    bool _snapToGrid, _uniformDistribution;

    [MenuItem("‽/Align, Distribute, Snap")]
    static void Init() => GetWindow<AlignDistributeSnapWindow>("Align, Distribute, Snap");

    void OnGUI()
    {
        _snapValue = EditorGUILayout.Slider("Snap Value", _snapValue, 0.1f, 10f);
        if (GUILayout.Button("Snap All Selected")) SnapSelected();

        EditorGUILayout.Space(5);
        _alignmentOffset = EditorGUILayout.Vector3Field("Alignment Offset", _alignmentOffset);
        _snapToGrid = EditorGUILayout.Toggle("Snap to Grid", _snapToGrid);
        if (GUILayout.Button("Align All")) Align(_alignmentOffset);

        EditorGUILayout.Space(5);
        _spacing = EditorGUILayout.Slider("Spacing", _spacing, 0f, 10f);
        _uniformDistribution = EditorGUILayout.Toggle("Uniform Distribution", _uniformDistribution);
        if (GUILayout.Button("Distribute X")) Distribute(Vector3.right);
        if (GUILayout.Button("Distribute Y")) Distribute(Vector3.up);
        if (GUILayout.Button("Distribute Z")) Distribute(Vector3.forward);
    }

    void SnapSelected()
    {
        Undo.RecordObjects(Selection.transforms, "Snap Objects");
        foreach (var t in Selection.transforms) t.position = SnapToGrid(t.position);
    }

    void Align(Vector3 offset)
    {
        Undo.RecordObjects(Selection.transforms, "Align Objects");
        var pos = Selection.transforms[0].position;
        foreach (var t in Selection.transforms) t.position = Snap(pos + offset);
    }

    void Distribute(Vector3 axis)
    {
        Undo.RecordObjects(Selection.transforms, "Distribute Objects");
        var sorted = Selection.transforms.OrderBy(t => Vector3.Dot(t.position, axis)).ToArray();
        for (var i = 1; i < sorted.Length; ++i)
        {
            sorted[i].position = Snap(sorted[i - 1].position + axis * (_uniformDistribution
                ? _spacing
                : Vector3.Distance(sorted[i - 1].position, sorted[i].position)));
        }
    }

    Vector3 Snap(Vector3 pos) => _snapToGrid ? SnapToGrid(pos) : pos;

    Vector3 SnapToGrid(Vector3 pos) =>
        new(MathF.Round(pos.x / _snapValue) * _snapValue,
            MathF.Round(pos.y / _snapValue) * _snapValue,
            MathF.Round(pos.z / _snapValue) * _snapValue);
}