using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;

    private EnemyAI enemyAI;
    private CrystalManager crystalManager;
    void Awake()
    {
        currentHealth = maxHealth;
        enemyAI = GetComponent<EnemyAI>();
        crystalManager = FindObjectOfType<CrystalManager>();
    }

    // This matches what your weapon is sending
    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        Debug.Log($"{gameObject.name} took {amount} damage, health = {currentHealth}");

        if (currentHealth <= 0f)
        {
            
            if (enemyAI.enemyType == EnemyAI.EnemyType.Ranged)
            {
                Score.Instance.AddScore(15);

                crystalManager.Drop(transform.position);
                enemyAI.RangedDead();

                enemyAI.isDead = true;


            }
            if(enemyAI.enemyType == EnemyAI.EnemyType.Melee)
            {
                Score.Instance.AddScore(40);
                crystalManager.Drop(transform.position);
                enemyAI.MeleeDead();
                enemyAI.isDead = true;
            }
            if(enemyAI.enemyType == EnemyAI.EnemyType.Explosion)
            {
                //jestli je zabit, tak score add, jestli sam tak nn
                Score.Instance.AddScore(20);
                crystalManager.Drop(transform.position);
                Destroy(gameObject);
                enemyAI.isDead = true;
            }
        }
    }

    
}
