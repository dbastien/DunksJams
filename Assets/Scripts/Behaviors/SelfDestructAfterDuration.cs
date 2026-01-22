using UnityEngine;

public class SelfDestructAfterDuration : MonoBehaviour
{
    public float Duration;

    public void Start() => Destroy(gameObject, Duration); 
}
