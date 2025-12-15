using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossController : MonoBehaviour
{
    [Header("Player Reference")]
    public Transform player;

    [Header("General Settings")]
    public bool isAttacking = true;
    public int activePhase = 1;

    [Header("Look Settings (Intermittent)")]
    public float rotationSpeed = 5f;
    public float minLookInterval = 1.5f;
    public float maxLookInterval = 3f;
    private float nextLookTime;
    private bool isRotating = false;

    [Header("Phase 1: Lasers & Rockets")]
    public GameObject laserPivot; // Objekt, který se toèí a má na sobì lasery
    public GameObject rocketPrefab;
    public Transform[] firePoints; // Místa odkud vylétají rakety
    public float laserRotationSpeed = 30f;

    [Header("Phase 2: Traps & Minions")]
    public GameObject groundTrapPrefab; // Magické pole (koleèko) - útok
    public GameObject airBaitPrefab;    // Falešný útok ve vzduchu (bez preview)
    public GameObject minionPrefab;     // Malí nepøátelé
    public Transform[] minionSpawnPoints;

    [Header("Phase 3: Pillars & Artillery")]
    public GameObject shieldVisual;     // Vizuál štítu
    public List<GameObject> pillars;    // Seznam pilíøù v scénì
    public GameObject artilleryProjectilePrefab; // Tìžká støela
    public GameObject shockwavePrefab; // Efekt/skyshock (pøípadný)

    [Header("Attack Preview Settings")]
    public float previewDuration = 2f;
    public GameObject groundTrapPreviewPrefab;   // kruh na zemi
    public GameObject artilleryPreviewPrefab;    // kruh na zemi (jiná grafika)
    public GameObject rocketPreviewPrefab;       // obdélník pøed bossem
    public GameObject shockwavePreviewPrefab;    // rozšiøující se kruh (boss-centred)

    // interní
    private Coroutine attackRoutine;

    void Start()
    {
        ScheduleNextLook();
        if (laserPivot) laserPivot.SetActive(false);
        if (shieldVisual) shieldVisual.SetActive(false);
        isAttacking = true;
    }

    void Update()
    {
        if (Time.time >= nextLookTime)
        {
            StartCoroutine(RotateToPlayerRoutine());
            ScheduleNextLook();
        }

        // Fáze 1: Rotace laserù (pokud jsou aktivní)
        if (activePhase == 1 && laserPivot != null && laserPivot.activeSelf)
        {
            float dynamicSpeed = laserRotationSpeed * Mathf.Sin(Time.time * 0.5f);
            laserPivot.transform.Rotate(Vector3.up * dynamicSpeed * Time.deltaTime);
        }
    }

    public void StartPhase(int phase)
    {
        activePhase = phase;
        // Bez StopAllCoroutines — zastavíme jen attack loop
        if (attackRoutine != null)
        {
            StopCoroutine(attackRoutine);
            attackRoutine = null;
        }

        isAttacking = true;

        if (phase == 1)
        {
            if (laserPivot) laserPivot.SetActive(true);
            attackRoutine = StartCoroutine(AttackLoop_Phase1());
        }
        else if (phase == 2)
        {
            if (laserPivot) laserPivot.SetActive(false);
            attackRoutine = StartCoroutine(AttackLoop_Phase2());
        }
        else if (phase == 3)
        {
            ActivateShieldPhase();
            attackRoutine = StartCoroutine(AttackLoop_Phase3());
        }
    }

    public void StopAllAttacks()
    {
        if (attackRoutine != null)
        {
            StopCoroutine(attackRoutine);
            attackRoutine = null;
        }
        isAttacking = false;
        if (laserPivot) laserPivot.SetActive(false);
    }

    // ---------------- Phase Loops ----------------
    IEnumerator AttackLoop_Phase1()
    {
        // Lasery bìží v Update (pokud zapnuto)
        while (isAttacking)
        {
            // Rakety s preview (rect pøed bossem)
            yield return StartCoroutine(RocketAttack());

            // Volitelnì: menší shockwave z bosse (boss-centred)
            if (Random.value < 0.25f)
            {
                yield return StartCoroutine(ShockwaveAttack());
            }

            yield return new WaitForSeconds(1.5f);
        }
    }

    IEnumerator AttackLoop_Phase2()
    {
        while (isAttacking)
        {
            int rng = Random.Range(0, 3);

            switch (rng)
            {
                case 0:
                    // ground trap with ground preview
                    yield return StartCoroutine(GroundTrapAttack());
                    break;
                case 1:
                    SpawnAirBait(); // bez preview
                    break;
                case 2:
                    SpawnMinions(); // bez preview
                    break;
            }

            yield return new WaitForSeconds(1.0f + Random.value * 3f); // trochu variability
        }
    }

    IEnumerator AttackLoop_Phase3()
    {
        while (isAttacking)
        {
            // Artilerie na hráèe (ground circle preview)
            yield return StartCoroutine(ArtilleryAttack());

            yield return new WaitForSeconds(0.8f);

            // Náhodnì pøidat trap
            if (Random.value > 0.5f)
                yield return StartCoroutine(GroundTrapAttack());

            // Obèas shockwave
            if (Random.value < 0.3f)
                yield return StartCoroutine(ShockwaveAttack());

            yield return new WaitForSeconds(1.5f);
        }
    }

    // ---------------- Attack Implementations ----------------

    // Rocket: preview = obdélník vycházející z bosse smìrem k hráèi; po preview se spawnují rakety z firePoints
    IEnumerator RocketAttack()
    {
        // spawn rect preview in front of boss
        Vector3 previewPos = transform.position + transform.forward * 1.5f;
        Quaternion previewRot = Quaternion.LookRotation(transform.forward);

        GameObject preview = null;
        if (rocketPreviewPrefab != null)
            preview = Instantiate(rocketPreviewPrefab, previewPos, previewRot);

        // optional: align preview to player direction if you want (replace transform.forward by dirToPlayer)
        yield return new WaitForSeconds(previewDuration);

        if (preview) Destroy(preview);

        // Fire rockets from firePoints
        FireRockets();
    }

    // Ground trap: preview = kruh na zemi pod hráèem
    IEnumerator GroundTrapAttack()
    {
        Vector3 targetPos = player.position;
        targetPos.y = 0.05f;

        GameObject preview = null;
        if (groundTrapPreviewPrefab != null)
            preview = Instantiate(groundTrapPreviewPrefab, targetPos, Quaternion.Euler(90, 0, 0));

        yield return new WaitForSeconds(previewDuration);

        if (preview) Destroy(preview);

        Instantiate(groundTrapPrefab, targetPos, Quaternion.identity);
    }

    // Artillery: preview kruh, pak spawn projektilu ze vzduchu na pøesnou pozici hráèe
    IEnumerator ArtilleryAttack()
    {
        Vector3 targetPos = player.position;
        targetPos.y = 0.05f;

        GameObject preview = null;
        if (artilleryPreviewPrefab != null)
            preview = Instantiate(artilleryPreviewPrefab, targetPos, Quaternion.Euler(90, 0, 0));

        yield return new WaitForSeconds(previewDuration);

        if (preview) Destroy(preview);

        Vector3 spawnPos = targetPos + Vector3.up * 20f;
        Instantiate(artilleryProjectilePrefab, spawnPos, Quaternion.Euler(90, 0, 0));
    }

    // Shockwave: preview = expanding circle centered on boss; po preview spawnne efekt, který aplikuje damage/force
    IEnumerator ShockwaveAttack()
    {
        Vector3 targetPos = transform.position;
        targetPos.y = 0.05f;

        GameObject preview = null;
        if (shockwavePreviewPrefab != null)
            preview = Instantiate(shockwavePreviewPrefab, targetPos, Quaternion.Euler(90, 0, 0));

        yield return new WaitForSeconds(previewDuration);

        if (preview) Destroy(preview);

        if (shockwavePrefab != null)
            Instantiate(shockwavePrefab, targetPos, Quaternion.identity);
        yield return null;
    }

    // Support functions
    void SpawnAirBait()
    {
        Vector3 targetPos = player.position + Vector3.up * 5f;
        Instantiate(airBaitPrefab, targetPos, Quaternion.identity);
    }

    void SpawnMinions()
    {
        if (minionSpawnPoints != null && minionSpawnPoints.Length > 0)
        {
            Transform spawnPoint = minionSpawnPoints[Random.Range(0, minionSpawnPoints.Length)];
            Instantiate(minionPrefab, spawnPoint.position, spawnPoint.rotation);
        }
    }

    void ActivateShieldPhase()
    {
        if (shieldVisual) shieldVisual.SetActive(true);
        BossManager.Instance.isInvulnerable = true;
        foreach (var pillar in pillars)
            if (pillar != null) pillar.SetActive(true);
    }

    public void OnPillarDestroyed()
    {
        int activePillars = 0;
        foreach (var pillar in pillars)
            if (pillar != null && pillar.activeInHierarchy) activePillars++;

        if (activePillars <= 0)
        {
            BossManager.Instance.isInvulnerable = false;
            if (shieldVisual) shieldVisual.SetActive(false);
            Debug.Log("Štít prolomen! Boss je zranitelný.");
        }
    }

    void FireRockets()
    {
        if (firePoints == null) return;
        foreach (Transform point in firePoints)
        {
            Instantiate(rocketPrefab, point.position, point.rotation);
        }
    }

    void ScheduleNextLook()
    {
        nextLookTime = Time.time + Random.Range(minLookInterval, maxLookInterval);
    }

    IEnumerator RotateToPlayerRoutine()
    {
        isRotating = true;
        float duration = 1.0f;
        float timer = 0f;

        while (timer < duration)
        {
            if (player != null)
            {
                Vector3 direction = player.position - transform.position;
                direction.y = 0f;
                if (direction != Vector3.zero)
                {
                    Quaternion targetRot = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
                }
            }
            timer += Time.deltaTime;
            yield return null;
        }
        isRotating = false;
    }
}
