using System.Collections;
using UnityEngine;
public abstract class WeaponBase : MonoBehaviour
{
    [Header("General")]
    public string weaponName = "Weapon";
    public Transform muzzleTransform; // where the shot/muzzle VFX originates
    public LayerMask targetMask = ~0; // what the weapon can hit


    [Header("Ammo")]
    public int magazineSize = 12;
    public int carriedAmmo = 36;
    public int currentAmmoInMag;
    public float reloadTime = 1.5f;
    public bool isReloading = false;

    [Header("Firing")]
    public float recoilAmount = 0.2f;
    public float recoilsmoothness = 4f;

    [HideInInspector] public bool isRecoiling = false;
    private Vector3 currentRecoil = Vector3.zero;
    public Vector3 originalLocalPos;
    [Header("Audio")]
    public AudioClip shootSound;
    public AudioClip reloadSound;
    protected AudioSource audioSource;


    protected virtual void Awake()
    {
        currentAmmoInMag = magazineSize;
        originalLocalPos = transform.localPosition;
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
    }


    public abstract void StartFire();
    public abstract void StopFire();


    public virtual bool CanFire()
    {
        return !isReloading && currentAmmoInMag > 0;
    }


    public virtual IEnumerator Reload()
    {
        if (isReloading) yield break;
        if (currentAmmoInMag >= magazineSize) yield break;
        if (carriedAmmo <= 0) yield break;


        isReloading = true;
        if (reloadSound != null) audioSource?.PlayOneShot(reloadSound);

        yield return new WaitForSeconds(reloadTime);


        int needed = magazineSize - currentAmmoInMag;
        int taken = Mathf.Min(needed, carriedAmmo);
        currentAmmoInMag += taken;
        carriedAmmo -= taken;
        isReloading = false;
        FindObjectOfType<WeaponManager>()?.UpdateWeaponUI();

    }
    public void ApplyRecoil()
    {
        Vector3 targetRecoil = Vector3.zero;
        if(isRecoiling)
        {
            targetRecoil = new Vector3(0, 0, -recoilAmount);
            if(Vector3.Distance(currentRecoil, targetRecoil) < 0.01f)
            {
                isRecoiling = false;
            }
        }

        currentRecoil = Vector3.Lerp(currentRecoil, targetRecoil, Time.deltaTime * recoilsmoothness);
        transform.localPosition = originalLocalPos + currentRecoil;

    }
    public IEnumerator RecoilResetRoutine()
    {
        float t = 0f;
        float duration = 0.1f;
        Vector3 startRecoil = currentRecoil;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            currentRecoil = Vector3.Lerp(startRecoil, Vector3.zero, t);
            transform.localPosition = originalLocalPos + currentRecoil;
            yield return null;
        }

        isRecoiling = false;
    }

    protected void PlayShootSound()
    {
        if (shootSound != null) audioSource?.PlayOneShot(shootSound);
    }
}