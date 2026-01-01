using System.Collections;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

public class BossController : MonoBehaviour
{
    [Header("Player Reference")]
    public Transform player;

    [Header("General Settings")]
    public bool isAttacking = true;
    [Tooltip("Aktivní fáze (1..3) - øídí které útoky jsou dostupné")]
    public int activePhase = 1;

    [Header("Look Settings (Intermittent)")]
    public float rotationSpeed = 5f;
    public float minLookInterval = 1.5f;
    public float maxLookInterval = 3f;
    private float nextLookTime;
    private Coroutine lookRoutine;

    [Header("Attack Preview Settings (global)")]
    public float previewDuration = 2f;

    [Header("Ground Trap (A)")]
    public GameObject groundTrapPreviewPrefab; // kruh preview (GroundPreview)
    public GameObject groundTrapPrefab;        // damage prefab (GroundTrapDamage)
    public float groundCheckHeight = 10f;
    public LayerMask groundLayer = ~0;
    public float groundTrapCooldown = 6f;
    public float trapDetectRadius = 1.5f;
    public LayerMask playerLayerMask;
    private float nextGroundTrapTime = 0f;
    private bool groundTrapRunning = false;
    [Tooltip("Distance in front of boss used as fallback spawn point")]
    public float groundTrapFallbackDistance = 3f;
    private Vector3 groundTrapScale;
    private float groundTrapYOffset = -0.3f;
   
    [Header("Laser Beam (B)")]
    public GameObject laserPreviewPrefab; // should visually show a box/beam
    public GameObject laserDamagePrefab;  // should contain LaserDamage component with collider
    public float laserCooldown = 5f;
    public float nextLaserTime = 0f;
    public float laserRange = 12f;
    public float laserWidth = 2f;
    public float laserHeight = 1.0f; // thickness of the beam (y)
    private bool laserRunning = false;
    [Header("Laser Ring Settings")]
    [Range(4, 32)]
    public int laserCount = 12;

    public float laserLength = 14f;        // délka laseru
    public float laserRadiusOffset = 0f;
    [Header("Laser Colors")]
    public Color phase1PreviewColor = Color.yellow;
    public Color phase2PreviewColor = Color.red;
    public Color phase3PreviewColor = Color.magenta;

    public Color phase1LaserColor = new Color(1f, 1f, 0.2f); // jasnì žlutá
    public Color phase2LaserColor = Color.red;
    public Color phase3LaserColor = Color.magenta;

    [Header("Shockwave (C)")]
    public GameObject shockwavePreviewPrefab; // visual radius
    public GameObject shockwaveDamagePrefab;  // ShockwaveDamage script on prefab
    public float shockwaveCooldown = 8f;
    public float nextShockwaveTime = 0f;
    public float shockwaveRadius = 6f;
    private bool shockwaveRunning = false;
    [Tooltip("Upward impulse applied to player when shockwave hits")]
    public float shockwaveKnockupForce = 6f;

    // for tracking coroutines so StopAllAttacks is safe
    private List<Coroutine> activeAttackCoroutines = new List<Coroutine>();
    [Header("Attack Scheduling")]
    public float globalAttackGap = 0.6f;
    private float nextGlobalAttackTime = 0f;

    [Header("Rotation Lock")]
    [Tooltip("Lock boss pitch (x rotation) to this degree value (world euler X).")]
    public float lockedPitch = 80f;

    [Header("Laser Beam (B)")]
    [Tooltip("World-space Y position where laser previews/damage will be spawned.")]
    public float laserSpawnY = 1.0f;
    // tracks any GameObject spawned by attack coroutines so we can force-destroy them if needed
    private List<GameObject> activeSpawnedObjects = new List<GameObject>();

