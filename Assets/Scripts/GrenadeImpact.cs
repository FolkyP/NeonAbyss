using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeImpact : MonoBehaviour
{
    [Header("Grenade Settings")]
    public float explosionRadius = 5f;
    public float explosionForce = 700f;
    public float damage = 50f;
    public float lifetime = 5f; // auto-destroy if no impact

    [Header("Effects")]
    public GameObject explosionEffectPrefab;

    private bool hasExploded = false;

    private void Start()
    {
        // Auto-destroy in case it never hits anything
        Destroy(gameObject, lifetime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hasExploded) return;
        hasExploded = true;

        Explode();
    }

    private void Explode()
    {
        // Spawn explosion effect
        if (explosionEffectPrefab != null)
        {
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        }

        // Physics explosion
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider nearby in colliders)
        {
            Rigidbody rb = nearby.attachedRigidbody;
            if (rb != null)
            {
                rb.AddExplosionForce(explosionForce, transform.position, explosionRadius);
            }

            // Example: if target has health script, apply damage
            // var health = nearby.GetComponent<Health>();
            // if (health != null) health.TakeDamage(damage);
        }

        Destroy(gameObject);
    }
}
