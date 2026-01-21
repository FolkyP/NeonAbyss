using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using static UnityEngine.EventSystems.EventTrigger;


public class Overdrive : MonoBehaviour
{
    public static Overdrive Instance;
    [Header("Overdrive Settings")]
    public KeyCode overKey = KeyCode.F;
    public float overdriveRange = 5f;
    public LayerMask enemyLayer;
    public Camera cam;
    [Header("Screen Targeting")]
    [Tooltip("Max allowed distance from screen center (0 = exact center, ~0.15 = generous)")]
    public float maxScreenDistance = 0.15f;
    [SerializeField] VignetteEffect vignette;


    [Header("Charge Settings")]
    [Tooltip("Kolik damage musí hráè udìlit, aby byl Overdrive na 100%")]
    public float damageToFullCharge = 100f;
    [Tooltip("Pokud true, aktivace ubere pøesnì 100% (reset na 0). Pokud false, sníží o 100% a zbytek zùstane.")]
    public bool consumeOnActivate = true;

    [Header("Debug / State")]
    [Range(0f, 100f)]
    public float currentPercent = 0f;
    public event Action<float> OnChargeChanged;
    public event Action OnFullyCharged;
    private EnemyAI currentPreview;
    public bool enableSoftLock = true;
    public float softLockSpeed = 6f;

    [SerializeField] private GameObject meleeOverdrivePrefab;
    [SerializeField] private GameObject explosionOverdrivePrefab;
    private PlayerMovement playerMovement;
    [Header("Melee dash settings")]
    public float meleeDashDelay = 1f;         // èekání pøed dashingem
    public float meleeDashDistance = 1.6f;    // jak daleko projdeš za enemy (mùžeš nastavit)
    public float meleeDashDuration = 0.12f;

    public float slashSpeed = 5f;   // rychlost seku
    public float slashDuration = 0.1f; // jak dlouho sek trvá

    private bool isSlashing1 = false;
    private bool isSlashing2 = false;
    private float timer1 = 0f;
    private float timer2 = 0f;
    public GameObject katana1;
    public GameObject katana2;
    [SerializeField] private GameObject abilityIcon;
    private Vector3 startLocalPosition1;
    private Vector3 startLocalPosition2;
    private Quaternion startWorldRotation1;
    private Quaternion startWorldRotation2;

