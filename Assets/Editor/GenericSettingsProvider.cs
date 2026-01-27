using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public static class GeneralSettingsProvider
{
    [SettingsProvider]
    public static SettingsProvider CreateGeneralSettingsProvider()
    {
        var sectionTypes = ReflectionUtils.GetNonGenericDerivedTypes<SettingsProviderSectionAttribute>();
        var providers = sectionTypes.Select(CreateSettingsProviderForSection);
        return providers.FirstOrDefault();
    }

    static SettingsProvider CreateSettingsProviderForSection(Type sectionType)
    {
        var sectionAttr = sectionType.GetAttribute<SettingsProviderSectionAttribute>();
        if (sectionAttr == null) return null;
        return new SettingsProvider(sectionAttr.Path, SettingsScope.Project)
        {
            label = sectionAttr.Label,
            activateHandler = (_, rootElement) =>
            {
                rootElement.Add(new IMGUIContainer(() =>
                {
                    EditorGUILayout.LabelField(sectionAttr.Label, EditorStyles.boldLabel);
                    DrawSettingsFields(sectionType);
                }));
            }
        };
    }

    static void DrawSettingsFields(Type settingsType)
    {
        var membersWithAttributes = settingsType.GetAttributesForMembers<SettingsProviderFieldAttribute>();
        foreach (var (member, fieldAttr) in membersWithAttributes)
        {
            DrawField(member.GetValue(null).GetType(), fieldAttr.Label,
                () => member.GetValue(null),
                value => member.SetValue(null, value));
        }
    }

    static void DrawField(Type fieldType, string label, Func<object> getter, Action<object> setter)
    {
        var value = getter();
        switch (value)
        {
            case Font font:
                var newFont = (Font)EditorGUILayout.ObjectField(label, font, typeof(Font), false);
                if (newFont != font) setter(newFont);
                break;
            case int intValue:
                var newInt = EditorGUILayout.IntField(label, intValue);
                if (newInt != intValue) setter(newInt);
                break;
            case float floatValue:
                var newFloat = EditorGUILayout.FloatField(label, floatValue);
                if (!Mathf.Approximately(newFloat, floatValue)) setter(newFloat);
                break;
            case Color colorValue:
                var newColor = EditorGUILayout.ColorField(label, colorValue);
                if (newColor != colorValue) setter(newColor);
                break;
            case bool boolValue:
                var newBool = EditorGUILayout.Toggle(label, boolValue);
                if (newBool != boolValue) setter(newBool);
                break;
            case Enum enumValue:
                var newEnum = EditorGUILayout.EnumPopup(label, enumValue);
                if (!Equals(newEnum, enumValue)) setter(newEnum);
                break;
            default:
                EditorGUILayout.LabelField($"{label} (Unsupported type: {fieldType.Name})", EditorStyles.boldLabel);
                break;
        }
    }
    
    public static object GetSettingValue(string settingName)
    {
        var sectionTypes = ReflectionUtils.GetNonGenericDerivedTypes<SettingsProviderSectionAttribute>();
        foreach (var type in sectionTypes)
        {
            var membersWithAttributes = type.GetAttributesForMembers<SettingsProviderFieldAttribute>();
            foreach (var (member, attr) in membersWithAttributes)
            {
                if (attr.Label.Equals(settingName, StringComparison.OrdinalIgnoreCase))
                    return member.GetValue(null); // Retrieve static field or property value
            }
        }
    
        DLog.LogW($"Setting '{settingName}' not found.");
        return null;
    }

    public static Dictionary<string, object> GetAllSettings()
    {
        var settings = new Dictionary<string, object>();
        var sectionTypes = ReflectionUtils.GetNonGenericDerivedTypes<SettingsProviderSectionAttribute>();
    
        foreach (var type in sectionTypes)
        {
            var membersWithAttributes = type.GetAttributesForMembers<SettingsProviderFieldAttribute>();
            foreach (var (member, attr) in membersWithAttributes)
            {
                var value = member.GetValue(null);
                settings[attr.Label] = value;
            }
        }

        return settings;
    }
}
