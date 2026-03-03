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
    public bool dealSelfDamage = true;

    // optional: how much damage a crystal takes from one explosion
    [Header("Crystal Settings")]
    public int crystalDamagePerExplosion = 1;

    public void TriggerImpact(Vector3 hitPosition)
    {
        SpawnFX(hitPosition);
        ApplyForce(hitPosition);
        ApplyDamage(hitPosition);
        PlayerCam.Instance?.Shake(0.3f, 0.5f);
    }

    private void SpawnFX(Vector3 pos)
    {
        if (explosionEffectPrefab == null) return;

        pos.y += 0.1f;
        GameObject fx = Instantiate(explosionEffectPrefab, pos, Quaternion.identity);

        LaserExplosionSphere effect = fx.GetComponent<LaserExplosionSphere>();
        if (effect != null && explosionSound != null)
            effect.PlaySound(explosionSound);

        Destroy(fx, 1.5f);
    }

    private void ApplyForce(Vector3 pos)
    {
        int forceMask = ~LayerMask.GetMask("Player"); // exclude player from physics force
        Collider[] forceColliders = Physics.OverlapSphere(pos, explosionRadius, forceMask, QueryTriggerInteraction.Ignore);

        foreach (Collider nearby in forceColliders)
        {
            Rigidbody rb = nearby.attachedRigidbody;
            if (rb != null)
                rb.AddExplosionForce(explosionForce, pos, explosionRadius, 1f, ForceMode.Impulse);
        }
    }

    private void ApplyDamage(Vector3 pos)
    {
        // get everything in radius (you might choose to filter by layer mask to optimize)
        Collider[] overlaps = Physics.OverlapSphere(pos, explosionRadius, ~0, QueryTriggerInteraction.Ignore);

        HashSet<EnemyHealth> damagedEnemies = new HashSet<EnemyHealth>();
        HashSet<ShieldCrystal> damagedCrystals = new HashSet<ShieldCrystal>();
        bool anyEnemyHit = false;
        bool playerDamaged = false;

        foreach (Collider col in overlaps)
        {
            if (col == null) continue;

            // --- PLAYER DAMAGE (once) ---
            if (!playerDamaged && dealSelfDamage && col.CompareTag("Player"))
            {
                // Use ClosestPoint so tall colliders don't under-report distance
                Vector3 closest = col.ClosestPoint(pos);
                float dist = Vector3.Distance(pos, closest);
                float falloff = Mathf.Clamp01(1f - dist / explosionRadius);
                float pDamage = damage * falloff * 0.5f; // keep half self-damage as before

                if (pDamage > 0f)
                    PlayerLife.Instance?.TakeDamage(Mathf.RoundToInt(pDamage));

                playerDamaged = true;
                // continue searching for enemies/crystals - don't `continue` here
            }

            // --- SHIELD CRYSTAL HANDLING ---
            ShieldCrystal crystal = col.GetComponentInParent<ShieldCrystal>();
            if (crystal != null && !damagedCrystals.Contains(crystal))
            {
                Vector3 closest = col.ClosestPoint(pos);
                float dist = Vector3.Distance(pos, closest);
                float falloff = Mathf.Clamp01(1f - dist / explosionRadius);

                // If you want crystals to also get falloff-based damage, you can compute here,
                // but your original logic used a fixed amount (1). We'll apply fixed amount but only if inside radius.
                if (falloff > 0f)
                {
                    crystal.TakeDamage(crystalDamagePerExplosion);
                    damagedCrystals.Add(crystal);
                    Hitmarker.Instance?.ShowHit(crystal.transform.position, crystalDamagePerExplosion, true);
                }

                // proceed to next collider (avoid double-processing same collider as enemy)
                continue;
            }

            // --- ENEMY HANDLING (use EnemyHealth on parent) ---
            EnemyHealth enemy = col.GetComponentInParent<EnemyHealth>();
            if (enemy != null && !damagedEnemies.Contains(enemy))
            {
                // If enemy is already dead, skip
                if (enemy.currentHealth <= 0f)
                {
                    damagedEnemies.Add(enemy);
                    continue;
                }

                Vector3 closest = col.ClosestPoint(pos);
                float dist = Vector3.Distance(pos, closest);
                float falloff = Mathf.Clamp01(1f - dist / explosionRadius);
                float finalDamage = damage * falloff;

                if (finalDamage > 0f)
                {
                    damagedEnemies.Add(enemy);
                    enemy.TakeDamage(finalDamage);
                    anyEnemyHit = true;
                    Hitmarker.Instance?.ShowHit(enemy.transform.position, finalDamage, false);
                }
                continue;
            }
            // --- BOSS HANDLING ---
            BossManager boss = col.GetComponentInParent<BossManager>();
            if (boss != null)
            {
                Vector3 closest = col.ClosestPoint(pos);
                float dist = Vector3.Distance(pos, closest);
                float falloff = Mathf.Clamp01(1f - dist / explosionRadius);
                float finalDamage = damage * falloff;

                if (finalDamage > 0f)
                {
                    boss.ReciveDamage(Mathf.RoundToInt(finalDamage));
                    Hitmarker.Instance?.ShowHit(boss.transform.position, finalDamage, false);
                }
                continue;
            }
            // If you have other damageable objects (e.g. destructible crates) that implement an interface,
            // you can detect and apply damage here. Example (optional):
            // IDamageable dmg = col.GetComponentInParent<IDamageable>();
            // if (dmg != null) { dmg.TakeDamage(finalDamage); }
        }

        // global central hitmarker when an enemy was hit
        if (anyEnemyHit)
            Hitmarker.Instance?.ShowHit(pos, 0f, false);
    }

}
