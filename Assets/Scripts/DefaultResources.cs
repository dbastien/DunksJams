using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;

public static class DefaultResources
{
    static readonly Lazy<Material> _material = new(() =>
        #if UNITY_EDITOR
            AssetDatabase.GetBuiltinExtraResource<Material>("Default-Material.mat") ?? new Material(Shader.Find("Standard"))
        #else
            new Material(Shader.Find("Standard"))
        #endif
    );

    static readonly Lazy<Material> _particleMaterial = new(() =>
        #if UNITY_EDITOR
            AssetDatabase.GetBuiltinExtraResource<Material>("Default-Particle.mat")
        #else
            new Material(Shader.Find("Particles/Standard Unlit"))
        #endif
    );

    static readonly Lazy<Material> _lineMaterial = new(() =>
        #if UNITY_EDITOR
            AssetDatabase.GetBuiltinExtraResource<Material>("Default-Line.mat")
        #else
            new Material(Shader.Find("Sprites/Default"))
        #endif
    );

    static readonly Lazy<Material> _terrainMaterial = new(() =>
        #if UNITY_EDITOR
            AssetDatabase.GetBuiltinExtraResource<Material>("Default-Terrain-Standard.mat")
        #else
            new Material(Shader.Find("Nature/Terrain/Standard"))
        #endif
    );

    static readonly Lazy<Material> _uiMaterial = new(() =>
        #if UNITY_EDITOR
            AssetDatabase.GetBuiltinExtraResource<Material>("Default-UI.mat")
        #else
            new Material(Shader.Find("UI/Default"))
        #endif
    );

    static readonly Lazy<Shader> _shader = new(() =>
        Shader.Find("Standard") ?? Shader.Find("Hidden/InternalErrorShader")
    );

    static readonly Lazy<Material> _skybox = new(() =>
        #if UNITY_EDITOR
            AssetDatabase.GetBuiltinExtraResource<Material>("Skybox-Default.mat")
        #else
            new Material(Shader.Find("Skybox/Procedural"))
        #endif
    );

    static readonly Lazy<Material> _spriteMaterial = new(() =>
        #if UNITY_EDITOR
            AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat")
        #else
            new Material(Shader.Find("Sprites/Default"))
        #endif
    );

    static readonly Lazy<Material> _material2D = new(() =>
        #if UNITY_EDITOR
            AssetDatabase.GetBuiltinExtraResource<Material>("Default-2D.mat")
        #else
            new Material(Shader.Find("Sprites/Default"))
        #endif
    );

    static readonly Lazy<Shader> _particleShader = new(() =>
        Shader.Find("Particles/Standard Unlit") ?? Shader.Find("Hidden/InternalErrorShader")
    );

    static readonly Lazy<Texture> _cubeTexture = new(() =>
        #if UNITY_EDITOR
            AssetDatabase.GetBuiltinExtraResource<Texture>("Default-CubeTexture.png")
        #else
            Texture2D.blackTexture
        #endif
    );

    static readonly Lazy<Texture> _texture2D = new(() =>
        #if UNITY_EDITOR
            AssetDatabase.GetBuiltinExtraResource<Texture>("Default-2DTexture.png")
        #else
            Texture2D.whiteTexture
        #endif
    );

    public static Material Material => _material.Value;
    public static Material ParticleMaterial => _particleMaterial.Value;
    public static Material LineMaterial => _lineMaterial.Value;
    public static Material TerrainMaterial => _terrainMaterial.Value;
    public static Material UIMaterial => _uiMaterial.Value;
    public static Shader Shader => _shader.Value;
    public static Material Skybox => _skybox.Value;
    public static Material SpriteMaterial => _spriteMaterial.Value;
    public static Material Material2D => _material2D.Value;
    public static Shader ParticleShader => _particleShader.Value;
    public static Texture CubeTexture => _cubeTexture.Value;
    public static Texture Texture2D => _texture2D.Value;
}