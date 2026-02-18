using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public static class GeneralSettingsProvider
{
    [SettingsProvider]
    public static SettingsProvider CreateGeneralSettingsProvider()
    {
        List<Type> sectionTypes = ReflectionUtils.GetNonGenericDerivedTypes<SettingsProviderSectionAttribute>();
        IEnumerable<SettingsProvider> providers = sectionTypes.Select(CreateSettingsProviderForSection);
        return providers.FirstOrDefault();
    }

    private static SettingsProvider CreateSettingsProviderForSection(Type sectionType)
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

    private static void DrawSettingsFields(Type settingsType)
    {
        Dictionary<MemberInfo, SettingsProviderFieldAttribute> membersWithAttributes =
            settingsType.GetAttributesForMembers<SettingsProviderFieldAttribute>();
        foreach ((MemberInfo member, SettingsProviderFieldAttribute fieldAttr) in membersWithAttributes)
            DrawField(member.GetValue(null).GetType(), fieldAttr.Label,
                () => member.GetValue(null),
                value => member.SetValue(null, value));
    }

    private static void DrawField(Type fieldType, string label, Func<object> getter, Action<object> setter)
    {
        object value = getter();
        switch (value)
        {
            case Font font:
                var newFont = (Font)EditorGUILayout.ObjectField(label, font, typeof(Font), false);
                if (newFont != font) setter(newFont);
                break;
            case int intValue:
                int newInt = EditorGUILayout.IntField(label, intValue);
                if (newInt != intValue) setter(newInt);
                break;
            case float floatValue:
                float newFloat = EditorGUILayout.FloatField(label, floatValue);
                if (!Mathf.Approximately(newFloat, floatValue)) setter(newFloat);
                break;
            case Color colorValue:
                Color newColor = EditorGUILayout.ColorField(label, colorValue);
                if (newColor != colorValue) setter(newColor);
                break;
            case bool boolValue:
                bool newBool = EditorGUILayout.Toggle(label, boolValue);
                if (newBool != boolValue) setter(newBool);
                break;
            case Enum enumValue:
                Enum newEnum = EditorGUILayout.EnumPopup(label, enumValue);
                if (!Equals(newEnum, enumValue)) setter(newEnum);
                break;
            default:
                EditorGUILayout.LabelField($"{label} (Unsupported type: {fieldType.Name})", EditorStyles.boldLabel);
                break;
        }
    }

    public static object GetSettingValue(string settingName)
    {
        List<Type> sectionTypes = ReflectionUtils.GetNonGenericDerivedTypes<SettingsProviderSectionAttribute>();
        foreach (Type type in sectionTypes)
        {
            Dictionary<MemberInfo, SettingsProviderFieldAttribute> membersWithAttributes =
                type.GetAttributesForMembers<SettingsProviderFieldAttribute>();
            foreach ((MemberInfo member, SettingsProviderFieldAttribute attr) in membersWithAttributes)
                if (attr.Label.Equals(settingName, StringComparison.OrdinalIgnoreCase))
                    return member.GetValue(null); // Retrieve static field or property value
        }

        DLog.LogW($"Setting '{settingName}' not found.");
        return null;
    }

    public static Dictionary<string, object> GetAllSettings()
    {
        var settings = new Dictionary<string, object>();
        List<Type> sectionTypes = ReflectionUtils.GetNonGenericDerivedTypes<SettingsProviderSectionAttribute>();

        foreach (Type type in sectionTypes)
        {
            Dictionary<MemberInfo, SettingsProviderFieldAttribute> membersWithAttributes =
                type.GetAttributesForMembers<SettingsProviderFieldAttribute>();
            foreach ((MemberInfo member, SettingsProviderFieldAttribute attr) in membersWithAttributes)
            {
                object value = member.GetValue(null);
                settings[attr.Label] = value;
            }
        }

        return settings;
    }
}