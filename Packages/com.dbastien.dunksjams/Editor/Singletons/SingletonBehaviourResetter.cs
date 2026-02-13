#if UNITY_EDITOR
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using UnityEditor;
    using UnityEngine;

    static class SingletonBehaviourResetter
    {
        static readonly HashSet<Type> s_reset = new();

        [InitializeOnEnterPlayMode]
        static void OnEnterPlayMode(EnterPlayModeOptions options)
        {
            if ((options & EnterPlayModeOptions.DisableDomainReload) == 0)
                return;

            s_reset.Clear();

            var types = TypeCache.GetTypesDerivedFrom<MonoBehaviour>();
            for (var i = 0; i < types.Count; i++)
            {
                var type = types[i];
                if (type == null) continue;

                var baseType = type;
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

        static void ResetSingletonBase(Type closedBase)
        {
            const BindingFlags flags = BindingFlags.Static | BindingFlags.NonPublic;
            var instanceField = closedBase.GetField("_instance", flags);
            if (instanceField != null)
                instanceField.SetValue(null, null);

            var quittingField = closedBase.GetField("_quitting", flags);
            if (quittingField != null)
                quittingField.SetValue(null, false);
        }
    }
#endif