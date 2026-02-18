using UnityEngine;

public class Orbit : MonoBehaviour
{
    private Vector3 center;

    public Vector3 Distance;

    public Vector3 Speed;

    // Use this for initialization
    private void Start() => center = transform.position;

    // Update is called once per frame
    private void Update()
    {
        Vector3 offset = Vector3.zero;

        offset.x = Distance.x * Mathf.Sin(Speed.x * Time.time);
        offset.y = Distance.y * Mathf.Cos(Speed.y * Time.time);
        offset.z = Distance.z * Mathf.Sin(Speed.z * Time.time);

        transform.position = center + offset;
    }
}