using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CrystalManager : MonoBehaviour
{
    
    public float dropChance = .15f;
    public GameObject crystalPrefab;
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

        //if (Random.value <= dropChance)
        //{
        if (totalCrystals == 4) return;
            Vector3 vector3 = new Vector3(pozice.x, 0.15f, pozice.z);
            GameObject crystal = Instantiate(crystalPrefab, vector3, Quaternion.identity);
            crystal.transform.rotation = Quaternion.Euler(-90, 0, 0);
            crystal.AddComponent<CrystalItem>();
            crystal.AddComponent<SphereCollider>();
            crystal.GetComponent<SphereCollider>().isTrigger = true;
        totalCrystals++;

        //}
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
    // Update is called once per frame
    void Update()
    {
        
    }
}
