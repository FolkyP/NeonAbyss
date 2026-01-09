using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance;

    [Header("References")]
    public GameObject player;

    public GameObject[] enemyPrefabs; // 0 = Melee, 1 = Ranged, 2 = Explosion (nastav v inspectoru)
    public List<Transform> spawnPoints = new List<Transform>();

    [Header("Map Spawn Points (Manually assigned)")]
    public Transform map1PlayerStart;
    public List<Transform> map1SpawnPoints = new List<Transform>();
    public Transform map2PlayerStart;
    public List<Transform> map2SpawnPoints = new List<Transform>();
    public Transform map3PlayerStart;
    public List<Transform> map3SpawnPoints = new List<Transform>();

    [Header("Map1 Spawn settings - Phase 1")]
    public float spawnIntervalPhase1 = 5f;
    public int maxActiveEnemies = 12;
    public int spawnPerWave = 1;

    [Header("Map1 Phase 2 (harder/faster)")]
    public bool inPhase2 = false;
    public float spawnIntervalPhase2 = 2f;
    public float healthMultiplierPhase2 = 1.6f;
    public float damageMultiplierPhase2 = 1.5f;
    public float agentSpeedMultiplierPhase2 = 1.2f;

    [Header("Misc")]
    public bool startWhenGameStarts = true;

    private Coroutine spawnLoopCoroutine;
    private bool spawning = false;

    private int mapSystem;

    private bool bossSpawning = false;
    public bool isPhaseForSpawn = false;
    public List<Transform> spawnPointsForBoss = new List<Transform>();

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        Instance = this;
    }

    private void Start()
    {
    }

  
    private void Update()
    {
        
        if (!spawning && startWhenGameStarts && GameSettings.Instance != null && GameSettings.Instance.isGameOn && mapSystem ==0 && GameSettings.Instance.isOverDriveActive == false)
        {
            StartSpawning();
        }
        if(GameSettings.Instance.isOverDriveActive)
        {
            StopSpawning();
        }
        if(mapSystem == 1)
        {
            //Wave mapa
        }
        if(mapSystem == 2 && isPhaseForSpawn)
        {
            //Boss
            StartBossSpawning();
        }
    }
    #region Map1
    public void StartSpawning()
    {
        if (spawnLoopCoroutine != null) StopCoroutine(spawnLoopCoroutine);
        spawning = true;
        spawnLoopCoroutine = StartCoroutine(SpawnLoop());
    }
    public void StartBossSpawning() {
        if (spawnLoopCoroutine != null) StopCoroutine(spawnLoopCoroutine);

        bossSpawning = true;
        spawnLoopCoroutine = StartCoroutine(SpawnBossLoop());

    }
    public void EnterPhase2()
    {
        inPhase2 = true;
        startWhenGameStarts = true;

        StartSpawning();
        
    }


    public void StopSpawning()
    {
        spawning = false;
        bossSpawning = false;
        isPhaseForSpawn = false;
        if (spawnLoopCoroutine != null)
        {
            StopCoroutine(spawnLoopCoroutine);
            spawnLoopCoroutine = null;
        }
    }

    // zavolat když skonèí countdown  zastaví spawnování a zabije všechny nepøátele
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
    IEnumerator SpawnBossLoop() {
        while (bossSpawning)
        {
            EnemyAI[] active = FindObjectsOfType<EnemyAI>();
            if (active.Length < 8 && spawnPointsForBoss.Count > 0 && enemyPrefabs.Length > 0)
            {
                int toSpawn = spawnPerWave;
                for (int i = 0; i < toSpawn; i++)
                {
                    Transform sp = spawnPointsForBoss[Random.Range(0, spawnPointsForBoss.Count)];
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
                    
                }
            }
            float wait = 3f;
            yield return new WaitForSeconds(wait);
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
    #endregion
    public void ApplyMapIndex(int index)
    {
        startWhenGameStarts = true;
        switch (index)
        {
            default:
            case 0:
                spawnPoints = new List<Transform>(map1SpawnPoints);
                if (player != null && map1PlayerStart != null)
                {
                    player.transform.position = map1PlayerStart.position;
                    player.transform.rotation = map1PlayerStart.rotation;
                    mapSystem = 0;
                }
                break;
            case 1:
                spawnPoints = new List<Transform>(map2SpawnPoints);
                if (player != null && map2PlayerStart != null)
                {
                    player.transform.position = map2PlayerStart.position;
                    player.transform.rotation = map2PlayerStart.rotation;
                    mapSystem = 2;
                }
                break;
            case 2:
                spawnPoints = new List<Transform>(map3SpawnPoints);
                if (player != null && map3PlayerStart != null)
                {
                    player.transform.position = map3PlayerStart.position;
                    player.transform.rotation = map3PlayerStart.rotation;
                    mapSystem = 1;
                }
                break;
        }
    }
    public void ResetForNewMap()
    {
        StopAndKillAll();
        ApplyMapIndex(GameSettings.Instance.currentMapIndex);
    }

    //public void ConfigureForGameMode(GameSettings.GameMode mode)
    //{
    //    switch (mode)
    //    {
    //        case GameSettings.GameMode.Survival:
    //            startWhenGameStarts = true;
    //            spawnIntervalPhase1 = 4f;
    //            maxActiveEnemies = 16;
    //            break;
    //        case GameSettings.GameMode.Waves:
    //            startWhenGameStarts = false;
    //            spawnIntervalPhase1 = 6f;
    //            maxActiveEnemies = 10;
    //            break;
    //        case GameSettings.GameMode.Final:
    //            startWhenGameStarts = false;
    //            spawnIntervalPhase1 = 3f;
    //            maxActiveEnemies = 20;
    //            break;
    //        default:
    //            // nic mìnit
    //            break;
    //    }
    //}
}
