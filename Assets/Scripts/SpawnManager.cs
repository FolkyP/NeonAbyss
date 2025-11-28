using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance;

    [Header("References")]
    public GameObject[] enemyPrefabs; // 0 = Melee, 1 = Ranged, 2 = Explosion (nastav v inspectoru)
    public bool autoFindSpawnPoints = true;
    public List<Transform> spawnPoints = new List<Transform>();

    [Header("Spawn settings - Phase 1")]
    public float spawnIntervalPhase1 = 5f;
    public int maxActiveEnemies = 12;
    public int spawnPerWave = 1;

    [Header("Phase 2 (harder/faster)")]
    public bool inPhase2 = false;
    public float spawnIntervalPhase2 = 2f;
    public float healthMultiplierPhase2 = 1.6f;
    public float damageMultiplierPhase2 = 1.5f;
    public float agentSpeedMultiplierPhase2 = 1.2f;

    [Header("Misc")]
    public bool startWhenGameStarts = true;

    private Coroutine spawnLoopCoroutine;
    private bool spawning = false;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        Instance = this;
    }

    private void Start()
    {
        if (autoFindSpawnPoints)
        {
            // 1) hledá objekty s komponentou SpawnPoint
            SpawnPoint[] pts = FindObjectsOfType<SpawnPoint>();
            foreach (var p in pts) spawnPoints.Add(p.transform);

            // 2) fallback: hledej objekty se tagem "SpawnPoint"
            if (spawnPoints.Count == 0)
            {
                GameObject[] byTag = GameObject.FindGameObjectsWithTag("SpawnPoint");
                foreach (var g in byTag) spawnPoints.Add(g.transform);
            }
        }

        
    }

    private void Update()
    {
        if (!spawning && startWhenGameStarts && GameSettings.Instance != null && GameSettings.Instance.isGameOn)
        {
            StartSpawning();
        }
    }

    public void StartSpawning()
    {
        if (spawnLoopCoroutine != null) StopCoroutine(spawnLoopCoroutine);
        spawning = true;
        spawnLoopCoroutine = StartCoroutine(SpawnLoop());
    }

    public void EnterPhase2()
    {
        inPhase2 = true;

        // restartujeme spawnování, aby se interval okamžitì zmìnil
        if (spawnLoopCoroutine != null)
        {
            StopCoroutine(spawnLoopCoroutine);
        }
        spawnLoopCoroutine = StartCoroutine(SpawnLoop());
    }


    public void StopSpawning()
    {
        spawning = false;
        if (spawnLoopCoroutine != null)
        {
            StopCoroutine(spawnLoopCoroutine);
            spawnLoopCoroutine = null;
        }
    }

    // zavolat když skonèí countdown -> zastaví spawnování a zabije všechny nepøátele
    public void StopAndKillAll()
    {
        startWhenGameStarts = false;
        StopSpawning();

        EnemyAI[] enemies = FindObjectsOfType<EnemyAI>();
        foreach (var e in enemies)
        {
            // znièení bez pøidání skóre (Destroy pøímo)
            Destroy(e.gameObject);
        }
    }

    IEnumerator SpawnLoop()
    {
        while (spawning)
        {
            // limit aktivních nepøátel
            EnemyAI[] active = FindObjectsOfType<EnemyAI>();
            if (active.Length < maxActiveEnemies && spawnPoints.Count > 0 && enemyPrefabs.Length > 0)
            {
                int toSpawn = spawnPerWave;
                for (int i = 0; i < toSpawn; i++)
                {
                    // náhodný spawnPoint (mùžeš zmìnit na round-robin)
                    Transform sp = spawnPoints[Random.Range(0, spawnPoints.Count)];
                    // náhodný typ nepøítele
                    GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
                    
                    GameObject go = Instantiate(prefab, sp.position, Quaternion.identity);
                    if (prefab == enemyPrefabs[1])
                    {
                        Vector3 pos = go.transform.position;
                        pos.y += 2.5f;
                        go.transform.position = pos;
                    }
                    EnemyAI enemyAI = go.GetComponent<EnemyAI>();
                    if (enemyAI != null && enemyAI.player == null)
                    {
                        enemyAI.player = GameObject.Find("MainCharacter");


                    }
                    // aplikuj modifikátory pro fázi 2
                    if (inPhase2)
                    {
                        ApplyPhase2Modifiers(go);
                    }
                }
            }

            float wait = inPhase2 ? spawnIntervalPhase2 : spawnIntervalPhase1;
            yield return new WaitForSeconds(wait);
        }
    }

    private void ApplyPhase2Modifiers(GameObject enemy)
    {
        var eh = enemy.GetComponent<EnemyHealth>();
        if (eh != null)
        {
            eh.maxHealth *= healthMultiplierPhase2;
            eh.currentHealth = eh.maxHealth;
        }

        var ai = enemy.GetComponent<EnemyAI>();
        if (ai != null)
        {
            ai.attackDamage = Mathf.CeilToInt(ai.attackDamage * damageMultiplierPhase2);
            var agent = enemy.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null)
            {
                agent.speed *= agentSpeedMultiplierPhase2;
            }
        }
    }

    #region Editor helpers
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        foreach (var sp in spawnPoints)
        {
            if (sp != null)
                Gizmos.DrawWireSphere(sp.position, 0.5f);
        }
    }
#endif
    #endregion
}
