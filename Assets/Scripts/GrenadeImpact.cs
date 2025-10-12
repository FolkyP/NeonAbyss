using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeImpact : MonoBehaviour
{
    [Header("Explosion Settings")]
    public float explosionRadius = 5f;
    public float explosionForce = 700f;
    public float damage = 50f;
    public GameObject explosionEffectPrefab;
    public AudioClip explosionSound;

    public void TriggerImpact(Vector3 hitPosition)
    {
        // --- 1. Visual & Sound FX ---
        if (explosionEffectPrefab != null)
        {
            GameObject fx = Instantiate(explosionEffectPrefab, hitPosition, Quaternion.identity);
            LaserExplosionSphere effect = fx.GetComponent<LaserExplosionSphere>();
            if (effect != null && explosionSound != null)
                effect.PlaySound(explosionSound);
            Destroy(fx, 1.5f);
        }

        // --- 2. Physics & Damage ---
        Collider[] colliders = Physics.OverlapSphere(hitPosition, explosionRadius);
        bool anyEnemyHit = false;

        foreach (Collider nearby in colliders)
        {
            if (nearby.CompareTag("Player"))
                continue;

            Rigidbody rb = nearby.attachedRigidbody;
            if (rb != null)
                rb.AddExplosionForce(explosionForce, hitPosition, explosionRadius, 1f, ForceMode.Impulse);

            // --- Damage falloff based on distance ---
            float distance = Vector3.Distance(hitPosition, nearby.transform.position);
            float falloff = Mathf.Clamp01(1f - distance / explosionRadius);
            float finalDamage = damage * falloff;

            if (finalDamage <= 0f)
                continue;

            // --- Apply damage ---
            nearby.gameObject.SendMessage("TakeDamage", finalDamage, SendMessageOptions.DontRequireReceiver);

            // --- Only show hitmarkers for enemies ---
            if (nearby.CompareTag("Enemy"))
            {
                anyEnemyHit = true;

                // Show floating damage number over each enemy hit
                Hitmarker.Instance?.ShowHit(nearby.transform.position, finalDamage);
            }
        }

        // --- 3. Central hitmarker flash (only once per explosion) ---
        if (anyEnemyHit)
        {
            Hitmarker.Instance?.ShowHit(hitPosition, 0f);
        }

        // --- 4. Camera Shake for feedback ---
        PlayerCam.Instance.Shake(0.3f, 0.5f);
    }

}
