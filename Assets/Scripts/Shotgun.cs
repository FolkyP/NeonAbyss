using System.Collections;
using UnityEngine;

public class Shotgun : WeaponBase
{
    [Header("Shotgun Settings")]
    public int pelletsPerShot = 8;        // number of lasers fired per click
    public float spreadAngle = 3f;       // degrees of spread cone
    public float fireRate = 1f;           // shots per second
    public float maxRange = 50f;          // shorter range than rifle/pistol
    public float laserDuration = 0.001f;
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
                PlayerCam.Instance.Shake(0.15f, 0.3f);
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
        if (muzzleTransform == null) return;

        Vector3 origin = muzzleTransform.position;

        // Ray from camera center
        Ray camRay = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Vector3 targetPoint;

        if (Physics.Raycast(camRay, out RaycastHit hit, maxRange, targetMask))
            targetPoint = hit.point;
        else
            targetPoint = camRay.GetPoint(maxRange);

        // Direction from muzzle to target
        Vector3 direction = (targetPoint - origin).normalized;

        // apply spread
        direction = Quaternion.Euler(
            Random.Range(-spreadAngle, spreadAngle),
            Random.Range(-spreadAngle, spreadAngle),
            0
        ) * direction;

        // Raycast from muzzle along direction for actual pellet hit
        bool didHitTarget = false;
        if (Physics.Raycast(origin, direction, out RaycastHit finalHit, maxRange, targetMask))
        {
            finalHit.collider.gameObject.SendMessage("TakeDamage", damagePerPellet, SendMessageOptions.DontRequireReceiver);

            if (finalHit.collider.CompareTag("Enemy"))
            {
                Hitmarker.Instance?.ShowHit(finalHit.point, damagePerPellet);
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
        yield return new WaitForSeconds(duration);
        Destroy(go);
    }
}
