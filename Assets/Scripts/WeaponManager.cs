using System.Collections.Generic;
using UnityEngine;
public class WeaponManager : MonoBehaviour
{
    [SerializeField] GameSettings gameSettings;
    public List<WeaponBase> weapons = new List<WeaponBase>();
    int currentIndex = 0;


    void Start()
    {
        for (int i = 0; i < weapons.Count; i++)
        {
            weapons[i].gameObject.SetActive(i == currentIndex);
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
            UpdateWeaponUI();
        }

    }


    void SwitchTo(int i)
    {
        if (i < 0 || i >= weapons.Count) return;
        weapons[currentIndex].StopFire();
        weapons[currentIndex].gameObject.SetActive(false);
        currentIndex = i;
        weapons[currentIndex].gameObject.SetActive(true);

        UpdateWeaponUI();
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
}