using System.Collections;
using UnityEngine;

public class Shotgun : WeaponBase
{
    [Header("Shotgun Settings")]
    public int pelletsPerShot = 8;        // number of pellets fired per click
    public float spreadAngle = 3f;        // degrees of spread cone
    public float fireRate = 1f;           // shots per second
    public float maxRange = 50f;
    public float damagePerPellet = 8f;    // damage per pellet
    public GameObject muzzleFlash;

    private bool isFiring = false;
    private float lastFireTime;
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
                if (flash != null)
                {
                    //var ps = muzzleFlash.GetComponent<ParticleSystem>();
                    //if (ps != null) ps.Play();
                    //else StartCoroutine(EnableFlashBriefly());
                    StartCoroutine(MuzzleFlashEffect());
                }
                FireShotgunBlast();
                PlayerCam.Instance.Shake(0.15f, 0.3f);
                PlayShootSound();
                ApplyRecoil();
                StartCoroutine(RecoilResetRoutine());

                if (!infiniteAmmo)
                    carriedAmmo--; // 

                wm?.UpdateWeaponUI();
                lastFireTime = Time.time;
            }
            else if (!CanFire())
            {
                Debug.Log("Click! Out of ammo!");
            }

            yield return null;
        }
    }
    private IEnumerator MuzzleFlashEffect()
    {
        //if (muzzleFlashPrefab != null)
        //{
        //    GameObject muzzleFX = Instantiate(muzzleFlashPrefab, muzzleTransform.position, muzzleTransform.rotation);
        //    Destroy(muzzleFX, 0.3f);
        //}

            var s = Instantiate(flash, muzzleTransform.position , muzzleTransform.rotation, muzzleTransform);
            Destroy(s, 0.05f);


        yield return null;
    }
    private void FireShotgunBlast()
    {
        if (muzzleTransform == null) return;
        isRecoiling = true;

        Vector3 origin = muzzleTransform.position;
        Ray camRay = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Vector3 baseTarget = camRay.GetPoint(maxRange);

        float totalDamage = 0f;
        Vector3 lastHitPoint = baseTarget;

        for (int i = 0; i < pelletsPerShot; i++)
        {
            // Base shooting direction
            Vector3 direction = (baseTarget - origin).normalized;

            // Add spread
            float spreadX = Random.Range(-spreadAngle, spreadAngle);
            float spreadY = Random.Range(-spreadAngle, spreadAngle);
            direction = Quaternion.Euler(spreadY, spreadX, 0) * direction;

            // Get ALL hits (ignores walls)
            RaycastHit[] hits = Physics.RaycastAll(origin, direction, maxRange, targetMask);

            // Sort by distance
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            foreach (var hit in hits)
            {
                // ENEMY body
                if (hit.collider.CompareTag("Enemy"))
                {
                    float dmg = damagePerPellet;
                    hit.collider.SendMessage("TakeDamage", dmg, SendMessageOptions.DontRequireReceiver);

                    totalDamage += dmg;
                    lastHitPoint = hit.point;
                    break; // pellet stops after enemy
                }
                if (hit.collider.CompareTag("Boss"))
                {
                    float dmg = damagePerPellet;
                    totalDamage += dmg;
                    lastHitPoint = hit.point;

                    BossManager.Instance.ReciveDamage((int)totalDamage);

                    break; // pellet stops after enemy
                }
                // HEADSHOT
                if (hit.collider.CompareTag("Head"))
                {
                    float dmg = damagePerPellet * 1.5f;
                    hit.collider.transform.root.SendMessage("TakeDamage", dmg, SendMessageOptions.DontRequireReceiver);

                    totalDamage += dmg;
                    lastHitPoint = hit.point;
                    break;
                }
                if (hit.collider.CompareTag("ShieldCrystal"))
                {

                    ShieldCrystal crystal = hit.transform.gameObject.GetComponent<ShieldCrystal>();
                    if (crystal != null)
                    {
                        crystal.TakeDamage(1); // dáš dmg, jaký chceš
                        Hitmarker.Instance?.ShowHit(hit.point, 1, true);

                    }
                }

                // Ignore everything else completely
            }
        }

        // Show combined hitmarker if any pellet hit
        if (totalDamage > 0)
            Hitmarker.Instance?.ShowHit(lastHitPoint, Mathf.RoundToInt(totalDamage), false);
    }


    private IEnumerator EnableFlashBriefly()
    {
        muzzleFlash.SetActive(true);
        yield return new WaitForSeconds(0.05f);
        muzzleFlash.SetActive(false);
    }
}
