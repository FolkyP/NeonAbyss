using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    private float currentHealth;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    // This matches what your weapon is sending
    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        Debug.Log($"{gameObject.name} took {amount} damage, health = {currentHealth}");

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} died!");
        // Destroy the object (you can replace this with ragdoll, explosion, etc.)
        Destroy(gameObject);
    }
}
