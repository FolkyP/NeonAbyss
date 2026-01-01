using UnityEngine;

public class GroundTrapDamage : MonoBehaviour
{
    public float damage = 30f;
    public float lifetime = 1.5f;
    [HideInInspector]
    public bool appliedOnSpawn = false;

    // Nová promìnná pro zabránìní vícenásobnému poškození
    private bool damageAppliedByTrigger = false; // <-- PØIDÁNO

    void Start()
    {
        Destroy(gameObject, lifetime);

        // Pokud již bylo poškození aplikováno v BossControlleru, 
        // mùžeme nastavit trigger jako neaktivní, nebo ho rovnou vypnout.
        if (appliedOnSpawn)
        {
            // Pokud poškození probìhlo v BossControlleru, 
            // už ho nechceme aplikovat znovu pøes trigger
            damageAppliedByTrigger = true;
            // Volitelnì: Mùžeme vypnout i trigger collider, aby se OnTriggerEnter nevolalo
            // GetComponent<Collider>().enabled = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Zkontrolujeme, zda už poškození triggerem probìhlo (pøípadnì jestli už bylo aplikováno BossControllerem v Start())
            if (damageAppliedByTrigger)
            {
                return;
            }

            // Aplikace poškození
            PlayerLife.Instance.TakeDamage((int)damage);
            other.SendMessage("TakeDamage", (int)damage, SendMessageOptions.DontRequireReceiver);

            // Oznaèíme, že poškození bylo udìleno, aby se neopakovalo
            damageAppliedByTrigger = true; // <-- ZMÌNA: Nastavíme vlajku

            // Volitelnì: Deaktivace collideru hned po prvním zásahu, 
            // aby se nevolaly další funkce:
            // GetComponent<Collider>().enabled = false;
        }
    }
}