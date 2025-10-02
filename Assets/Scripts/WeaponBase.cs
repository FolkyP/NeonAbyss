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

    
    //[Header("Audio")]
    //public AudioClip shootSound;
    //public AudioClip reloadSound;
    //protected AudioSource audioSource;


    protected virtual void Awake()
    {
        currentAmmoInMag = magazineSize;
        //audioSource = GetComponent<AudioSource>();
        //if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
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
        //if (reloadSound != null) audioSource?.PlayOneShot(reloadSound);

        yield return new WaitForSeconds(reloadTime);


        int needed = magazineSize - currentAmmoInMag;
        int taken = Mathf.Min(needed, carriedAmmo);
        currentAmmoInMag += taken;
        carriedAmmo -= taken;
        isReloading = false;
        FindObjectOfType<WeaponManager>()?.UpdateWeaponUI();

    }


    //protected void PlayShootSound()
    //{
    //    if (shootSound != null) audioSource?.PlayOneShot(shootSound);
    //}
}