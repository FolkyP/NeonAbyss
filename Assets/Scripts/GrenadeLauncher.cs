using System.Collections;
using UnityEngine;

public class GrenadeLauncher : WeaponBase
{
    [Header("Raycast Settings")]
    public LayerMask hitMask;
    public float fireRate = 1.2f;
    public float maxDistance = 1000f;
    public float damage = 100f;

    [Header("FX Prefabs")]
    public GameObject muzzleFlashPrefab;
    public GameObject hitEffectPrefab;
    public GameObject flash;

    private bool isFiring = false;
    private float lastFireTime;
    private WeaponManager wm;
    private Camera mainCam;

    private void Start()
    {
        wm = FindObjectOfType<WeaponManager>();
        mainCam = Camera.main;

        if (mainCam == null)
            Debug.LogError("GrenadeLauncher: Main Camera not found!");
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
                PlayShootSound();
                ApplyRecoil();
                StartCoroutine(RecoilResetRoutine());

                if (!infiniteAmmo)
                    carriedAmmo--; //  only total ammo

                wm?.UpdateWeaponUI();
                lastFireTime = Time.time;
            }
            else if (!CanFire())
            {
                Debug.Log("Click! Out of grenades!");
            }

            yield return null;
        }
    }

    private void FireGrenade()
    {
        if (muzzleTransform == null || mainCam == null) return;
        isRecoiling = true;

        Ray ray = mainCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        // Get ALL hits
        RaycastHit[] hits = Physics.RaycastAll(ray, maxDistance, hitMask, QueryTriggerInteraction.Ignore);

        // Sort by distance
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        RaycastHit? bestHit = null;

        foreach (var hit in hits)
        {
            // explode if hitting enemy
            if (hit.collider.CompareTag("Enemy") || hit.collider.CompareTag("Head"))
            {
                bestHit = hit;
                break;
            }
            if (hit.collider.GetComponentInParent<BossController>() != null || hit.collider.CompareTag("Enemy"))
            {
               
                break;
            }

            // ignore everything else (walls, props)
        }

        if (bestHit.HasValue)
        {
            HandleImpact(bestHit.Value);
        }
        else
        {
            RaycastHit fallback;
            if (Physics.Raycast(ray, out fallback, maxDistance, hitMask))
                HandleImpact(fallback);
        }

        StartCoroutine(MuzzleFlashEffect());
    }


    private IEnumerator MuzzleFlashEffect()
    {
        if (muzzleFlashPrefab != null)
        {
            GameObject muzzleFX = Instantiate(muzzleFlashPrefab, muzzleTransform.position, muzzleTransform.rotation);
            Destroy(muzzleFX, 0.3f);
        }

        if (flash != null)
        {
            var s = Instantiate(flash, muzzleTransform.position, muzzleTransform.rotation, muzzleTransform);
            Destroy(s, 0.05f);
        }

        yield return null;
    }

    private void HandleImpact(RaycastHit hit)
    {
        if (hitEffectPrefab != null)
        {
            var fx = Instantiate(hitEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
            Destroy(fx, 0.5f);
        }

        BossController boss = hit.collider.GetComponentInParent<BossController>();
        if (boss != null)
        {
            BossManager.Instance.ReciveDamage((int)damage);
            return;
        }


        // Ostatní
        GrenadeImpact impact = new GameObject("GrenadeImpact").AddComponent<GrenadeImpact>();
        impact.damage = damage;
        impact.TriggerImpact(hit.point);
        Destroy(impact.gameObject, 1f);
    }


}
