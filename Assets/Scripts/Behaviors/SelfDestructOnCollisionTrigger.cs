using UnityEngine;

public class SelfDestructOnCollisionTrigger : MonoBehaviour
{
    public void OnTriggerEnter(Collider other)
    {
        Object.Destroy(this.gameObject);
    }
}
