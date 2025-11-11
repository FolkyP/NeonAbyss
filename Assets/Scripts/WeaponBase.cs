using System.Collections;
using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    [Header("General")]
    public string weaponName = "Weapon";
    public Transform muzzleTransform;
    public LayerMask targetMask = ~0;

    [Header("Ammo (Total Only)")]
    public int carriedAmmo = 50; // total ammo pool
    public bool infiniteAmmo = false;

    [Header("Firing")]
    public float recoilAmount = 0.2f;
    public float recoilsmoothness = 4f;

    [HideInInspector] public bool isRecoiling = false;
    private Vector3 currentRecoil = Vector3.zero;
    public Vector3 originalLocalPos;

    [Header("Audio")]
    public AudioClip shootSound;
    protected AudioSource audioSource;

    protected virtual void Awake()
    {
        originalLocalPos = transform.localPosition;
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    public abstract void StartFire();
    public abstract void StopFire();

    public virtual bool CanFire()
    {
        return infiniteAmmo || carriedAmmo > 0;
    }

    public void ApplyRecoil()
    {
        Vector3 targetRecoil = isRecoiling ? new Vector3(0, 0, -recoilAmount) : Vector3.zero;
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
        if (shootSound == null) return;

        // Use local weapon audio source to avoid overlapping global SFX
        if (audioSource != null)
        {
            audioSource.PlayOneShot(shootSound, AudioSettings.Instance?.GetSfxVolume() ?? 1f);
        }
    }

}