    [Header("Levitation Settings")]
    public bool enableLevitation = true;
    public float levitateAmplitude = 2f;  // Distance up and down
    public float levitateFrequency = 1.0f;  // Speed of oscillation
    private Vector3 startPos;
    void Start()
    {
        startPos = transform.position;
        ScheduleNextLook();
        nextGroundTrapTime = Time.time + Random.Range(1f, groundTrapCooldown);
        nextLaserTime = Time.time + Random.Range(2f, laserCooldown);
        nextShockwaveTime = Time.time + Random.Range(3f, shockwaveCooldown);
        if (groundTrapPrefab != null)
        {
            groundTrapScale = groundTrapPrefab.transform.localScale;

            // Pokud má damage collider, dopoèítáme pøesný Y offset
            var col = groundTrapPrefab.GetComponent<Collider>();
            if (col != null)
                groundTrapYOffset = col.bounds.extents.y;
        }
        // enforce locked pitch at start (keeps initial orientation consistent)
        Vector3 startEuler = transform.eulerAngles;
        startEuler.x = lockedPitch;
        transform.eulerAngles = startEuler;


    }

    void Update()
    {
        if (enableLevitation)
        {
            ApplyLevitation();
        }
        if (!isAttacking) return;

        // look logic
        if (Time.time >= nextLookTime && lookRoutine == null)
        {
            lookRoutine = StartCoroutine(RotateToPlayerRoutine());
            ScheduleNextLook();
        }

        HandlePhaseAttacks();
    }

    void HandlePhaseAttacks()
    {
        // Ve všech fázích voláme VŠECHNY útoky
        TryStartGroundTrap();
        TryStartLaser();
        //TryStartShockwave();
    }

    void TryStartGroundTrap()
    {
        if (groundTrapRunning) return;
        if (Time.time < nextGroundTrapTime) return;
        if (Time.time < nextGlobalAttackTime) return;

        Coroutine c = StartCoroutine(GroundTrapAttackRoutine());
        activeAttackCoroutines.Add(c);
        groundTrapRunning = true;

        nextGroundTrapTime = Time.time + groundTrapCooldown;
        nextGlobalAttackTime = Time.time + globalAttackGap;
    }

    void TryStartLaser()
    {
        if (laserRunning) return;
        if (Time.time < nextLaserTime) return;
        if (Time.time < nextGlobalAttackTime) return;

        Coroutine c = StartCoroutine(LaserAttackRoutine());
        activeAttackCoroutines.Add(c);
        groundTrapRunning = true;
        nextLaserTime = Time.time + laserCooldown;
        nextGlobalAttackTime = Time.time + globalAttackGap;
    }

    void TryStartShockwave()
    {
        if (shockwaveRunning) return;
        if (Time.time < nextShockwaveTime) return;
        if (Time.time < nextGlobalAttackTime) return;

        StartCoroutine(ShockwaveAttackRoutine());
        shockwaveRunning = true;
        nextShockwaveTime = Time.time + shockwaveCooldown;
        nextGlobalAttackTime = Time.time + globalAttackGap;
    }
    void ApplyPhaseSettings(
    float trapCd,
    float laserCd,
    float shockwaveCd,
    float globalGap)
    {
        groundTrapCooldown = trapCd;
        laserCooldown = laserCd;
        shockwaveCooldown = shockwaveCd;
        globalAttackGap = globalGap;

        float now = Time.time;
        nextGroundTrapTime = now + 1f;
        nextLaserTime = now + 1.5f;
        nextShockwaveTime = now + 2f;
    }

    #region Look Rotation
    void ScheduleNextLook()
    {
        nextLookTime = Time.time + Random.Range(minLookInterval, maxLookInterval);
    }

