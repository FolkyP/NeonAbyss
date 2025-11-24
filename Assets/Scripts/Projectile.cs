using UnityEngine;

public class Projectile : MonoBehaviour
{
    public int damage;
    private bool hasDealtDamage = false; // Track if damage has been applied

    private void Start()
    {
        Destroy(gameObject, 3f); // Destroy the projectile after 3 seconds to avoid clutter
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasDealtDamage) return; // Exit if damage already dealt

        if (other.CompareTag("Player"))
        {
            Debug.Log("Projectile hit the player!");
            PlayerLife.Instance.TakeDamage(damage);
            hasDealtDamage = true; // Mark as done
            Destroy(gameObject);
        }
        else if (other.CompareTag("Untagged"))
        {
            Destroy(gameObject);
        }
    }
}
