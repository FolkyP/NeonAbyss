using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class GrenadeImpact : MonoBehaviour
{
    [Header("Explosion Settings")]
    public float explosionRadius = 5f;
    public float explosionForce = 700f;
    public float damage = 50f;
    public GameObject explosionEffectPrefab;
    public AudioClip explosionSound;
    public bool dealSelfDamage = true;

    public void TriggerImpact(Vector3 hitPosition)
    {
        // --- 1. Visual & Sound FX ---
        if (explosionEffectPrefab != null)
        {
            hitPosition.y += 0.1f;

            GameObject fx = Instantiate(explosionEffectPrefab, hitPosition, Quaternion.identity);
            LaserExplosionSphere effect = fx.GetComponent<LaserExplosionSphere>();
            if (effect != null && explosionSound != null)
                effect.PlaySound(explosionSound);
            Destroy(fx, 1.5f);
        }

        // --- 2A. Physics Force (exclude player layer) ---
        int forceMask = ~LayerMask.GetMask("Player");
        Collider[] forceColliders = Physics.OverlapSphere(hitPosition, explosionRadius, forceMask, QueryTriggerInteraction.Ignore);

        foreach (Collider nearby in forceColliders)
        {
            Rigidbody rb = nearby.attachedRigidbody;
            if (rb != null)
                rb.AddExplosionForce(explosionForce, hitPosition, explosionRadius, 1f, ForceMode.Impulse);
        }

        // --- 2B. Damage (include player) ---
        int damageMask = ~0; // include everything
        Collider[] damageColliders = Physics.OverlapSphere(hitPosition, explosionRadius, damageMask, QueryTriggerInteraction.Ignore);

        HashSet<GameObject> damagedEnemies = new HashSet<GameObject>(); // Track already damaged enemies
        bool anyEnemyHit = false;

        foreach (Collider nearby in damageColliders)
        {
            GameObject rootObj = nearby.transform.root.gameObject; // Enemy root
            bool isPlayer = nearby.CompareTag("Player") || rootObj.CompareTag("Player");

            // Skip if this enemy already took damage from this explosion
            if (!isPlayer && damagedEnemies.Contains(rootObj))
                continue;

            // --- Damage falloff ---
            float distance = Vector3.Distance(hitPosition, nearby.transform.position);
            float falloff = Mathf.Clamp01(1f - distance / explosionRadius);
            float finalDamage = damage * falloff;

            if (finalDamage <= 0f)
                continue;

            if (isPlayer)
            {
                if (dealSelfDamage)
                    PlayerLife.Instance?.TakeDamage(Mathf.RoundToInt(finalDamage * 0.5f)); // half self-damage
            }
            else
            {
                damagedEnemies.Add(rootObj); // Mark as damaged

                nearby.gameObject.SendMessage("TakeDamage", finalDamage, SendMessageOptions.DontRequireReceiver);

                if (nearby.CompareTag("Enemy"))
                {
                    anyEnemyHit = true;
                    Hitmarker.Instance?.ShowHit(nearby.transform.position, finalDamage, false);
                }
                if (nearby.CompareTag("ShieldCrystal"))
                {

                    ShieldCrystal crystal = nearby.transform.gameObject.GetComponent<ShieldCrystal>();
                    if (crystal != null)
                    {
                        crystal.TakeDamage(1); // dáš dmg, jaký chceš
                        Hitmarker.Instance?.ShowHit(nearby.transform.position, 1, true);

                    }
                }
            }
        }

        // --- 3. Central hitmarker flash ---
        if (anyEnemyHit)
            Hitmarker.Instance?.ShowHit(hitPosition, 0f, false);

        // --- 4. Camera Shake ---
        PlayerCam.Instance.Shake(0.3f, 0.5f);
    }

}