    void ApplyLevitation()
    {
        // Math formula for smooth oscillation: 
        // StartPosition + Sine(Time * Speed) * Height
        float newY = startPos.y + Mathf.Sin(Time.time * levitateFrequency) * levitateAmplitude;

        // Update position while keeping X and Z current (in case the boss moves)
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    // --- IMPORTANT FIX FOR ROTATION ---
    // Since we are now moving the boss's Y position via script, 
    // the "RotateToPlayerRoutine" needs to make sure it doesn't fight 
    // with the ApplyLevitation function.

    IEnumerator RotateToPlayerRoutine()
    {
        yield return null;

        while (player != null)
        {
            Vector3 direction = player.position - transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(-direction.normalized);
                Vector3 targetEuler = targetRot.eulerAngles;
                targetEuler.x = lockedPitch;
                targetEuler.z = 0f;
                targetRot = Quaternion.Euler(targetEuler);

                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRot,
                    rotationSpeed * Time.deltaTime
                );
            }
            yield return null;
        }
    }




    #endregion

    #region Ground Trap Attack (A)
    IEnumerator GroundTrapAttackRoutine()
    {
        groundTrapRunning = true;

        // pick target point (prefer under player)
        Vector3 targetPoint = transform.position + transform.forward * groundTrapFallbackDistance;
        if (player != null)
        {
            Vector3 rayStart = player.position + Vector3.up * groundCheckHeight;
            if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, groundCheckHeight * 2f, groundLayer))
            {
                targetPoint = hit.point;
                targetPoint.y = 0.4f;

            }
            else
            {
                rayStart = transform.position + transform.forward * groundTrapFallbackDistance + Vector3.up * groundCheckHeight;
                if (Physics.Raycast(rayStart, Vector3.down, out hit, groundCheckHeight * 2f, groundLayer))
                {
                    targetPoint = hit.point;
                    targetPoint.y = 0.4f;

                }
            }
        }
        else
        {
            // fallback: ray from boss forward
            Vector3 rayStart = transform.position + transform.forward * groundTrapFallbackDistance + Vector3.up * groundCheckHeight;
            if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, groundCheckHeight * 2f, groundLayer))
                targetPoint = hit.point;
        }

        GameObject preview = null;
        if (groundTrapPreviewPrefab != null)
        {
            Vector3 previewPos = targetPoint;

            preview = Instantiate(groundTrapPreviewPrefab, previewPos, Quaternion.identity);
            preview.transform.localScale = groundTrapScale;
            activeSpawnedObjects.Add(preview);

            var gp = preview.GetComponent<GroundPreview>();
            if (gp != null)
                gp.duration = previewDuration;
        }


        // wait preview
        float timer = 0f;
        while (timer < previewDuration)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        // spawn damage
        if (groundTrapPrefab != null)
        {
            GameObject trap = Instantiate(groundTrapPrefab, targetPoint, Quaternion.identity);
            activeSpawnedObjects.Add(trap);

            GroundTrapDamage gtd = trap.GetComponent<GroundTrapDamage>();
            float damageValue = (gtd != null) ? gtd.damage : 30f;

            Collider[] hits = Physics.OverlapSphere(targetPoint, trapDetectRadius, playerLayerMask);
            bool appliedOnSpawn = false;
            foreach (var col in hits)
            {
                if (col == null) continue;

                // Pokud má hráè komponentu PlayerLife (nebo jinou health komponentu) na parentu/pøímém objektu, volej ji pøímo
                var playerLife = col.GetComponentInParent<PlayerLife>();
                if (playerLife != null)
                {
                    playerLife.TakeDamage((int)damageValue);
                    appliedOnSpawn = true;
                    continue;
                }

                // pokud nemáme PlayerLife, zkúsíme SendMessageUpwards aby to našlo metodu i na parentu
                col.SendMessageUpwards("TakeDamage", (int)damageValue, SendMessageOptions.DontRequireReceiver);
                appliedOnSpawn = true;
            }

            if (appliedOnSpawn && gtd != null)
                gtd.appliedOnSpawn = true;

        }

        if (preview != null)
        {
            activeSpawnedObjects.Remove(preview);
            Destroy(preview);
        }


        groundTrapRunning = false;
        activeAttackCoroutines.RemoveAll(x => x == null); // cleanup
    }
    #endregion

    #region Laser Attack (B)
    IEnumerator LaserAttackRoutine()
    {
        laserRunning = true;

        // use configured world Y for laser spawn height
        Vector3 origin = new Vector3(transform.position.x, laserSpawnY, transform.position.z);


        // ===== PREVIEW =====
        List<GameObject> previews = new List<GameObject>();

        float angleStep = 360f / laserCount;

        // smìr k hráèi
        Vector3 toPlayer = (player.position - transform.position);
        toPlayer.y = 0f;
        float playerAngle = Mathf.Atan2(toPlayer.x, toPlayer.z) * Mathf.Rad2Deg;

        // spawn preview
        for (int i = 0; i < laserCount; i++)
        {
            float angle = playerAngle + (i * angleStep);
            Quaternion rot = Quaternion.Euler(0f, angle, 0f);

            Vector3 center = origin + rot * Vector3.forward * (laserLength * 0.5f + laserRadiusOffset);

            GameObject preview = Instantiate(laserPreviewPrefab, center, rot);
            previews.Add(preview);
            activeSpawnedObjects.Add(preview);


            // ZDE JE OPRAVA PRO VIZUÁLNÍ PREVIEW:
            preview.transform.localScale = new Vector3(laserWidth, laserHeight, laserLength); // <--- PØIDÁNO: Zmìna velikosti modelu

            // nastav barvu preview
            MeshRenderer mr = preview.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                mr.material = new Material(mr.material);
                mr.material.color = GetPreviewColor();
            }

            previews.Add(preview);
        }

        yield return new WaitForSeconds(previewDuration);

        // ===== DAMAGE =====
        foreach (var p in previews)
        {
            activeSpawnedObjects.Remove(p);
            Destroy(p);
        }


        for (int i = 0; i < laserCount; i++)
        {
            float angle = playerAngle + (i * angleStep);
            Quaternion rot = Quaternion.Euler(0f, angle, 0f);

            Vector3 center = origin + rot * Vector3.forward * (laserLength * 0.5f + laserRadiusOffset);

            GameObject laser = Instantiate(laserDamagePrefab, center, rot);
            activeSpawnedObjects.Add(laser);

            // ZDE JE OPRAVA PRO VIZUÁLNÍ LASER:
            laser.transform.localScale = new Vector3(laserWidth, laserHeight, laserLength); // <--- PØIDÁNO: Zmìna velikosti modelu

            MeshRenderer mrLaser = laser.GetComponent<MeshRenderer>();
            if (mrLaser != null)
            {
                mrLaser.material = new Material(mrLaser.material);
                mrLaser.material.color = Color.red;
            }

            // nastav parametry LaserDamage
            LaserDamage ld = laser.GetComponent<LaserDamage>();
            if (ld != null)
            {
                ld.length = laserLength;
                ld.width = laserWidth;
                ld.height = laserHeight;
                ld.owner = this;
                ld.damage = 20;
            }

            // pøidáme collider pokud není
            BoxCollider bc = laser.GetComponent<BoxCollider>();
            if (bc == null)
                bc = laser.AddComponent<BoxCollider>();

            bc.isTrigger = true;
            

            bc.size = Vector3.one; // <--- ZMÌNA: Protože jsme zvìtšili celý objekt pomocí Scale, collider musí být 1, jinak by byl obrovský (Scale * Size)
            bc.center = Vector3.zero;
        }

        laserRunning = false;
    }

    Color GetPreviewColor()
    {
        switch (activePhase)
        {
            case 1: return phase1PreviewColor;
            case 2: return phase2PreviewColor;
            case 3: return phase3PreviewColor;
            default: return Color.white;
        }
    }

    Color GetLaserColor()
    {
        switch (activePhase)
        {
            case 1: return phase1LaserColor;
            case 2: return phase2LaserColor;
            case 3: return phase3LaserColor;
            default: return Color.white;
        }
    }

    #endregion

    #region Shockwave Attack (C)
    IEnumerator ShockwaveAttackRoutine()
    {
        shockwaveRunning = true;

        Vector3 center = transform.position;

        GameObject preview = null;
        if (shockwavePreviewPrefab != null)
        {
            preview = Instantiate(shockwavePreviewPrefab, center, Quaternion.identity);
            var sp = preview.GetComponent<ShockwavePreview>();
            if (sp != null) sp.duration = previewDuration;
            preview.transform.localScale = Vector3.one * (shockwaveRadius * 2f); // diameter scale if prefab assumes 1 unit = 1m
        }

        float timer = 0f;
        while (timer < previewDuration)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        // spawn damage prefab (may animate outward)
        if (shockwaveDamagePrefab != null)
        {
            GameObject sw = Instantiate(shockwaveDamagePrefab, center, Quaternion.identity);
            ShockwaveDamage sd = sw.GetComponent<ShockwaveDamage>();
            if (sd != null)
            {
                sd.radius = shockwaveRadius;
                sd.damage = sd.damage; // keep configured
                sd.knockupForce = shockwaveKnockupForce;
            }

            // immediate check for players
            Collider[] hits = Physics.OverlapSphere(center, shockwaveRadius, playerLayerMask);
            foreach (var col in hits)
            {
                if (col != null && (col.CompareTag("Player") || ((1 << col.gameObject.layer) & playerLayerMask) != 0))
                {
                    // apply damage
                    int dmg = (sd != null) ? Mathf.RoundToInt(sd.damage) : 15;
                    col.SendMessage("TakeDamage", dmg, SendMessageOptions.DontRequireReceiver);

                    // try to apply upward force (player Rigidbody) or call method ApplyKnockup
                    Rigidbody rb = col.attachedRigidbody;
                    if (rb != null)
                    {
                        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z); // reset vertical
                        rb.AddForce(Vector3.up * shockwaveKnockupForce, ForceMode.VelocityChange);
                    }
                    else
                    {
                        col.SendMessage("ApplyKnockup", shockwaveKnockupForce, SendMessageOptions.DontRequireReceiver);
                    }
                }
            }
        }

        if (preview != null) Destroy(preview);

        shockwaveRunning = false;
        activeAttackCoroutines.RemoveAll(x => x == null);
    }
    #endregion

    #region Public Controls
    public void StartPhase(int phase)
    {
        activePhase = phase;

        switch (phase)
        {
            case 1:
                ApplyPhaseSettings(
                    trapCd: 6.0f,
                    laserCd: 7.0f,
                    shockwaveCd: 999f,   // prakticky vypnuté
                    globalGap: 0.6f
                );
                break;

            case 2:
                ApplyPhaseSettings(
                    trapCd: 4.5f,
                    laserCd: 5.0f,
                    shockwaveCd: 8.0f,
                    globalGap: 0.45f
                );
                break;

            case 3:
                ApplyPhaseSettings(
                    trapCd: 3.5f,
                    laserCd: 4.0f,
                    shockwaveCd: 6.0f,
                    globalGap: 0.3f
                );
                break;
        }

        // malý delay po zmìnì fáze
        nextGlobalAttackTime = Time.time + 1.0f;
    }


    public void StopAllAttacks()
    {
        isAttacking = false;

        // Stop tracked attack coroutines
        foreach (var c in activeAttackCoroutines)
        {
            if (c != null) StopCoroutine(c);
        }
        activeAttackCoroutines.Clear();

        // destroy any previews / damage objects that might have been left by a prematurely-stopped coroutine
        for (int i = activeSpawnedObjects.Count - 1; i >= 0; i--)
        {
            var go = activeSpawnedObjects[i];
            if (go != null)
                Destroy(go);
        }
        activeSpawnedObjects.Clear();

        // stop look routine too
        if (lookRoutine != null) StopCoroutine(lookRoutine);
        lookRoutine = null;

        // reset flags
        groundTrapRunning = laserRunning = shockwaveRunning = false;
    }

    #endregion

    #region Debug
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 p = transform.position + transform.forward * groundTrapFallbackDistance;
        Gizmos.DrawWireSphere(p, trapDetectRadius);

        Gizmos.color = Color.yellow;
        Vector3 center = transform.position + transform.forward * (laserRange * 0.5f) + Vector3.up * (laserHeight * 0.5f);
        Gizmos.DrawWireCube(center, new Vector3(laserWidth, laserHeight, laserRange));

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, shockwaveRadius);
    }
    #endregion
}
