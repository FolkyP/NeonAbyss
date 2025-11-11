using System.Collections;
using UnityEngine;

public class Pistol : WeaponBase
{
    [Header("Pistol Settings")]
    public float fireRate = 3f;
    public bool isAutomatic = false;
    public float maxRange = 100f;
    public float damage = 25f;

    public GameObject muzzleFlash;
    public GameObject impactEffectPrefab;

    private bool isFiring = false;
    private WeaponManager wm;
    public GameObject flash;

    void Start()
    {
        wm = FindObjectOfType<WeaponManager>();
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
            TryFireOnce();
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
    private IEnumerator MuzzleFlashEffect()
    {
        //if (muzzleFlashPrefab != null)
        //{
        //    GameObject muzzleFX = Instantiate(muzzleFlashPrefab, muzzleTransform.position, muzzleTransform.rotation);
        //    Destroy(muzzleFX, 0.3f);
        //}
        var s = Instantiate(flash, muzzleTransform.position, muzzleTransform.rotation, muzzleTransform);
        Destroy(s, 0.05f);


        yield return null;
    }
    private void TryFireOnce()
    {
        if (!CanFire()) return;

        FireLaser();

        if (!infiniteAmmo)
            carriedAmmo--; //  total ammo only

        wm?.UpdateWeaponUI();

        PlayShootSound();
        ApplyRecoil();
        PlayerCam.Instance.Shake(0.1f, 0.1f);
        StartCoroutine(RecoilResetRoutine());
    }

    private void FireLaser()
    {
        if (muzzleTransform == null) return;
        isRecoiling = true;

        // Muzzle flash
        if (flash != null)
        {
            StartCoroutine(MuzzleFlashEffect());
        }

        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (Physics.Raycast(ray, out RaycastHit hit, maxRange, targetMask))
        {
            hit.collider.gameObject.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);

            if (hit.collider.CompareTag("Head"))
            {
                // pošli zprávu rodièi, kde je EnemyHealth
                hit.collider.transform.root.SendMessage("TakeDamage", damage * 1.5f, SendMessageOptions.DontRequireReceiver);
                Hitmarker.Instance?.ShowHit(hit.point, damage * 1.5f, true);
            }

            if (hit.collider.CompareTag("Enemy"))
                Hitmarker.Instance?.ShowHit(hit.point, Mathf.RoundToInt(damage), false);

            if (impactEffectPrefab != null)
                Instantiate(impactEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
        }
    }

    private IEnumerator EnableFlashBriefly()
    {
        muzzleFlash.SetActive(true);
        yield return new WaitForSeconds(0.05f);
        muzzleFlash.SetActive(false);
    }
}
