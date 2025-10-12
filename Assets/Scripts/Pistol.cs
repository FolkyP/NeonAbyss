using System.Collections;
using UnityEngine;

public class Pistol : WeaponBase
{
    [Header("Pistol Settings")]
    public float fireRate = 3f;            // shots per second (only used in automatic mode)
    public bool isAutomatic = false;       // false = semi-auto (one shot per StartFire call)
    public float maxRange = 100f;
    public float laserDuration = 0.05f;   // how long the visible laser beam stays
    public float damage = 25f;
    public GameObject laserPrefab;        // prefab with a LineRenderer on root (no particle system required)
    public GameObject impactEffectPrefab; // optional effect to spawn on hit

    // optional muzzle flash GameObject (enable/disable) — can be a short-lived object or particle system on the weapon
    public GameObject muzzleFlash;

    private bool isFiring = false;
    private WeaponManager wm;

    void Start()
    {
        wm = FindObjectOfType<WeaponManager>(); // used only for UpdateWeaponUI calls
    }

    public override void StartFire()
    {
        if (isAutomatic)
        {
            if (isFiring) return;
            isFiring = true;
            StartCoroutine(AutomaticFireRoutine());
        }
        else
        {
            // semi-auto: fire one shot per StartFire call
            TryFireOnce();
            PlayShootSound();
            ApplyRecoil();
            PlayerCam.Instance.Shake(0.1f, 0.1f);
            StartCoroutine(RecoilResetRoutine());
        }
    }

    public override void StopFire()
    {
        isFiring = false;
    }

    private IEnumerator AutomaticFireRoutine()
    {
        float delay = 1f / fireRate;
        while (isFiring)
        {
            TryFireOnce();
            yield return new WaitForSeconds(delay);
        }
    }

    private void TryFireOnce()
    {
        if (!CanFire()) return; // respects isReloading and currentAmmoInMag > 0

        FireLaser();
        currentAmmoInMag--;
        wm?.UpdateWeaponUI();

        // Optional: if you want automatic reload when emptied, uncomment:
        // if (currentAmmoInMag <= 0 && carriedAmmo > 0) StartCoroutine(Reload());
    }

    private void FireLaser()
    {
        if (muzzleTransform == null) return;
        isRecoiling = true;

        // Muzzle flash
        if (muzzleFlash != null)
        {
            var ps = muzzleFlash.GetComponent<ParticleSystem>();
            if (ps != null) ps.Play();
            else StartCoroutine(EnableFlashBriefly());
        }

        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;
        Vector3 targetPoint;

        bool didHitTarget = false;

        if (Physics.Raycast(ray, out RaycastHit hits, maxRange, targetMask))
        {
            hits.collider.gameObject.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);

            if (hits.collider.CompareTag("Enemy"))
            {
                Hitmarker.Instance?.ShowHit(hits.point, damage);
            }
        }

    }


    private IEnumerator EnableFlashBriefly()
    {
        muzzleFlash.SetActive(true);
        yield return new WaitForSeconds(0.05f);
        muzzleFlash.SetActive(false);
    }

    private IEnumerator TempLaserLine(Vector3 start, Vector3 end, float duration)
    {
        GameObject go = new GameObject("TempLaser");
        LineRenderer lr = go.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        // Keep default material / color so you can style it in editor if needed
        yield return new WaitForSeconds(duration);
        Destroy(go);
    }
}
