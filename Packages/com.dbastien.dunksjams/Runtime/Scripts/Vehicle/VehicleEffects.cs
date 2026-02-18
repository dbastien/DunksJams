using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(VehicleController))]
public class VehicleEffects : MonoBehaviour
{
    [Header("Skid Marks")] [SerializeField]
    private float skidMarkWidth = 0.2f;

    [SerializeField] private float skidMarkSlipThreshold = 0.3f;
    [SerializeField] private int maxSkidMarksPerWheel = 256;

    [Header("Tire Particles")] [SerializeField]
    private float particleSlipThreshold = 0.4f;

    [SerializeField] [Range(0f, 100f)] private float maxEmissionRate = 50f;

    [Header("Brake Lights")] [SerializeField]
    private Renderer[] brakeLightRenderers;

    [SerializeField] private int brakeLightMaterialIndex;
    [SerializeField] private Material brakeLightOnMaterial;
    [SerializeField] private Material brakeLightOffMaterial;

    private VehicleController _vehicle;
    private readonly Dictionary<VehicleWheel, SkidMarkState> _skidStates = new();
    private readonly Dictionary<VehicleWheel, ParticleSystem> _particleSystems = new();

    private struct SkidMarkState
    {
        public Vector3 LastPosition;
        public bool WasSkidding;
        public List<Vector3> Vertices;
        public List<int> Triangles;
        public List<Vector2> UVs;
        public List<Color> Colors;
        public Mesh Mesh;
        public GameObject Object;
    }

    private void Awake() { _vehicle = GetComponent<VehicleController>(); }

    private void Start()
    {
        foreach (VehicleWheel w in _vehicle.Wheels)
            _skidStates[w] = new SkidMarkState
            {
                Vertices = new List<Vector3>(maxSkidMarksPerWheel * 2),
                Triangles = new List<int>(maxSkidMarksPerWheel * 6),
                UVs = new List<Vector2>(maxSkidMarksPerWheel * 2),
                Colors = new List<Color>(maxSkidMarksPerWheel * 2),
                WasSkidding = false
            };
    }

    private void LateUpdate()
    {
        UpdateSkidMarks();
        UpdateTireParticles();
        UpdateBrakeLights();
    }

    private void UpdateSkidMarks()
    {
        foreach (VehicleWheel w in _vehicle.Wheels)
        {
            if (!_skidStates.TryGetValue(w, out SkidMarkState state)) continue;

            bool isSkidding = w.IsGrounded && w.CombinedSlip > skidMarkSlipThreshold;

            if (isSkidding)
            {
                GroundSurface surface = w.CurrentSurface;
                Material mat = surface?.SkidMarkMaterial;
                if (mat == null)
                {
                    state.WasSkidding = false;
                    _skidStates[w] = state;
                    continue;
                }

                float intensity = Mathf.InverseLerp(skidMarkSlipThreshold, 1f, w.CombinedSlip);
                var color = new Color(1f, 1f, 1f, intensity * 0.8f);

                if (!state.WasSkidding)
                    // Start new mark segment
                    if (state.Object == null)
                    {
                        state.Object = new GameObject($"SkidMark_{w.name}");
                        state.Object.transform.SetParent(null);
                        var mf = state.Object.AddComponent<MeshFilter>();
                        var mr = state.Object.AddComponent<MeshRenderer>();
                        mr.material = mat;
                        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                        mr.receiveShadows = false;
                        state.Mesh = new Mesh { name = "SkidMark" };
                        mf.mesh = state.Mesh;
                    }

                AddSkidSegment(ref state, w.ContactPoint, w.ContactNormal,
                    w.transform.right, color);
                state.WasSkidding = true;
            }
            else { state.WasSkidding = false; }

            _skidStates[w] = state;
        }
    }

    private void AddSkidSegment(ref SkidMarkState state, Vector3 pos, Vector3 normal, Vector3 right, Color color)
    {
        if (state.Vertices.Count >= maxSkidMarksPerWheel * 2) return;

        float halfWidth = skidMarkWidth * 0.5f;
        Vector3 offset = right * halfWidth;
        Vector3 liftedPos = pos + normal * 0.01f;

        int baseIdx = state.Vertices.Count;
        state.Vertices.Add(liftedPos - offset);
        state.Vertices.Add(liftedPos + offset);
        state.UVs.Add(new Vector2(0f, baseIdx * 0.5f));
        state.UVs.Add(new Vector2(1f, baseIdx * 0.5f));
        state.Colors.Add(color);
        state.Colors.Add(color);

        if (baseIdx >= 2)
        {
            state.Triangles.Add(baseIdx - 2);
            state.Triangles.Add(baseIdx - 1);
            state.Triangles.Add(baseIdx);
            state.Triangles.Add(baseIdx - 1);
            state.Triangles.Add(baseIdx + 1);
            state.Triangles.Add(baseIdx);
        }

        if (state.Mesh != null)
        {
            state.Mesh.Clear();
            state.Mesh.SetVertices(state.Vertices);
            state.Mesh.SetTriangles(state.Triangles, 0);
            state.Mesh.SetUVs(0, state.UVs);
            state.Mesh.SetColors(state.Colors);
            state.Mesh.RecalculateNormals();
        }

        state.LastPosition = pos;
    }

    private void UpdateTireParticles()
    {
        foreach (VehicleWheel w in _vehicle.Wheels)
        {
            if (!w.IsGrounded || w.CombinedSlip < particleSlipThreshold)
            {
                StopParticle(w);
                continue;
            }

            GroundSurface surface = w.CurrentSurface;
            if (surface?.ParticlePrefab == null)
            {
                StopParticle(w);
                continue;
            }

            if (!_particleSystems.TryGetValue(w, out ParticleSystem ps) || ps == null)
            {
                GameObject go = Instantiate(surface.ParticlePrefab, w.ContactPoint, Quaternion.identity, transform);
                ps = go.GetComponent<ParticleSystem>();
                _particleSystems[w] = ps;
            }

            if (ps != null)
            {
                ps.transform.position = w.ContactPoint;
                float intensity = Mathf.InverseLerp(particleSlipThreshold, 1.5f, w.CombinedSlip);
                ParticleSystem.EmissionModule emission = ps.emission;
                emission.rateOverTime = intensity * maxEmissionRate;
                if (!ps.isPlaying) ps.Play();
            }
        }
    }

    private void StopParticle(VehicleWheel w)
    {
        if (_particleSystems.TryGetValue(w, out ParticleSystem ps) && ps != null && ps.isPlaying)
            ps.Stop();
    }

    private void UpdateBrakeLights()
    {
        bool braking = _vehicle.BrakeInput > 0.1f || _vehicle.HandbrakeInput;
        if (brakeLightRenderers == null || brakeLightOnMaterial == null || brakeLightOffMaterial == null) return;

        Material mat = braking ? brakeLightOnMaterial : brakeLightOffMaterial;
        foreach (Renderer r in brakeLightRenderers)
        {
            if (r == null) continue;
            Material[] mats = r.materials;
            if (brakeLightMaterialIndex >= 0 && brakeLightMaterialIndex < mats.Length)
            {
                mats[brakeLightMaterialIndex] = mat;
                r.materials = mats;
            }
        }
    }

    private void OnDestroy()
    {
        foreach (KeyValuePair<VehicleWheel, SkidMarkState> kvp in _skidStates)
            if (kvp.Value.Object != null)
                Destroy(kvp.Value.Object);

        foreach (KeyValuePair<VehicleWheel, ParticleSystem> kvp in _particleSystems)
            if (kvp.Value != null)
                Destroy(kvp.Value.gameObject);
    }
}