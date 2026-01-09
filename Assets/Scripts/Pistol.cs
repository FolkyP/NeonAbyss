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

        RaycastHit[] hits = Physics.RaycastAll(ray, maxRange, targetMask);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
       
        foreach (var hit in hits)
        {
            // CÍL JE ENEMY
            if (hit.collider.CompareTag("Head"))
            {
                hit.collider.transform.root.SendMessage("TakeDamage", damage * 1.5f, SendMessageOptions.DontRequireReceiver);
                Hitmarker.Instance?.ShowHit(hit.point, damage * 1.5f, true);
                return;
            }
            if (hit.collider.CompareTag("Enemy"))
            {
                hit.collider.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
                Hitmarker.Instance?.ShowHit(hit.point, damage, false);
                continue;
            }
            if (hit.collider.CompareTag("Boss"))
            {
                BossManager.Instance.ReciveDamage((int)damage);
                Hitmarker.Instance?.ShowHit(hit.point, damage, false);
                continue;
            }
            // CÍL JE HEADSHOT
            
            if (hit.collider.CompareTag("ShieldCrystal"))
            {

                ShieldCrystal crystal = hit.transform.gameObject.GetComponent<ShieldCrystal>();
                if (crystal != null)
                {
                    crystal.TakeDamage(1); // dáš dmg, jaký chceš
                    Hitmarker.Instance?.ShowHit(hit.point, 1, true);

                }
            }
            if (impactEffectPrefab != null)
                Instantiate(impactEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
            // cokoliv jiného ignorujeme úplnì
        }
    }
  
    private IEnumerator EnableFlashBriefly()
    {
        muzzleFlash.SetActive(true);
        yield return new WaitForSeconds(0.05f);
        muzzleFlash.SetActive(false);
    }
}
