using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(EnemyHealth))]
public class EnemyAI : MonoBehaviour
{

    public enum EnemyType { Melee, Ranged, Explosion }
    [Header("Enemy Type")]
    public EnemyType enemyType;
    private bool isFrozen = false;

    [Header("Glory Kill")]
    public float gloryKillRange = 1.8f;
    private Renderer[] cachedRenderers;
    [SerializeField] private Color glowColor = Color.magenta;
    [SerializeField] private float glowIntensity = 3f;
    private MaterialPropertyBlock mpb;
    private bool glowActive;
    public bool IsGloryKillAvailable => glowActive && !isDead;

    [Header("References")]
    public GameObject player;              // odkaz na hráèe
    private NavMeshAgent agent;
    private EnemyHealth enemyHealth;
    public Transform firePoint;
    public Transform pew;
    public GameObject projectilePrefab;
    Animator animator;
    public AudioClip shootSound;
    public AudioClip explosion;
    public AudioClip shine;
    public AudioClip explosionPrefab;
    public AudioClip footstepSound;
    public AudioClip swing;
    public AudioClip bounce;
    public AudioClip Explo;
    public AudioClip roll;

    public Material explosionMaterial;


    [Header("AI Settings")]
    public float chaseRange = 10f;        // vzdálenost, kdy zaène pronásledovat
    public float attackRange = 2f;        // vzdálenost, kdy zaène útoèit
    public int attackDamage = 10;      // kolik ubere
    public float attackCooldown = 1.5f;   // interval mezi útoky
    public float shootRange = 50f;
    public float rotationSpeed = 10f;
    public float projectileSpeed = 10f;
    private float lastAttackTime = 0f;

   


    [Header("Floating Motion + Death")]
    public float floatAmplitude = 0.5f;  // jak vysoko se bude houpat
    public float floatFrequency = 1.25f;    // rychlost houpání
    private Vector3 startPosition;
    public float maxScale = 5f;
    public float growSpeed = 10f;

    [Header("Melee Death")]
    public float fallDuration = 1.2f;
    public float glowSpeed = 2f;          // rychlost rozsvícení
    public float maxEmission = 2f;        // max intenzita

    public bool isDead = false;

    [Header("ExploPref")]

    public GameObject target;       // GameObject, který se má otáèet
    public float rotationSpeedExplo = 10f;
    private float explodeRange = 2f;

    private AudioSource audioSource;

    public Color previewColor = new Color(1f, 0.85f, 0.2f); // žluto-oranžová
    public float previewIntensity = 4f;

