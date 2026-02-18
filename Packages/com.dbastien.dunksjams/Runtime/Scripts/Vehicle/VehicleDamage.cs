using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(VehicleController))]
public class VehicleDamage : MonoBehaviour
{
    [Header("Deformation")] [SerializeField]
    private float deformRadius = 0.5f;

    [SerializeField] private float maxDeformation = 0.3f;
    [SerializeField] private float deformFalloff = 1f;
    [SerializeField] private float impulseToDamage = 0.001f;
    [SerializeField] private LayerMask deformableMeshLayers = ~0;

    [Header("Health Integration")] [SerializeField]
    private bool applyDamageToHealth = true;

    [SerializeField] private float impulseToHPDamage = 0.01f;
    [SerializeField] private DamageType damageType = DamageType.Physical;

    private VehicleController _vehicle;
    private Health _health;

    private readonly Dictionary<MeshFilter, Vector3[]> _originalVertices = new();

    private void Awake()
    {
        _vehicle = GetComponent<VehicleController>();
        TryGetComponent(out _health);
        CacheOriginalMeshes();
    }

    private void OnEnable() => _vehicle.OnImpact += HandleImpact;
    private void OnDisable() => _vehicle.OnImpact -= HandleImpact;

    private void CacheOriginalMeshes()
    {
        MeshFilter[] filters = GetComponentsInChildren<MeshFilter>();
        foreach (MeshFilter mf in filters)
        {
            if (mf.sharedMesh == null) continue;
            // Create unique mesh instance for deformation
            mf.mesh = Instantiate(mf.sharedMesh);
            _originalVertices[mf] = mf.sharedMesh.vertices.Clone() as Vector3[];
        }
    }

    private void HandleImpact(Vector3 worldPoint, Vector3 normal, float impulse)
    {
        float deformAmount = impulse * impulseToDamage;
        if (deformAmount < 0.01f) return;
        deformAmount = Mathf.Min(deformAmount, maxDeformation);

        foreach (KeyValuePair<MeshFilter, Vector3[]> kvp in _originalVertices)
        {
            MeshFilter mf = kvp.Key;
            if (mf == null) continue;

            Mesh mesh = mf.mesh;
            Vector3[] verts = mesh.vertices;
            Vector3 localPoint = mf.transform.InverseTransformPoint(worldPoint);
            Vector3 localNormal = mf.transform.InverseTransformDirection(normal);
            var modified = false;

            for (var i = 0; i < verts.Length; i++)
            {
                float dist = Vector3.Distance(verts[i], localPoint);
                if (dist > deformRadius) continue;

                float falloff = 1f - Mathf.Pow(dist / deformRadius, deformFalloff);
                Vector3 displacement = localNormal * (deformAmount * falloff);

                // Clamp total deformation from original position
                Vector3 original = kvp.Value[i];
                Vector3 newPos = verts[i] + displacement;
                if (Vector3.Distance(newPos, original) > maxDeformation)
                {
                    Vector3 dir = (newPos - original).normalized;
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

        foreach (KeyValuePair<MeshFilter, Vector3[]> kvp in _originalVertices)
        {
            MeshFilter mf = kvp.Key;
            if (mf == null) continue;

            Mesh mesh = mf.mesh;
            Vector3[] verts = mesh.vertices;
            Vector3[] originals = kvp.Value;

            for (var i = 0; i < verts.Length; i++)
                verts[i] = Vector3.Lerp(verts[i], originals[i], amount01);

            mesh.vertices = verts;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
        }
    }

    public void FullRepair() => Repair(1f);
}