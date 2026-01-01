using UnityEngine;

public class LaserDamage : MonoBehaviour
{
    public float damage = 20f;
    public float length = 12f;
    public float width = 2f;
    public float height = 1f;
    public float lifetime = 0.6f;
    public BossController owner;

    void Start()
    {
        BoxCollider bc = GetComponent<BoxCollider>();
        if (bc == null)
        {
            bc = gameObject.AddComponent<BoxCollider>();
            bc.isTrigger = true;
        }

        // PÙVODNÍ KÓD: bc.size = new Vector3(width, height, length);
        // NOVÝ KÓD: Necháme velikost collideru na 1, protože velikost urèuje transform.localScale rodièe
        bc.size = Vector3.one;

        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerLife.Instance.TakeDamage((int)damage);

            other.SendMessage("TakeDamage", (int)damage, SendMessageOptions.DontRequireReceiver);
            // optionally apply knockback here
        }
    }
}
