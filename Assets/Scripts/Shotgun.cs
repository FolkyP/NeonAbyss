using System.Collections;
using UnityEngine;

public class Shotgun : WeaponBase
{
    [Header("Shotgun Settings")]
    public int pelletsPerShot = 8;        // number of lasers fired per click
    public float spreadAngle = 10f;       // degrees of spread cone
    public float fireRate = 1f;           // shots per second
    public float maxRange = 50f;          // shorter range than rifle/pistol
    public float laserDuration = 0.05f;
    public float damagePerPellet = 8f;    // damage for each pellet
    public GameObject laserPrefab;
    public GameObject impactEffectPrefab;
    public GameObject muzzleFlash;

    private bool isFiring = false;
    private float lastFireTime;
    private WeaponManager wm;

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
                FireShotgunBlast();
                PlayShootSound();
                ApplyRecoil();
                StartCoroutine(RecoilResetRoutine());
                currentAmmoInMag--;
                wm?.UpdateWeaponUI();
                lastFireTime = Time.time;
            }
            yield return null;
        }
    }

    private void FireShotgunBlast()
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

        // Fire multiple pellets
        for (int i = 0; i < pelletsPerShot; i++)
        {
            ShootPellet();
        }
    }

    private void ShootPellet()
    {
        // Ray from screen center
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        // Random spread
        ray.direction = Quaternion.Euler(
            Random.Range(-spreadAngle, spreadAngle),
            Random.Range(-spreadAngle, spreadAngle),
            0
        ) * ray.direction;

        RaycastHit hit;
        Vector3 targetPoint;

        if (Physics.Raycast(ray, out hit, maxRange, targetMask))
        {
            targetPoint = hit.point;

            // Apply damage if object has Health
            hit.collider.gameObject.SendMessage("TakeDamage", damagePerPellet, SendMessageOptions.DontRequireReceiver);

            if (hit.rigidbody != null)
                hit.rigidbody.AddForceAtPosition(ray.direction * 30f, hit.point, ForceMode.Impulse);

            if (impactEffectPrefab != null)
            {
                GameObject fx = Instantiate(impactEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(fx, 1.5f);
            }
        }
        else
        {
            targetPoint = ray.GetPoint(maxRange);
        }

        // Laser beam
        if (laserPrefab != null)
        {
            GameObject go = Instantiate(laserPrefab);
            LineRenderer lr = go.GetComponent<LineRenderer>();
            if (lr != null)
            {
                lr.positionCount = 2;
                lr.SetPosition(0, muzzleTransform.position);
                lr.SetPosition(1, targetPoint);
            }
            Destroy(go, laserDuration);
        }
        else
        {
            StartCoroutine(TempLaserLine(muzzleTransform.position, targetPoint, laserDuration));
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
        yield return new WaitForSeconds(duration);
        Destroy(go);
    }
}