    private bool isPreviewed = false;
    private bool isPaused = false;
    private void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        enemyHealth = GetComponent<EnemyHealth>();
        startPosition = transform.position;  // uložíme startovní pozici
        cachedRenderers = GetComponentsInChildren<Renderer>(true);
        mpb = new MaterialPropertyBlock();
    }
    private void Awake()
    {
        
            audioSource = GetComponent<AudioSource>();
        if (audioSource != null)
        {
            // Pokud je tato hodnota false, zvuk se pozastaví, když se pozastaví hra.
            audioSource.ignoreListenerPause = false;
        }
    }
    private void Update()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();

        if (GameSettings.Instance != null && GameSettings.Instance.isOverDriveActive)
        {
            if (!isFrozen)
                FreezeEnemy();
            return;
        }
        else
        {
            if (isFrozen)
                UnfreezeEnemy();
        }

        if (isFrozen)
            return;

        // základní null-checky
        if (enemyHealth == null || player == null) return;
        if (GameSettings.Instance != null && GameSettings.Instance.isGameOn == false) return;

        float distance = Vector3.Distance(transform.position, player.transform.position);

        // pokud agent není použitelný, vynech kód který s nim pracuje
        bool canAgent = CanUseAgent();

        // pokud používáš agent.isStopped jinde, vždy obalit canAgent:
        if (canAgent && agent.isStopped) { /* nemusí nic dìlat, ale nevolat když nelze */ }

        // ===== MELEE =====
        if (enemyType == EnemyType.Melee && !isDead)
        {
            if (canAgent)
            {
                agent.stoppingDistance = attackRange * 0.9f;

                if (distance <= chaseRange)
                {
                    if (distance <= agent.stoppingDistance)
                    {
                        if (!agent.isStopped) agent.isStopped = true;
                    }
                    else
                    {
                        if (agent.isStopped) agent.isStopped = false;
                        agent.SetDestination(player.transform.position);
                    }
                }
                else
                {
                    if (!agent.isStopped) agent.isStopped = true;
                }

                if (agent != null && agent.enabled && agent.isOnNavMesh)
                {
                    bool shouldRun = !agent.isStopped && agent.velocity.sqrMagnitude > 0.01f;
                    animator.SetBool("isRunning", shouldRun);
                }
                else
                {
                    animator.SetBool("isRunning", false);
                }

            }
            else
            {
                // fallback (agent nepoužitelný) - vypnout bìh/útok
                animator.SetBool("isRunning", false);
            }

            if (!isPaused && distance <= attackRange)
            {
                Attack();
            }
        }

        // ===== EXPLOSION =====
        if (enemyType == EnemyType.Explosion && !isDead)
        {
            if (target != null) target.transform.Rotate(0f, 0f, rotationSpeedExplo * Time.deltaTime);

            if (canAgent)
            {
                agent.updateRotation = true;
                agent.updatePosition = true;

                if (distance <= chaseRange)
                {
                    agent.isStopped = false;
                    agent.SetDestination(player.transform.position);
                    StartRollingSound();

                    if (distance <= explodeRange)
                    {
                        Explode();
                    }
                }
                else
                {
                    agent.isStopped = true;
                    StopRollingSound();
                }
            }
            else
            {
                // pokud agent není použitelný, zajisti, aby rolling zvuk nebìžel
                StopRollingSound();
            }
        }

        // ===== RANGED =====
        if (enemyType == EnemyType.Ranged && !isDead)
        {
            // ranged nepotøebuje navmesh pohyb — zpracuj bez nìj
            if (CanUseAgent())
            {
                agent.updatePosition = false;
                agent.updateRotation = false;
            }

            Vector3 pos = transform.position;
            pos.y = 3f;
            transform.position = pos;

            RotateTowardsPlayer();

            lastAttackTime -= Time.deltaTime;
            bool inRange = (player.transform.position - transform.position).sqrMagnitude <= shootRange * shootRange;

            if (!isPaused && lastAttackTime <= 0f && inRange)
            {
                ShootAtPlayer();
                lastAttackTime = attackCooldown;
            }

            float newY = startPosition.y + Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
            Vector3 floatPos = transform.position;
            floatPos.y = newY;
            transform.position = floatPos;
        }
        else
        {
            if (CanUseAgent())
            {
                agent.updatePosition = true;
                agent.updateRotation = true;
            }
        }
    }
    private void FreezeEnemy()
    {
        isFrozen = true;

        // NavMesh
        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        // Animace
        if (animator != null)
        {
            animator.SetBool("isRunning", false);
            animator.ResetTrigger("Attack");
            animator.speed = 0f; // STOP animací
        }

        // Zvuky
        StopRollingSound();
    }

    private void UnfreezeEnemy()
    {
        isFrozen = false;

        // Animace
        if (animator != null)
        {
            animator.speed = 1f;
        }
    }

    public void SetPaused(bool paused)
    {
        if (isPaused == paused) return;
        isPaused = paused;

        // zastav následující AI chování
        if (CanUseAgent())
        {
            agent.isStopped = paused;
            if (paused) agent.ResetPath(); // volitelné: vymaže cíl
        }

        // animace
        if (animator != null)
        {
            animator.SetBool("isRunning", false);
            if (paused) animator.ResetTrigger("Attack"); // bezpeènì zruší trigger
        }

        if (paused) StopRollingSound();
    }
    private bool CanUseAgent()
    {
        return agent != null && agent.enabled && agent.isOnNavMesh;
    }
    private void PlayEnemySFX(AudioClip clip)
    {
        if (clip == null || audioSource == null)
            return;

        audioSource.volume = AudioSettings.Instance.GetSfxVolume();
        audioSource.PlayOneShot(clip);
    }

    public void PlayFootstep()
    {
        if (isFrozen || isDead) return;
        PlayEnemySFX(footstepSound);
    }


    private void Explode()
    {
        if (isDead) return;
        isDead = true;

        agent.isStopped = true;
        agent.updateRotation = false;
        agent.updatePosition = false;

        StartCoroutine(ExplosionBehavior());
    }
    private void StartRollingSound()
    {
        if (audioSource == null || roll == null || isDead) return;
        
        // Pokud nehraje (nebo hraje, ale jiný klip), nastavíme ho a spustíme.
        if (!audioSource.isPlaying || audioSource.clip != roll)
        {
            audioSource.clip = roll;
            audioSource.loop = true; // Zapneme opakování
            audioSource.volume = AudioSettings.Instance.GetSfxVolume();
            audioSource.Play();
        }
    }

    private void StopRollingSound()
    {
        // Zastavíme pouze, pokud právì hraje klip pro kutálení
        if (audioSource != null && audioSource.isPlaying && audioSource.clip == roll)
        {
            audioSource.Stop();
            audioSource.loop = false; // Vypneme opakování pro pøíštì
            audioSource.clip = null;
        }
    }
    private IEnumerator ExplosionBehavior()
    {
        // --- 1) VÝSKOK SMÌREM K HRÁÈI ---
        if (agent != null) agent.enabled = false;
        PlayEnemySFX(bounce);
        Vector3 toPlayer = (player.gameObject.transform.position - transform.position).normalized;
        Vector3 jumpDir = (toPlayer + Vector3.up) * 0.3f;   
        float jumpDuration = 0.1f;
        float timer = 0f;

        Vector3 startPos = transform.position;
        Vector3 targetPos = transform.position + jumpDir;

        while (timer < jumpDuration)
        {
            timer += Time.deltaTime;
            float t = timer / jumpDuration;
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }
        SpawnInstantExplosion(transform.position + Vector3.up * 0.5f, Color.red);
        PlayEnemySFX(Explo);

        float dist = Vector3.Distance(transform.position, player.transform.position);
        if (dist < explodeRange)
        {
            PlayerLife hp = player.GetComponentInParent<PlayerLife>();
            if (hp != null) hp.TakeDamage(attackDamage * 2);

            // Optional: apply “visual knockback” on screen or camera shake instead of moving player
            PlayerCam.Instance.Shake(0.3f, 0.5f);
        }



        // enemy zmizne hned
        Destroy(gameObject);


    }

    private void Attack()
    {
        if (isFrozen || isDead) return;
        if (Time.time - lastAttackTime < attackCooldown)
            return;

        // zastavit pohyb pøi útoku
        agent.isStopped = true;
        animator.SetBool("isRunning", false);

        // spustit útokovou animaci
        animator.SetTrigger("Attack");

        lastAttackTime = Time.time;
        PlayEnemySFX(swing);
    }

    public void DealDamage()
    {
        if (player == null) return;

        PlayerLife playerHealth = player.GetComponentInParent<PlayerLife>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(attackDamage);
        }
    }
    public void AttackEnd()
    {
        // obnovíme pohyb AI po animaci
        if (!isDead)
            agent.isStopped = false;

        animator.ResetTrigger("Attack");
    }



    void RotateTowardsPlayer()
    {
        if (player == null) return;

        // Vypoèítej smìr k hráèi
        Vector3 direction = player.gameObject.transform.position - transform.position;

        if (direction.sqrMagnitude < 0.001f) return;

        // --- OTOÈENÍ TÌLA (jen horizontálnì) ---
        Vector3 flatDirection = new Vector3(direction.x, 0f, direction.z);
        if (flatDirection.sqrMagnitude > 0.001f)
        {
            Quaternion bodyRotation = Quaternion.LookRotation(-flatDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, bodyRotation, rotationSpeed * Time.deltaTime);
        }

        // --- OTOÈENÍ HLAVNÌ (nahoru/dolù) ---
        if (firePoint != null)
        {
            // Smìr z hlavnì k hráèi
            Vector3 lookDir = player.gameObject.transform.position - firePoint.position;

            // Cílová rotace pro hlaveò
            Quaternion aimRotation = Quaternion.LookRotation(-lookDir);

            // Vyhlazené natoèení hlavnì
            firePoint.rotation = Quaternion.Slerp(firePoint.rotation, aimRotation, rotationSpeed * Time.deltaTime);
        }
    }



    void ShootAtPlayer()
    {
        if (projectilePrefab == null || firePoint == null || player == null)
            return;

        // Calculate direction from fire point to player
        Vector3 direction = (player.gameObject.transform.position - firePoint.position).normalized;
        direction.y += 0.055f; // slight upward adjustment if needed  

        PlayEnemySFX(shootSound);
        // Spawn the projectile
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.LookRotation(direction));

        // Apply velocity
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = direction * projectileSpeed;
        }

        // Assign damage if your projectile script has it
        Projectile proj = projectile.GetComponent<Projectile>();
        if (proj != null)
        {
            proj.damage = attackDamage;
        }

        // Optionally: add muzzle flash, sound, etc.
        Debug.Log($"{gameObject.name} shot at player!");
    }
    public void RangedDead()
    {
        // zabrání spuštìní víckrát
        if (hasExploded) return;
        gameObject.GetComponent<Collider>().enabled = false;
        hasExploded = true;
        PlayEnemySFX(explosion);
        StartCoroutine(ExplosionEffect(transform.position,Color.magenta));
    }

    private bool hasExploded = false; // ochrana proti opakování
    private IEnumerator ExplosionEffect(Vector3 position,Color color)
    {
        // vytvoøíme kouli
        GameObject explosionSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        explosionSphere.transform.position = position;

        // nový materiál (aby originál zùstal nezmìnìn)
        Material mat = new Material(explosionMaterial);
        explosionSphere.GetComponent<Renderer>().material = mat;

        // odstraníme collider
        Destroy(explosionSphere.GetComponent<Collider>());

        // zapneme emission
        mat.EnableKeyword("_EMISSION");

        // poèáteèní barva a síla emission
        Color baseEmission = color;// mùžeš zmìnit na jakou chceš barvu
        float emissionStrength = 1f;         // poèáteèní síla
        mat.SetColor("_EmissionColor", baseEmission * emissionStrength);

        // rùst koule
        float currentScale = 0.1f;
        while (currentScale < maxScale)
        {
            currentScale += growSpeed * Time.deltaTime * 6f;
            explosionSphere.transform.localScale = Vector3.one * currentScale;

            // zvýšení síly emission bìhem rùstu
            emissionStrength += Time.deltaTime * 2f; // mìní rychlost zesílení
            mat.SetColor("_EmissionColor", baseEmission * emissionStrength);

            yield return null;
        }

        Destroy(explosionSphere); // zniè efekt
        Destroy(gameObject);
    }
    public void MeleeDead()
    {
        foreach (Collider c in GetComponentsInChildren<Collider>())
            c.enabled = false;

        //animator.SetBool("isDead", true);
        var agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent) agent.enabled = false;

        var animator = GetComponent<Animator>();
        if (animator) animator.enabled = false;
        StartCoroutine(FallOver());
    }
    IEnumerator FallOver()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        Material[] mats = new Material[0];
        PlayEnemySFX(shine);
        if (renderers.Length > 0)
        {
            List<Material> tempMats = new List<Material>();
            foreach (var rend in renderers)
            {
                tempMats.AddRange(rend.materials); // získáme všechny materiály
            }
            mats = tempMats.ToArray();
        }

        float emissionStrength = 0f;

        // aktivujeme Emission pro všechny materiály
        foreach (var mat in mats)
            mat.EnableKeyword("_EMISSION");

        // postupné rozsvícení
        while (emissionStrength < maxEmission)
        {
            emissionStrength += Time.deltaTime * glowSpeed *6f;
            foreach (var mat in mats)
                mat.SetColor("_EmissionColor", Color.green * emissionStrength);
            yield return null;
        }
        maxScale = 6f;
        growSpeed = 20f;
        Vector3 spawnPos = transform.position + new Vector3(0, 1.25f, 0); // zvýší Y o 2 metry
        PlayEnemySFX(explosionPrefab);
        StartCoroutine(ExplosionEffect(spawnPos, Color.green));
    }
    private void SpawnInstantExplosion(Vector3 position, Color color)
    {
        GameObject explosionSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        explosionSphere.transform.position = position;

        Material mat = new Material(explosionMaterial);
        explosionSphere.GetComponent<Renderer>().material = mat;
        Destroy(explosionSphere.GetComponent<Collider>());

        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", color * 5f); // silný svit

        explosionSphere.transform.localScale = Vector3.one * maxScale * 1.25f; // okamžitý rùst

        Destroy(explosionSphere, 0.3f); // zniè sphere po krátké dobì
    }
    public void SetGloryKillGlow(bool enabled)
    {
        if (glowActive == enabled)
            return;

        glowActive = enabled;
        ApplyEmission();

    }
    public void SetPreview(bool enabled)
    {
        if (isPreviewed == enabled) return;
        isPreviewed = enabled;
        ApplyEmission();
    }
    private void ApplyEmission()
    {
        // precedence: preview > gloryActive > none
        Color emission;
        if (isPreviewed)
            emission = previewColor * previewIntensity;
        else if (glowActive)
            emission = glowColor * glowIntensity;
        else
            emission = Color.black;

        if (cachedRenderers == null || mpb == null) return;

        foreach (Renderer r in cachedRenderers)
        {
            if (r == null) continue;
            r.GetPropertyBlock(mpb);
            // použij shader property "_EmissionColor" (standardní)
            mpb.SetColor("_EmissionColor", emission);
            r.SetPropertyBlock(mpb);
        }
    }
}
