using UnityEngine;

public class CrystalItem : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Najdeme CrystalManager a pøidáme krystal
            CrystalManager manager = FindObjectOfType<CrystalManager>();
            manager.PickCrystal();

            // Znièíme item
            Destroy(gameObject);
        }
    }
}
