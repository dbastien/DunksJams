using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[SingletonAutoCreate]
public class GroundSurfaceManager : SingletonEagerBehaviour<GroundSurfaceManager>
{
    [SerializeField] private GroundSurface defaultSurface;
    [SerializeField] private List<PhysicsMaterialMapping> mappings = new();

    private readonly LRUCache<int, GroundSurface> _cache = new(64);

    [System.Serializable]
    public class PhysicsMaterialMapping
    {
        public PhysicsMaterial physicsMaterial;
        public GroundSurface groundSurface;
    }

    protected override void InitInternal()
    {
        foreach (PhysicsMaterialMapping m in mappings)
            if (m.physicsMaterial != null && m.groundSurface != null)
                _cache.Set(m.physicsMaterial.GetInstanceID(), m.groundSurface);
    }

    public GroundSurface GetSurface(PhysicsMaterial mat)
    {
        if (mat == null) return defaultSurface;

        int id = mat.GetInstanceID();
        if (_cache.TryGetValue(id, out GroundSurface surface)) return surface;

        return defaultSurface;
    }

    public GroundSurface GetSurface(Collider collider)
    {
        if (collider == null) return defaultSurface;
        return GetSurface(collider.sharedMaterial);
    }

    public GroundSurface DefaultSurface => defaultSurface;
}