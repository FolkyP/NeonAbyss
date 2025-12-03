using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CrystalManager : MonoBehaviour
{
    
    public float crystalDropChance = .15f;
    public float healthDropChance = .15f;
    public float shieldDropChance = .15f;
    public float ammoDropChance = .15f;



    public GameObject crystalPrefab;
    public GameObject healthPrefab;
    public GameObject shieldPrefab;
    public GameObject ammoPrefab;
    public int crystalCount = 0;
    public int totalCrystals = 0;
    public TMP_Text count;
    public AudioClip pick;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    public void DropCrystals(Vector3 pozice)
    {

        
            if (totalCrystals == 4) return;
            Vector3 vector3 = new Vector3(pozice.x, 0.15f, pozice.z);
            GameObject crystal = Instantiate(crystalPrefab, vector3, Quaternion.identity);
            
            crystal.transform.rotation = Quaternion.Euler(-90, 0, 0);
            crystal.AddComponent<CrystalItem>();
            SphereCollider col = crystal.AddComponent<SphereCollider>();
            col.isTrigger = true;
            col.radius = 0.1f;
        totalCrystals++;

          
    }
    
    public void PickCrystal()
    {
        if (crystalCount < 4)
        {
            AudioSettings.Instance.PlaySFX(pick);
            crystalCount++;
            
            Debug.Log("Crystals: " + crystalCount);
            count.text = crystalCount.ToString();
        }
    }
    public void Drop(Vector3 pozice)
    {
        float random = Random.value;
        if (random <= .35f)
        {
            DropCrystals(pozice);
        }
        if(random > .15f && random <= .25f)
        {
            DropHealth(pozice);
        }
        if (random > .25f && random <= .35f)
        {
            DropAmmo(pozice);
        }
        if (random > .35f && random <= .45f)
        {
            DropShield(pozice);
        }
        else
        {
            return;
        }
    }
    public void DropHealth(Vector3 pozice)
    {
        Vector3 vector3 = new Vector3(pozice.x, 0.15f, pozice.z);
        GameObject health = Instantiate(healthPrefab, vector3, Quaternion.identity);
        health.AddComponent<PowerUp>().type = PowerUp.PowerUpType.Health;
        health.GetComponent<PowerUp>().pick = pick;

        SphereCollider col = health.AddComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = 1f;
    }
    public void DropAmmo(Vector3 pozice)
    {
        Vector3 vector3 = new Vector3(pozice.x, 0.15f, pozice.z);
        GameObject ammo = Instantiate(ammoPrefab, vector3, Quaternion.identity);
        ammo.AddComponent<PowerUp>().type = PowerUp.PowerUpType.Ammo;
        ammo.GetComponent<PowerUp>().pick = pick;


        SphereCollider col = ammo.AddComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = 1f;
    }
    public void DropShield(Vector3 pozice)
    {
        Vector3 vector3 = new Vector3(pozice.x, 0.15f, pozice.z);
        GameObject shield = Instantiate(shieldPrefab, vector3, Quaternion.identity);
       
        shield.AddComponent<PowerUp>().type = PowerUp.PowerUpType.Shield;
        shield.GetComponent<PowerUp>().pick = pick;


        SphereCollider col = shield.AddComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = 1f;
    }
}
