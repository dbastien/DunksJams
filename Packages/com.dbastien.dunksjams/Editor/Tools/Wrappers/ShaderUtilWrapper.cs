using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;

public static class ShaderUtilWrapper
{
    static readonly Type type = Type.GetType("UnityEditor.ShaderUtil, UnityEditor.dll");
    static MethodInfo GetMethod(string name) => type?.GetMethod(name, BindingFlags.Static | BindingFlags.Public);

    static readonly MethodInfo HasShadowCasterPassMethod = GetMethod("HasShadowCasterPass");
    static readonly MethodInfo GetShaderActiveSubshaderIndexMethod = GetMethod("GetShaderActiveSubshaderIndex");
    static readonly MethodInfo GetSRPBatcherCompatibilityCodeMethod = GetMethod("GetSRPBatcherCompatibilityCode");
    static readonly MethodInfo GetVariantCountMethod = GetMethod("GetVariantCount");
    static readonly MethodInfo OpenShaderCombinationsMethod = GetMethod("OpenShaderCombinations");
    static readonly MethodInfo GetShaderGlobalKeywordsMethod = GetMethod("GetShaderGlobalKeywords");
    static readonly MethodInfo GetShaderLocalKeywordsMethod = GetMethod("GetShaderLocalKeywords");
    
    public static bool HasShadowCasterPass(Shader s) => HasShadowCasterPassMethod != null && (bool)HasShadowCasterPassMethod.Invoke(null, new object[] {s});

    public static int GetShaderActiveSubshaderIndex(Shader s) => GetShaderActiveSubshaderIndexMethod != null ? (int)GetShaderActiveSubshaderIndexMethod.Invoke(null, new object[] {s}) : 0;

    public static int GetSRPBatcherCompatibilityCode(Shader s, int subshader) => GetSRPBatcherCompatibilityCodeMethod != null ? (int)GetSRPBatcherCompatibilityCodeMethod.Invoke(null, new object[] { s, subshader }) : -1;

    public static ulong GetVariantCount(Shader s, bool sceneOnly) => GetVariantCountMethod != null ? (ulong)GetVariantCountMethod.Invoke(null, new object[] {s, sceneOnly}) : 0;
    public static void OpenShaderVariations(Shader s, bool sceneOnly) => OpenShaderCombinationsMethod?.Invoke(null, new object[] {s, sceneOnly});
    public static string[] GetShaderGlobalKeywords(Shader s) => GetShaderGlobalKeywordsMethod != null ? (string[])GetShaderGlobalKeywordsMethod.Invoke(null, new object[] {s}) : Array.Empty<string>();

    public static string[] GetShaderLocalKeywords(Shader s) => GetShaderLocalKeywordsMethod != null ? (string[])GetShaderLocalKeywordsMethod.Invoke(null, new object[] {s}) : Array.Empty<string>();

    public static bool IsSRPBatcherCompatible(Shader s)
    {
        if (RenderPipelineManager.currentPipeline == null) return false;
        var compatCheckMaterial = new Material(s);
        if (!compatCheckMaterial) return false; //todo: when does this occur?
        compatCheckMaterial.SetPass(0);
        return GetSRPBatcherCompatibilityCode(s, GetShaderActiveSubshaderIndex(s)) == 0;
    }

    public static string FormatCount(ulong c)
    {
        const ulong k = 1000;
        const ulong m = k * k;
        const ulong b = k * m;
        const ulong t = k * b;
        const ulong q = k * t;

        var d = (double)c;

        return c switch
        {
            > q => $"{d/q:f2}Q",
            > t => $"{d/t:f2}T",
            > b => $"{d/b:f2}B",
            > m => $"{d/m:f2}M",
            > k => $"{d/k:f2}K",
            _ => d.ToString("f0")
        };
    }
}