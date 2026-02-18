#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

internal static class SingletonBehaviourResetter
{
    private static readonly HashSet<Type> s_reset = new();

    [InitializeOnEnterPlayMode]
    private static void OnEnterPlayMode(EnterPlayModeOptions options)
    {
        if ((options & EnterPlayModeOptions.DisableDomainReload) == 0)
            return;

        s_reset.Clear();

        TypeCache.TypeCollection types = TypeCache.GetTypesDerivedFrom<MonoBehaviour>();
        for (var i = 0; i < types.Count; i++)
        {
            Type type = types[i];
            if (type == null) continue;

            Type baseType = type;
            while (baseType != null)
            {
                if (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == typeof(SingletonBehaviour<>))
                {
                    if (!baseType.ContainsGenericParameters && s_reset.Add(baseType))
                        ResetSingletonBase(baseType);
                    break;
                }

                baseType = baseType.BaseType;
            }
        }
    }

    private static void ResetSingletonBase(Type closedBase)
    {
        const BindingFlags flags = BindingFlags.Static | BindingFlags.NonPublic;
        FieldInfo instanceField = closedBase.GetField("_instance", flags);
        if (instanceField != null)
            instanceField.SetValue(null, null);

        FieldInfo quittingField = closedBase.GetField("_quitting", flags);
        if (quittingField != null)
            quittingField.SetValue(null, false);
    }
}
#endif