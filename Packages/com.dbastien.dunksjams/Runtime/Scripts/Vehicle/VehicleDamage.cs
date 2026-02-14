using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(VehicleController))]
public class VehicleDamage : MonoBehaviour
{
    [Header("Deformation")]
    [SerializeField] float deformRadius = 0.5f;
    [SerializeField] float maxDeformation = 0.3f;
    [SerializeField] float deformFalloff = 1f;
    [SerializeField] float impulseToDamage = 0.001f;
    [SerializeField] LayerMask deformableMeshLayers = ~0;

    [Header("Health Integration")]
    [SerializeField] bool applyDamageToHealth = true;
    [SerializeField] float impulseToHPDamage = 0.01f;
    [SerializeField] DamageType damageType = DamageType.Physical;

    VehicleController _vehicle;
    Health _health;

    readonly Dictionary<MeshFilter, Vector3[]> _originalVertices = new();

    void Awake()
    {
        _vehicle = GetComponent<VehicleController>();
        TryGetComponent(out _health);
        CacheOriginalMeshes();
    }

    void OnEnable() => _vehicle.OnImpact += HandleImpact;
    void OnDisable() => _vehicle.OnImpact -= HandleImpact;

    void CacheOriginalMeshes()
    {
        var filters = GetComponentsInChildren<MeshFilter>();
        foreach (var mf in filters)
        {
            if (mf.sharedMesh == null) continue;
            // Create unique mesh instance for deformation
            mf.mesh = Instantiate(mf.sharedMesh);
            _originalVertices[mf] = mf.sharedMesh.vertices.Clone() as Vector3[];
        }
    }

    void HandleImpact(Vector3 worldPoint, Vector3 normal, float impulse)
    {
        float deformAmount = impulse * impulseToDamage;
        if (deformAmount < 0.01f) return;
        deformAmount = Mathf.Min(deformAmount, maxDeformation);

        foreach (var kvp in _originalVertices)
        {
            var mf = kvp.Key;
            if (mf == null) continue;

            var mesh = mf.mesh;
            var verts = mesh.vertices;
            var localPoint = mf.transform.InverseTransformPoint(worldPoint);
            var localNormal = mf.transform.InverseTransformDirection(normal);
            bool modified = false;

            for (int i = 0; i < verts.Length; i++)
            {
                float dist = Vector3.Distance(verts[i], localPoint);
                if (dist > deformRadius) continue;

                float falloff = 1f - Mathf.Pow(dist / deformRadius, deformFalloff);
                var displacement = localNormal * (deformAmount * falloff);

                // Clamp total deformation from original position
                var original = kvp.Value[i];
                var newPos = verts[i] + displacement;
                if (Vector3.Distance(newPos, original) > maxDeformation)
                {
                    var dir = (newPos - original).normalized;
                    newPos = original + dir * maxDeformation;
                }

                verts[i] = newPos;
                modified = true;
            }

            if (modified)
            {
                mesh.vertices = verts;
                mesh.RecalculateNormals();
                mesh.RecalculateBounds();
            }
        }

        // Health damage
        if (applyDamageToHealth && _health != null)
        {
            float hpDam = impulse * impulseToHPDamage;
            if (hpDam > 0.5f) _health.TakeDamage(hpDam, damageType);
        }
    }

    public void Repair(float amount01)
    {
        amount01 = Mathf.Clamp01(amount01);

        foreach (var kvp in _originalVertices)
        {
            var mf = kvp.Key;
            if (mf == null) continue;

            var mesh = mf.mesh;
            var verts = mesh.vertices;
            var originals = kvp.Value;

            for (int i = 0; i < verts.Length; i++)
                verts[i] = Vector3.Lerp(verts[i], originals[i], amount01);

            mesh.vertices = verts;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
        }
    }

    public void FullRepair() => Repair(1f);
}
