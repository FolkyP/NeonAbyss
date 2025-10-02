using System.Collections;
using UnityEngine;

public class GrenadeLauncher : WeaponBase
{
    [Header("Grenade Launcher Settings")]
    public GameObject grenadePrefab;
    public float launchForce = 25f;
    public float fireRate = 1f; // shots per second
    public GameObject explosion;

    private bool isFiring = false;
    private float lastFireTime;
    WeaponManager wm;
    private void Start()
    {
         wm = FindObjectOfType<WeaponManager>();
    }
   

    public override void StartFire()
    {
        if (!isFiring)
        {
            isFiring = true;
            StartCoroutine(FireRoutine());
        }
    }

    public override void StopFire()
    {
        isFiring = false;
    }

    private IEnumerator FireRoutine()
    {
        while (isFiring)
        {
            if (CanFire() && Time.time >= lastFireTime + (1f / fireRate))
            {
                FireGrenade();
                lastFireTime = Time.time;
            }
            yield return null;
        }
    }

    private void FireGrenade()
    {
        if (grenadePrefab == null || muzzleTransform == null) return;

        // Spawn grenade
        GameObject grenade = Instantiate(grenadePrefab, muzzleTransform.position, muzzleTransform.rotation);
        grenade.AddComponent<GrenadeImpact>(); // Add impact script if not already on prefab
        grenade.GetComponent<GrenadeImpact>().explosionEffectPrefab = explosion;
        // Get grenade rigidbody
        Rigidbody rb = grenade.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Raycast from center of screen
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            RaycastHit hit;

            Vector3 targetPoint;

            if (Physics.Raycast(ray, out hit, 1000f))
            {
                targetPoint = hit.point; // aim at what the crosshair is pointing to
            }
            else
            {
                targetPoint = ray.GetPoint(1000f); // aim far away if nothing hit
            }
            
            // Calculate direction from muzzle to target
            Vector3 direction = (targetPoint - muzzleTransform.position).normalized;

            // Launch grenade
            rb.AddForce(direction * launchForce * 3, ForceMode.VelocityChange);

            
        }

        currentAmmoInMag--;
        
        if (wm != null)
        {
            wm.UpdateWeaponUI();
        }

        // Uncomment if you want sound support from WeaponBase
        //PlayShootSound();
    }
}
