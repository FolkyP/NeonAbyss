using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine;
using System.Collections;

public class WeaponManager : MonoBehaviour
{
    [SerializeField] GameSettings gameSettings;
    public List<WeaponBase> weapons = new List<WeaponBase>();
    int currentIndex = 0;

    [SerializeField] public RawImage[] weaponCursors;
    [SerializeField] private Image[] weaponImages;
    [SerializeField] private TMP_Text[] weaponTexts;
    [SerializeField] private RawImage[] weaponAmmo;
    [SerializeField] private float switchDuration = 0.2f;
    [SerializeField] private float offscreenY = -1f;
    private bool isSwitching = false;

    [Header("Ammo given by pickups per weapon")]
    public int[] ammoPickupAmount = { 40, 20, 10, 5};

    void Start()
    {
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
                w.gameObject.SetActive(true);
                w.transform.localPosition = new Vector3(w.originalLocalPos.x, offscreenY, w.originalLocalPos.z);
                w.gameObject.SetActive(false);
            }
        }

        UpdateWeaponUI();
        UpdateWeaponOpacity();
    }

    void Update()
    {
        if (gameSettings.isGameOn == false) return;
        if (gameSettings.isGameStopped) return;

        // number key switching
        for (int i = 0; i < weapons.Count; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                SwitchTo(i);
        }

        // scroll wheel switching
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f) SwitchTo((currentIndex + 1) % weapons.Count);
        if (scroll < 0f) SwitchTo((currentIndex - 1 + weapons.Count) % weapons.Count);

        // fire
        if (Input.GetButtonDown("Fire1"))
        {
            WeaponBase currentWeapon = weapons[currentIndex];

            if (currentWeapon.CanFire())
                currentWeapon.StartFire();
            else
                Debug.Log("Click! No ammo!");
        }

        if (Input.GetButtonUp("Fire1"))
            weapons[currentIndex].StopFire();
    }

    void SwitchTo(int i)
    {
        if (i < 0 || i >= weapons.Count || isSwitching || i == currentIndex)
            return;

        StartCoroutine(SwitchWeaponCoroutine(i));
    }

    public void UpdateWeaponUI()
    {
        WeaponBase w = weapons[currentIndex];
        if (gameSettings != null && gameSettings.ammoText != null)
        {
            gameSettings.WeaponText.text = w.weaponName;
            gameSettings.ammoText.text = w.infiniteAmmo ? "" : $"{w.carriedAmmo}";
        }
    }

    private IEnumerator SwitchWeaponCoroutine(int newIndex)
    {
        if (isSwitching) yield break;
        isSwitching = true;

        WeaponBase oldWeapon = weapons[currentIndex];
        WeaponBase newWeapon = weapons[newIndex];

        oldWeapon.StopFire();

        // Animation
        oldWeapon.transform.localPosition = oldWeapon.originalLocalPos;
        newWeapon.transform.localPosition = new Vector3(newWeapon.originalLocalPos.x, offscreenY, newWeapon.originalLocalPos.z);
        newWeapon.gameObject.SetActive(true);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / switchDuration;

            newWeapon.transform.localPosition = Vector3.Lerp(
                new Vector3(newWeapon.originalLocalPos.x, offscreenY, newWeapon.originalLocalPos.z),
                newWeapon.originalLocalPos,
                t
            );

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
        UpdateCursor();
        UpdateWeaponOpacity();
        isSwitching = false;
    }

    private void UpdateCursor()
    {
        for (int i = 0; i < weaponCursors.Length; i++)
        {
            weaponCursors[i].gameObject.SetActive(i == currentIndex);
        }

        for (int i = 0; i < weaponAmmo.Length; i++)
        {
            weaponAmmo[i].gameObject.SetActive(i == currentIndex);
        }
    }
    public void AddAmmoToCurrentWeapon()
    {
        WeaponBase current = weapons[currentIndex];

        if (current.infiniteAmmo)
            return;

        // Read amount from the ammo table
        int amount = ammoPickupAmount[currentIndex];

        current.carriedAmmo += amount;

        Debug.Log($"Added {amount} ammo to {current.weaponName}");

        UpdateWeaponUI();
    }
    
public void LoadAllGuns()
    {
        Debug.Log("Spouštím LoadAllGuns pro reset munice na hodnoty z Inspektoru.");

        foreach (var weapon in weapons)
        {
            if (weapon.infiniteAmmo) continue;

            if (weapon.carriedAmmo < weapon.startingAmmoFromInspector)
            {
                weapon.carriedAmmo = weapon.startingAmmoFromInspector;
            }

        }

        UpdateWeaponUI();
    }

    public void ResetGun()
    {
        foreach (var weapon in weapons)
        {
            if (weapon.infiniteAmmo) continue;

            
                weapon.carriedAmmo = weapon.startingAmmoFromInspector;
            

        }
        UpdateWeaponUI();
    }
    private void UpdateWeaponOpacity()
    {
        for (int i = 0; i < weaponImages.Length; i++)
        {
            if (weaponImages[i] == null) continue;

            Color imgColor = weaponImages[i].color;
            imgColor.a = (i == currentIndex) ? 1f : 100f / 255f;
            weaponImages[i].color = imgColor;

            if (weaponTexts[i] != null)
            {
                Color txtColor = weaponTexts[i].color;
                txtColor.a = (i == currentIndex) ? 1f : 100f / 255f;
                weaponTexts[i].color = txtColor;
            }

            if (weaponAmmo != null && i < weaponAmmo.Length && weaponAmmo[i] != null)
            {
                Color ammoColor = weaponAmmo[i].color;
                ammoColor.a = (i == currentIndex) ? 1f : 100f / 255f;
                weaponAmmo[i].color = ammoColor;
            }
        }
    }
}
