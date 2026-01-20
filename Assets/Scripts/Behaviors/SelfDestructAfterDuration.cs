using UnityEngine;

public class SelfDestructAfterDuration : MonoBehaviour
{
    public float Duration;

    public void Start()
    {
        Object.Destroy(this.gameObject, this.Duration);
    }
}