    public AudioClip slash;
    public AudioClip readyAb;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // Pokud nechceš, aby Overdrive zmizel pøi zmìnì scény:
            // DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        playerMovement = GetComponent<PlayerMovement>();
        if (playerMovement == null)
            Debug.LogWarning("Overdrive: PlayerMovement not found on same GameObject — Phase dash won't work.");
    }
    private void Start()
    {
        OnChargeChanged?.Invoke(currentPercent);
        float diameter = overdriveRange * 2f;
    }
    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.J) && !isSlashing1)
        //{
        //    isSlashing1 = true;
        //    timer1 = slashDuration;

        //    // uložíme lokální pozici (abychom mohli resetovat vùèi parentu)
        //    startLocalPosition1 = katana1.transform.localPosition;

        //    // uložíme svìtovou rotaci katany v okamžiku stisku
        //    startWorldRotation1 = katana1.transform.rotation;
        //}
        //if (Input.GetKeyDown(KeyCode.K) && !isSlashing2)
        //{
        //    isSlashing2 = true;
        //    timer2 = slashDuration;

        //    startLocalPosition2 = katana2.transform.localPosition;
        //    startWorldRotation2 = katana2.transform.rotation;
        //}
        if (isSlashing1)
        {
            Transform yawOrientation = PlayerCam.Instance.orientation;

            float yWeight = 0.65f;
            Vector3 worldSlashDir =
                (-yawOrientation.right - Vector3.up * yWeight).normalized;

            katana1.transform.Translate(
                worldSlashDir * slashSpeed * Time.deltaTime,
                Space.World
            );

            timer1 -= Time.deltaTime;
            if (timer1 <= 0f)
            {
                isSlashing1 = false;
                katana1.transform.localPosition = startLocalPosition1;
            }
        }
        if (isSlashing2)
        {
            Vector3 worldSlashDir2 = Vector3.down; 

            katana2.transform.Translate(
                worldSlashDir2 * slashSpeed * Time.deltaTime,
                Space.World
            );

            timer2 -= Time.deltaTime;
            if (timer2 <= 0f)
            {
                isSlashing2 = false;
                katana2.transform.localPosition = startLocalPosition2;
            }
        }



        if (currentPercent >= 100f)
            UpdatePreview();
        else
            ClearPreview();

        if (enableSoftLock && currentPreview != null)
            SoftLockTowards(currentPreview.transform);

        if (Input.GetKeyDown(overKey))
            TryActivateOverdrive();
    }

    private void TryActivateOverdrive()
    {
        if (currentPercent < 100f)
        {
            Debug.Log($"Overdrive: not ready ({currentPercent:0.##}%)");
            return;
        }

        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            overdriveRange,
            enemyLayer,
            QueryTriggerInteraction.Ignore
        );

        if (hits.Length == 0)
        {
            Debug.Log("Overdrive: no enemies in range");
            return;
        }

        EnemyAI target = SelectByScreenCenter(hits);

        if (target == null)
        {
            Debug.Log("Overdrive: enemies in range, none under crosshair");
            return;
        }

        Debug.Log($"Overdrive activated on: {target.name}");

        // Start the Overdrive sequence (for melee it will run the player's phase-dash)
        StartCoroutine(ExecuteOverdriveOn(target));

        // consume charge
        if (consumeOnActivate)
            SetCharge(0f);
        else
            AddPercent(-100f);
    }

    private IEnumerator ExecuteOverdriveOn(EnemyAI target)
    {
        GameObject overdriveEffect = null;
        GameSettings.Instance.isOverDriveActive = true;
        vignette.SetIntensityFromSpeed(0.1f, new Color(1, 0, 1));
        WeaponManager wm = FindObjectOfType<WeaponManager>();
        if (wm != null)
            wm.SetAllWeaponsActive(false);
        abilityIcon.SetActive(false);
        AudioSettings.Instance.PlaySFX(slash);
        switch (target.enemyType)
        {
            case EnemyAI.EnemyType.Melee:
                // místo okamžitého instancování -> zavolej coroutine v player movement
                if (playerMovement != null)
                {
                    
                    Vector3 spawnPos = target.transform.position;
                    spawnPos.y = 0f;
                    Vector3 dirToPlayer = transform.position - spawnPos;
                    dirToPlayer.y = 0f;
                    Quaternion rot = Quaternion.LookRotation(dirToPlayer) * Quaternion.Euler(0f, 180f, 0f);
                    overdriveEffect = Instantiate(meleeOverdrivePrefab, spawnPos, rot);
                    Destroy(target.gameObject);
                    StartCoroutine(playerMovement.PhaseDashThroughEnemy(
    overdriveEffect,
    meleeDashDelay,
    meleeDashDistance,
    meleeDashDuration,
    meleeOverdrivePrefab,
    rot,
    StartSlash1   
));
                    if (overdriveEffect != null)
                    {
                        Transform fxChild = FindChildRecursive(overdriveEffect.transform, "RobotikFirstHalf");

                        if (fxChild != null)
                        {
                            // doleva (-X) a dolù (-Y) v lokálním prostoru
                            Vector3 localOffset = new Vector3(-0.4f, -0.3f, 0f);

                            StartCoroutine(
                                MoveEffectLocal(
                                    fxChild,
                                    localOffset,
                                    0.35f
                                )
                            );
                        }
                    }

                }

                break;

            case EnemyAI.EnemyType.Explosion:
                if (playerMovement != null)
                {

                    Vector3 spawnPos = target.transform.position;
                    spawnPos.y = 0f;
                    Vector3 dirToPlayer = transform.position - spawnPos;
                    dirToPlayer.y = 0f;
                    Quaternion rot = Quaternion.LookRotation(dirToPlayer) * Quaternion.Euler(0f, 180f, 0f);
                    overdriveEffect = Instantiate(explosionOverdrivePrefab, spawnPos, rot);
                    Destroy(target.gameObject);
                    StartCoroutine(playerMovement.PhaseDashThroughEnemy(
    overdriveEffect,
    meleeDashDelay,
    meleeDashDistance,
    meleeDashDuration,
    explosionOverdrivePrefab,
    rot,
    StartSlash2
));
                    if (overdriveEffect != null)
                    {
                        Transform left = FindChildRecursive(overdriveEffect.transform, "Left");
                        Transform right = FindChildRecursive(overdriveEffect.transform, "Right");

                        StartCoroutine(
    MoveAndRotateEffectLocal(
        left,
        new Vector3(-0.4f, -0.3f, 0f),
        new Vector3(0f, 0f, 15f),
        0.35f
    )
);

                        StartCoroutine(
                            MoveAndRotateEffectLocal(
                                right,
                                new Vector3(+0.4f, -0.3f, 0f),
                                new Vector3(0f, 0f, -15f),
                                0.35f
                            )
                        );

                    }


                }
                break;

            default:
                Debug.LogWarning("Unknown enemy type: " + target.enemyType);
                break;
        }
        yield return new WaitForSeconds(0.3f);
        if (wm != null)
            wm.SetAllWeaponsActive(true);

        if(overdriveEffect != null)
        Destroy(overdriveEffect.gameObject);
        GameSettings.Instance.isOverDriveActive = false;
        StartCoroutine(OverdriveEffect(speedMultiplier: 1.625f, duration: 6f));
    }
    private void StartSlash2()
    {
        isSlashing2 = true;
        timer2 = slashDuration;

        startLocalPosition2 = katana2.transform.localPosition;
        startWorldRotation2 = katana2.transform.rotation;
    }
    private void StartSlash1()
{
    isSlashing1 = true;
    timer1 = slashDuration;

    startLocalPosition1 = katana1.transform.localPosition;
    startWorldRotation1 = katana1.transform.rotation;
}

    private EnemyAI SelectByScreenCenter(Collider[] hits)
    {
        EnemyAI bestEnemy = null;
        float bestScore = float.MaxValue;

        Vector2 screenCenter = new Vector2(0.5f, 0.5f);

        foreach (Collider col in hits)
        {
            EnemyAI enemy = col.GetComponentInParent<EnemyAI>();
            if (enemy == null || enemy.isDead || !enemy.IsGloryKillAvailable)
                continue;

            Renderer rend = enemy.GetComponentInChildren<Renderer>();
            Vector3 worldPos = rend != null
                ? rend.bounds.center
                : enemy.transform.position;

            Vector3 viewportPos = cam.WorldToViewportPoint(worldPos);

            // Reject enemies behind camera or off-screen
            if (viewportPos.z <= 0f)
                continue;

            Vector2 viewport2D = new Vector2(viewportPos.x, viewportPos.y);
            float screenDistance = Vector2.Distance(viewport2D, screenCenter);

            float allowedScreenDistance =
    enemy.enemyType == EnemyAI.EnemyType.Explosion
        ? maxScreenDistance * 1.5f
        : maxScreenDistance;

            if (screenDistance > allowedScreenDistance)
                continue;


            // Prefer closer enemies if screen distance is similar
            float worldDistance = Vector3.Distance(transform.position, enemy.transform.position);

            // Combined score (screen alignment weighted more heavily)
            float score = screenDistance * 10f + worldDistance * 0.1f;

            if (score < bestScore)
            {
                bestScore = score;
                bestEnemy = enemy;
            }
        }

        return bestEnemy;
    }
    public void AddChargeFromDamage(float damage)
    {
        if (damageToFullCharge <= 0f) return;

        float percentToAdd = (damage / damageToFullCharge) * 100f;
        AddPercent(percentToAdd);
        
    }


    public void AddPercent(float deltaPercent)
    {
        float prev = currentPercent;
        currentPercent = Mathf.Clamp(currentPercent + deltaPercent, 0f, 100f);

        if (!Mathf.Approximately(prev, currentPercent))
        {
            Debug.Log($"Overdrive.AddPercent: {prev:0.##}% -> {currentPercent:0.##}%");
            OnChargeChanged?.Invoke(currentPercent);
        }

        if (currentPercent >= 100f && prev < 100f)
        {
            Debug.Log("Overdrive: fully charged!");
            AudioSettings.Instance.PlaySFXAbility(readyAb);
            OnFullyCharged?.Invoke();
        }
    }

    public void SetCharge(float percent)
    {
        float prev = currentPercent;
        currentPercent = Mathf.Clamp(percent, 0f, 100f);
        Debug.Log($"Overdrive.SetCharge: {prev:0.##}% -> {currentPercent:0.##}%");
        OnChargeChanged?.Invoke(currentPercent);

        if (currentPercent >= 100f && prev < 100f)
            OnFullyCharged?.Invoke();
    }
    private void UpdatePreview()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, overdriveRange, enemyLayer, QueryTriggerInteraction.Ignore);
        EnemyAI t = SelectByScreenCenter(hits);

        if (t != null && (t.enemyType == EnemyAI.EnemyType.Melee || t.enemyType == EnemyAI.EnemyType.Explosion))
        {
            if (currentPreview != t)
            {
                ClearPreview();
                currentPreview = t;
                currentPreview.SetPreview(true); // musíš mít metodu v EnemyAI
                abilityIcon.SetActive(true);
            }
        }
        else
        {
            ClearPreview();
            abilityIcon.SetActive(false);

        }
       
    }
    private void ClearPreview()
    {
        if (currentPreview != null)
        {
            currentPreview.SetPreview(false);
            currentPreview = null;
        }
    }

    private void SoftLockTowards(Transform target)
    {
        Vector3 dir = target.position - transform.position; dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;
        Quaternion desired = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, desired, Time.deltaTime * softLockSpeed);
    }

    // --- vlož do tøídy Overdrive ---

    // Coroutine, která provede pohyb katany souèasnì s dash
    Transform FindChildRecursive(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
                return child;

            Transform found = FindChildRecursive(child, name);
            if (found != null)
                return found;
        }
        return null;
    }
    IEnumerator MoveEffectLocal(
    Transform target,
    Vector3 localOffset,
    float duration
)
    {
        if (target == null) yield break;
        Vector3 start = target.localPosition;
        Vector3 end = start + localOffset;

        float t = 0f;
        yield return new WaitForSeconds(0.2f);
        while (t < duration)
        {
            if (target == null) yield break;
            t += Time.deltaTime;
            float k = Mathf.SmoothStep(0f, 1f, t / duration);
            target.localPosition = Vector3.Lerp(start, end, k);
            yield return null;
        }

        if (target != null) target.localPosition = end;
    }


    private IEnumerator MoveAndRotateEffectLocal(
    Transform target,
    Vector3 localOffset,
    Vector3 localRotationOffset,
    float duration
)
    {
        if (target == null) yield break;
        Vector3 startPos = target.localPosition;
        Quaternion startRot = target.localRotation;

        Vector3 endPos = startPos + localOffset;
        Quaternion endRot = startRot * Quaternion.Euler(localRotationOffset);

        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (target == null) yield break;
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            t = Mathf.SmoothStep(0f, 1f, t);

            target.localPosition = Vector3.Lerp(startPos, endPos, t);
            target.localRotation = Quaternion.Slerp(startRot, endRot, t);

            yield return null;
        }

        target.localPosition = endPos;
        target.localRotation = endRot;
    }
    private IEnumerator OverdriveEffect(float speedMultiplier, float duration)
    {
        if (playerMovement == null) yield break;

        playerMovement.CacheBaseSpeeds();
        playerMovement.ApplySpeedMultiplier(speedMultiplier);

        vignette.SetIntensityFromSpeed(0.13f, new Color(0, 1, 1));

        // Heal volá UpdateUI, ale díky isOverDriveActive = true 
        // se v PlayerLife nic nepøepíše.
        PlayerLife.Instance.Heal(50);
        PlayerLife.Instance.AddShield(50);

        yield return new WaitForSeconds(duration);

        playerMovement.RestoreBaseSpeeds();

        // 2. Vynulujeme azurovou vignette
        vignette.SetIntensityFromSpeed(0f, new Color(0, 1, 1));

        // 3. Pøinutíme PlayerLife si to zkontrolovat (kdyby byl hráè po buffu stále zranìný)
        PlayerLife.Instance.Heal(0);
    }

}
