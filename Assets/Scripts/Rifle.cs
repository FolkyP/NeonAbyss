using System.Collections;
using UnityEngine;

public class Rifle : WeaponBase
{
    [Header("Rifle Settings")]
    public float fireRate = 10f;            // shots per second
    public float maxRange = 150f;           // longer than pistol
    public float laserDuration = 0.03f;     // shorter flash
    public float damage = 15f;              // lower damage than pistol, but faster fire
    public GameObject laserPrefab;          // prefab with LineRenderer
    public GameObject impactEffectPrefab;   // optional spark/explosion effect
    public GameObject muzzleFlash;          // optional flash effect at barrel

    private bool isFiring = false;
    private float lastFireTime = 0f;
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
                FireLaser();
                currentAmmoInMag--;
                wm?.UpdateWeaponUI();
                lastFireTime = Time.time;
            }

            yield return null;
        }
    }

    private void FireLaser()
    {
        if (muzzleTransform == null) return;

        // Play muzzle flash
        if (muzzleFlash != null)
        {
            var ps = muzzleFlash.GetComponent<ParticleSystem>();
            if (ps != null) ps.Play();
            else StartCoroutine(EnableFlashBriefly());
        }

        // Ray from center of screen
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;
        Vector3 targetPoint;

        if (Physics.Raycast(ray, out hit, maxRange, targetMask))
        {
            targetPoint = hit.point;

            // Damage
            hit.collider.gameObject.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);

            if (hit.rigidbody != null)
                hit.rigidbody.AddForceAtPosition(ray.direction * 40f, hit.point, ForceMode.Impulse);

            // Impact FX
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
