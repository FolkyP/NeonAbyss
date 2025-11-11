using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(EnemyHealth))]
public class EnemyAI : MonoBehaviour
{
    [Header("References")]
    public Transform player;              // odkaz na hráèe
    private NavMeshAgent agent;
    private EnemyHealth enemyHealth;

    [Header("AI Settings")]
    public float chaseRange = 10f;        // vzdálenost, kdy zaène pronásledovat
    public float attackRange = 2f;        // vzdálenost, kdy zaène útoèit
    public int attackDamage = 10;      // kolik ubere
    public float attackCooldown = 1.5f;   // interval mezi útoky

    private float lastAttackTime = 0f;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        enemyHealth = GetComponent<EnemyHealth>();

        if (player == null)
        {
            // najdi hráèe podle tagu
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj)
                player = playerObj.transform;
        }
    }

    private void Update()
    {
        if (enemyHealth == null || player == null)
            return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= chaseRange)
        {
            // pronásleduj hráèe
            agent.SetDestination(player.position);

            if (distance <= attackRange)
            {
                Attack();
            }
        }
        else
        {
            // zastav, když je hráè daleko
            agent.ResetPath();
        }
    }

    private void Attack()
    {
        if (Time.time - lastAttackTime < attackCooldown)
            return;

        lastAttackTime = Time.time;

        // najdi komponentu PlayerHealth (musíš mít vlastní skript pro zdraví hráèe)
        PlayerLife playerHealth = player.GetComponent<PlayerLife>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(attackDamage);
            Debug.Log($"{gameObject.name} attacked player for {attackDamage} damage!");
        }
    }
}
