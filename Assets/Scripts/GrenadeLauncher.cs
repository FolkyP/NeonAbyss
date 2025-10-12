using System.Collections;
using UnityEngine;

public class GrenadeLauncher : WeaponBase
{
    [Header("Laser Settings")]
    public LineRenderer lineRendererPrefab;
    public float laserDuration = 0.05f;
    public float fireRate = 5f;
    public float maxDistance = 1000f;
    public float damage = 25f;

    [Header("FX Prefabs")]
    public GameObject muzzleFlashPrefab;   // new
    public GameObject hitEffectPrefab;     // stays for explosion / impact
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
        if (!isFiring && currentAmmoInMag > 0)
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
                if (currentAmmoInMag <= 0)
                {
                    StopFire();
                    yield break;
                }

                FireLaser();
                PlayShootSound();
                ApplyRecoil();
                StartCoroutine(RecoilResetRoutine());

                lastFireTime = Time.time;
            }
            yield return null;
        }
    }

    private void FireLaser()
    {
        if (muzzleTransform == null || mainCam == null)
            return;

        isRecoiling = true;

        // --- Raycast from camera center ---
        Ray ray = mainCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;
        Vector3 targetPoint;

        if (Physics.Raycast(ray, out hit, maxDistance))
        {
            targetPoint = hit.point;
            HandleLaserImpact(hit);
        }
        else
        {
            targetPoint = ray.GetPoint(maxDistance);
        }

        StartCoroutine(MuzzleFlashEffect());

        // --- Apply ammo + UI ---
        currentAmmoInMag--;
        wm?.UpdateWeaponUI();
    }

    private IEnumerator MuzzleFlashEffect()
    {
        //if (lineRendererPrefab != null)
        //{
        //    LineRenderer flash = Instantiate(lineRendererPrefab, muzzleTransform.position, Quaternion.identity);
        //    Vector3 flashEnd = muzzleTransform.position + muzzleTransform.forward * 0.3f;
        //    flash.SetPosition(0, muzzleTransform.position);
        //    flash.SetPosition(1, flashEnd);
        //    flash.startWidth = 0.12f;
        //    flash.endWidth = 0.12f;

        //    if (flash.material != null)
        //        flash.material.color = Color.green * 10f;

        //    Destroy(flash.gameObject, laserDuration);
        //}

        if (muzzleFlashPrefab != null)
        {
            GameObject muzzleFX = Instantiate(
                muzzleFlashPrefab,              // prefab
                muzzleTransform.position,       // position at barrel tip
                muzzleTransform.rotation        // rotation matches barrel
            );

            Destroy(muzzleFX, 0.3f);
        }


        //Light flashLight = new GameObject("MuzzleLight").AddComponent<Light>();
        //flashLight.type = LightType.Directional;
        //flashLight.color = Color.green;
        //flashLight.intensity = 3f;
        //flashLight.range = .2f;

        //// Position slightly in front of the muzzle to appear visually correct
        //flashLight.transform.position = muzzleTransform.position;

        

        //Destroy(flashLight.gameObject, 0.05f);
        var s = Instantiate(flash, muzzleTransform.position, muzzleTransform.rotation, muzzleTransform);
        Destroy(s, 0.05f);

        yield return null;
    }


    private void HandleLaserImpact(RaycastHit hit)
    {
        if (hitEffectPrefab != null)
        {
            GameObject impactFX = Instantiate(hitEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
            Destroy(impactFX, 0.5f);
        }

        // Trigger explosion logic
        GrenadeImpact impact = new GameObject("LaserImpact").AddComponent<GrenadeImpact>();
        impact.damage = damage;
        impact.TriggerImpact(hit.point);
        PlayerCam.Instance.Shake(0.2f, 0.5f);

        Destroy(impact.gameObject, 1f);
    }


    private IEnumerator DisableLaserAfterDelay(LineRenderer lr, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (lr != null)
            Destroy(lr.gameObject);
    }
}
