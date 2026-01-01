using UnityEngine;

public class ShockwaveDamage : MonoBehaviour
{
    public float damage = 15f;
    public float radius = 6f;
    public float lifetime = 1f;
    public float knockupForce = 6f;

    void Start()
    {
        // optional expand animation logic here
        Destroy(gameObject, lifetime);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, radius);
    }

    private void OnTriggerEnter(Collider other)
    {
        // This method only fires if prefab has a collider configured to detect players as it expands.
        if (other.CompareTag("Player"))
        {
            other.SendMessage("TakeDamage", (int)damage, SendMessageOptions.DontRequireReceiver);
            Rigidbody rb = other.attachedRigidbody;
            if (rb != null)
            {
                rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
                rb.AddForce(Vector3.up * knockupForce, ForceMode.VelocityChange);
            }
            else
            {
                other.SendMessage("ApplyKnockup", knockupForce, SendMessageOptions.DontRequireReceiver);
            }
        }
    }
}
