using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class WeaponManager : MonoBehaviour
{
    [SerializeField] GameSettings gameSettings;
    public List<WeaponBase> weapons = new List<WeaponBase>();
    int currentIndex = 0;

    [SerializeField] private float switchDuration = 0.2f;
    [SerializeField] private float offscreenY = -1f; // start position below view
    private bool isSwitching = false;

    void Start()
    {
        // Initialize all weapons
        for (int i = 0; i < weapons.Count; i++)
        {
            WeaponBase w = weapons[i];
            w.originalLocalPos = w.transform.localPosition;

            if (i == currentIndex)
            {
                w.gameObject.SetActive(true);
                w.transform.localPosition = w.originalLocalPos;
            }
            else
            {
                w.gameObject.SetActive(true); // temporarily activate to set position
                w.transform.localPosition = new Vector3(w.originalLocalPos.x, offscreenY, w.originalLocalPos.z);
                w.gameObject.SetActive(false);
            }
        }

        UpdateWeaponUI();
    }




    void Update()
    {
        if (gameSettings.isGameStopped) return;

        for (int i = 0; i < weapons.Count; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i)) SwitchTo(i);
        }


        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f) SwitchTo((currentIndex + 1) % weapons.Count);
        if (scroll < 0f) SwitchTo((currentIndex - 1 + weapons.Count) % weapons.Count);


        if (Input.GetButtonDown("Fire1")) weapons[currentIndex].StartFire();
        if (Input.GetButtonUp("Fire1")) weapons[currentIndex].StopFire();
        //if (Input.GetButtonDown("Fire2")) { /* aim or block */ }
        if (Input.GetKeyDown(KeyCode.R) && !weapons[currentIndex].isReloading)
        {
            StartCoroutine(weapons[currentIndex].Reload());
            StartCoroutine(ReloadAnimation(weapons[currentIndex]));
            UpdateWeaponUI();
        }

    }


    //void SwitchTo(int i)
    //{
    //    if (i < 0 || i >= weapons.Count) return;
    //    weapons[currentIndex].StopFire();
    //    weapons[currentIndex].gameObject.SetActive(false);
    //    currentIndex = i;
    //    weapons[currentIndex].gameObject.SetActive(true);

    //    UpdateWeaponUI();
    //}
    void SwitchTo(int i)
    {
        if (i < 0 || i >= weapons.Count || isSwitching || i == currentIndex)
            return; // do nothing if switching to the same weapon or already switching

        StartCoroutine(SwitchWeaponCoroutine(i));
    }


    public void UpdateWeaponUI()
    {
        if (gameSettings != null && gameSettings.WeaponText != null)
        {
            gameSettings.WeaponText.text = weapons[currentIndex].weaponName;
            gameSettings.ammoText.text = $"{weapons[currentIndex].currentAmmoInMag} / {weapons[currentIndex].magazineSize}";
            gameSettings.allAmmo.text = $"{weapons[currentIndex].carriedAmmo}";

        }
    }

    private IEnumerator SwitchWeaponCoroutine(int newIndex)
    {
        if (isSwitching) yield break;
        isSwitching = true;

        WeaponBase oldWeapon = weapons[currentIndex];
        WeaponBase newWeapon = weapons[newIndex];

        // Stop firing old weapon
        oldWeapon.StopFire();

        // Reset old weapon to original position
        oldWeapon.transform.localPosition = oldWeapon.originalLocalPos;

        // Move new weapon offscreen first
        newWeapon.transform.localPosition = new Vector3(newWeapon.originalLocalPos.x, offscreenY, newWeapon.originalLocalPos.z);
        newWeapon.gameObject.SetActive(true);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / switchDuration;

            // Animate new weapon up
            newWeapon.transform.localPosition = Vector3.Lerp(
                new Vector3(newWeapon.originalLocalPos.x, offscreenY, newWeapon.originalLocalPos.z),
                newWeapon.originalLocalPos,
                t
            );

            // Animate old weapon down
            oldWeapon.transform.localPosition = Vector3.Lerp(
                oldWeapon.originalLocalPos,
                new Vector3(oldWeapon.originalLocalPos.x, offscreenY, oldWeapon.originalLocalPos.z),
                t
            );

            yield return null;
        }

        oldWeapon.transform.localPosition = oldWeapon.originalLocalPos;
        oldWeapon.gameObject.SetActive(false);

        currentIndex = newIndex;
        UpdateWeaponUI();
        isSwitching = false;
    }
    public IEnumerator ReloadAnimation(WeaponBase weapon)
    {
        float duration = weapon.reloadTime;
        Vector3 startPos = weapon.originalLocalPos;
        Vector3 downPos = startPos + new Vector3(0f, -0.3f, 0f);

        Quaternion startRot = weapon.transform.localRotation;
        Quaternion forwardLeanRot = startRot * Quaternion.Euler(15f, 0f, 0f); // change axis if needed

        float halfDuration = duration / 2f;

        // Move down with lean
        for (float t = 0f; t < halfDuration; t += Time.deltaTime)
        {
            float normalized = t / halfDuration;
            float smooth = Mathf.SmoothStep(0f, 1f, normalized);

            weapon.transform.localPosition = Vector3.Lerp(startPos, downPos, smooth);
            weapon.transform.localRotation = Quaternion.Lerp(startRot, forwardLeanRot, smooth);
            yield return null;
        }

        weapon.transform.localPosition = downPos;
        weapon.transform.localRotation = forwardLeanRot;

        // Move back up
        for (float t = 0f; t < halfDuration; t += Time.deltaTime)
        {
            float normalized = t / halfDuration;
            float smooth = Mathf.SmoothStep(0f, 1f, normalized);

            weapon.transform.localPosition = Vector3.Lerp(downPos, startPos, smooth);
            weapon.transform.localRotation = Quaternion.Lerp(forwardLeanRot, startRot, smooth);
            yield return null;
        }

        weapon.transform.localPosition = startPos;
        weapon.transform.localRotation = startRot;
    }







}