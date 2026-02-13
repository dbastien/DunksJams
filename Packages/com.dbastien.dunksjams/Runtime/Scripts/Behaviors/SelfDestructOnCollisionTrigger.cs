using UnityEngine;

public class SelfDestructOnCollisionTrigger : MonoBehaviour
{
    public void OnTriggerEnter(Collider other) => Destroy(gameObject);
}