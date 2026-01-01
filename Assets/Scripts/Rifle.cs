using System.Collections;
using UnityEngine;

public class Rifle : WeaponBase
{
    [Header("Rifle Settings")]
    public float fireRate = 10f;
    public float maxRange = 150f;
    public float damage = 15f;
    public GameObject muzzleFlash;

    private bool isFiring = false;
    private float lastFireTime = 0f;
    private WeaponManager wm;
    public GameObject flash;

    void Start()
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
        float delay = 1f / fireRate;

        while (isFiring)
        {
            if (CanFire() && Time.time >= lastFireTime + delay)
            {
                FireShot();
                PlayerCam.Instance.Shake(0.05f, 0.08f);
                PlayShootSound();
                ApplyRecoil();
                StartCoroutine(RecoilResetRoutine());
                if (!infiniteAmmo) carriedAmmo--;
                wm?.UpdateWeaponUI();
                lastFireTime = Time.time;
            }

            yield return null;
        }
    }

    private void FireShot()
    {
        isRecoiling = true;
        if (flash != null)
        {
            //var ps = muzzleFlash.GetComponent<ParticleSystem>();
            //if (ps != null) ps.Play();
            //else StartCoroutine(EnableFlashBriefly());
            StartCoroutine(MuzzleFlashEffect());
        }

        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        // Získáme VŠECHNY hity
        RaycastHit[] hits = Physics.RaycastAll(ray, maxRange, targetMask);

        // Seøadíme podle vzdálenosti
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
        foreach (var hit in hits)
        {
            // CÍL JE ENEMY
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
            if (hit.collider.CompareTag("Head"))
            {
                hit.collider.transform.root.SendMessage("TakeDamage", damage * 1.5f, SendMessageOptions.DontRequireReceiver);
                Hitmarker.Instance?.ShowHit(hit.point, damage * 1.5f, true);
                continue;
            }
            if (hit.collider.CompareTag("ShieldCrystal")) {

                ShieldCrystal crystal = hit.transform.gameObject.GetComponent<ShieldCrystal>();
                if (crystal != null)
                {
                    crystal.TakeDamage(1); // dáš dmg, jaký chceš
                    Hitmarker.Instance?.ShowHit(hit.point, 1, true);

                }
            }

            // cokoliv jiného ignorujeme úplnì
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
    private IEnumerator EnableFlashBriefly()
    {
        muzzleFlash.SetActive(true);
        yield return new WaitForSeconds(0.05f);
        muzzleFlash.SetActive(false);
    }
}
