using UnityEngine;

[CreateAssetMenu(menuName = "‽/Physics/Ground Surface", fileName = "GroundSurface")]
public class GroundSurface : ScriptableObject
{
    public enum SurfaceType { Hard, Soft, Liquid }

    [SerializeField] SurfaceType type = SurfaceType.Hard;
    [SerializeField] [Range(0f, 2f)] float gripMultiplier = 1f;
    [SerializeField] [Range(0f, 1f)] float dragMultiplier;
    [SerializeField] [Range(0f, 1f)] float rollingResistanceMultiplier = 1f;
    [SerializeField] AudioClipReference impactSound;
    [SerializeField] AudioClipReference rollSound;
    [SerializeField] Material skidMarkMaterial;
    [SerializeField] GameObject particlePrefab;

    public SurfaceType Type => type;
    public float GripMultiplier => gripMultiplier;
    public float DragMultiplier => dragMultiplier;
    public float RollingResistanceMultiplier => rollingResistanceMultiplier;
    public AudioClipReference ImpactSound => impactSound;
    public AudioClipReference RollSound => rollSound;
    public Material SkidMarkMaterial => skidMarkMaterial;
    public GameObject ParticlePrefab => particlePrefab;
}
