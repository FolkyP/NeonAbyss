using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BossManager : MonoBehaviour
{
    public static BossManager Instance;
    [Header("Boss Shield")]
    public GameObject shieldObject;
    public List<GameObject> PanelsForShield;
    public float panelMoveSpeed = 2f;

    public MeshRenderer shieldRenderer;  // pøetáhni sem renderer té sféry
    [Range(0f, 1f)] public float baseOpacity = 0.15f;
    [Range(0f, 1f)] public float hitOpacity = 0.6f;
    public float fadeSpeed = 4f;

    [HideInInspector]
    public bool canAttack = false; // default false

    public List<GameObject> BossList;
    public Material newMaterial1;
    public Material newMaterial2;
    public Material newMaterial3;
    public GameObject bossPrefab;
    [Header("Phase Ring Attack")]
    public GameObject ringPrefab; // assign prefab with RingAttack.cs
    public bool spawnRingOnPhaseChange = true;

    [Header("Settings")]
    public BossController bossController;
    public int bossHp;
    public int maxBossHp;
    public Slider bossHPSlider;
    public TMP_Text hp;
    public TMP_Text nameText;
    [Header("UI Animation")]
    public float hpBarLerpSpeed = 6f;

    private float targetHp;
    [Header("State")]
    public bool isInvulnerable = false;
    public int currentPhase = 1;

    [Header("HP Bar Sprites")]
    public Image hpFillImage;
    private Sprite currentPhaseSprite;

    public Sprite phase1Sprite;
    public Sprite phase2Sprite;
    public Sprite phase3Sprite;
    [Header("HP Bar Glow (RawImages)")]
    public RawImage phase1Glow;
    public RawImage phase2Glow;
    public RawImage phase3Glow;

    public Color damageFlashColor = Color.white;
    public float flashDuration = 0.015f;

    private Coroutine flashRoutine;
    private Color phaseColor;

    [Header("HP Text Colors")]
    public Color phase1TextColor = new Color(0f, 0.85f, 0.8f); // tyrkysová
    public Color phase2TextColor = new Color(0.7f, 0.3f, 1f);  // fialová
    public Color phase3TextColor = Color.red;
    [Header("Shield Crystals")]
    public int totalCrystals = 3;         // kolik jich ve štítu je
    private int destroyedCrystals = 0;

    public TMP_Text totalCrystext;

    public int baseBossHp = 4000;

    [Header("Boss Model Flash")]
    public Color modelFlashColor = Color.white;
    public float modelFlashDuration = 0.1f;
    private Coroutine modelFlashRoutine;
    public List<GameObject> bossBlik;
    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        Instance = this;
    }
    private void Start()
    {
        totalCrystext.text = "0"+ "/"+ "3";
    }
    void UpdateHpText()
    {
        float percent = (float)bossHp / maxBossHp * 100f;
        hp.text = $"{Mathf.CeilToInt(percent)}%";
    }

    public void StartBossFight()
    {
        Vector3 leftSpawn = new Vector3(-37, 0, 105); // Change to your coordinates
        Vector3 rightSpawn = new Vector3(37, 0, 33); if (AudioSettings.Instance != null)
        {
            AudioSettings.Instance.CrossfadeTo(
                AudioSettings.Instance.bossMusic,
                0f,
                true
            );
            AudioSettings.Instance.MuteMusic(true);

        }
        if (CrystalManager.Instance != null)
        {
            CrystalManager.Instance.StartBuffSpawners(leftSpawn, rightSpawn);
        }
        bossPrefab.GetComponent<MeshRenderer>().materials[1] = newMaterial1;
        float bhp = GameSettings.Instance.bossHealthMultiplier;
        bossHp = Mathf.RoundToInt(baseBossHp * bhp);
        maxBossHp = bossHp;
        bossHPSlider.gameObject.SetActive(true);

        bossHPSlider.minValue = 0;
        bossHPSlider.maxValue = maxBossHp;
        bossHPSlider.value = maxBossHp;
        targetHp = maxBossHp;

        currentPhase = 1;

        SetHpBarSprite(phase1Sprite);
        SetPhaseGlow(1);
        SetHpTextColor(1);

        UpdateHpText();

        if (bossController != null)
            bossController.StartPhase(1);
    }



    private void Update()
    {
        if (!bossHPSlider.gameObject.activeSelf) return;

        bossHPSlider.value = Mathf.Lerp(
            bossHPSlider.value,
            targetHp,
            Time.deltaTime * hpBarLerpSpeed
        );
    }


    public void ReciveDamage(int amount)
    {
        if (isInvulnerable)
        {
            StartCoroutine(ShieldHitFlash());
            return;
        } 

        bossHp = Mathf.Max(0, bossHp - amount);
        targetHp = bossHp;
        CheckPhaseChange();
        UpdateHpText();

        

        // restart flash (JEDINÝ povolený)
        if (flashRoutine != null)
            StopCoroutine(flashRoutine);

        flashRoutine = StartCoroutine(FlashHpBar());

        

        if (bossHp <= 0)
        {
            bossController.StopAllAttacks();
            BossDefeated();
        }
    }
    
    private IEnumerator ShieldHitFlash()
    {
        // Zvýšení opacity
        SetShieldOpacity(hitOpacity);

        // Krátká pauza (štít "zabliká")
        yield return new WaitForSeconds(0.1f);

        // Postupné vracení zpìt na pùvodní hodnotu
        float t = 0f;
        float start = hitOpacity;

        while (t < 1f)
        {
            t += Time.deltaTime * fadeSpeed;
            float newOpacity = Mathf.Lerp(start, baseOpacity, t);
            SetShieldOpacity(newOpacity);
            yield return null;
        }

        SetShieldOpacity(baseOpacity);
    }
    private void SetShieldOpacity(float value)
    {
        if (shieldRenderer == null) return;

        Color c = shieldRenderer.material.color;
        c.a = value;
        shieldRenderer.material.color = c;
    }

    void ApplyMaterialToBossList(int phase)
    {
        foreach (GameObject boss in BossList)
        {
            if (boss == null)
                continue;

            MeshRenderer r = boss.GetComponent<MeshRenderer>();
            if (r == null)
                continue;

            Material[] mats = r.materials;

            switch (phase)
            {
                case 1:
                    mats[0] = newMaterial1;
                    break;
                case 2:
                    mats[0] = newMaterial2;
                    break;
                case 3:
                    mats[0] = newMaterial3;
                    break;
            }

            r.materials = mats; // dùležité zapsat zpìt
        }
    }

    void SetPhaseGlow(int phase)
    {
        if (phase1Glow != null) phase1Glow.gameObject.SetActive(false);
        if (phase2Glow != null) phase2Glow.gameObject.SetActive(false);
        if (phase3Glow != null) phase3Glow.gameObject.SetActive(false);
        var r = bossPrefab.GetComponent<Renderer>();
        Material[] mats = r.materials;
        switch (phase)
        {
            case 1:
                if (phase1Glow != null) phase1Glow.gameObject.SetActive(true);
                break;
            case 2:
                if (phase2Glow != null) phase2Glow.gameObject.SetActive(true);
                
                mats[1] = newMaterial2; // nebo newMaterial3
                r.materials = mats;
                
                break;
            case 3:
                if (phase3Glow != null) phase3Glow.gameObject.SetActive(true);
                
                mats[1] = newMaterial3; // nebo newMaterial3
                r.materials = mats;

                break;
        }
        ApplyMaterialToBossList(phase);
    }

    void SetHpTextColor(int phase)
    {
        switch (phase)
        {
            case 1:
                hp.color = phase1TextColor;
                nameText.color = phase1TextColor;
                break;
            case 2:
                hp.color = phase2TextColor;
                nameText.color = phase2TextColor;
                break;
            case 3:
                hp.color = phase3TextColor;
                nameText.color = phase3TextColor;
                break;
        }
    }

    void CheckPhaseChange()
    {
        float hpPercent = (float)bossHp / maxBossHp;

        if (hpPercent <= 0.70f && hpPercent > 0.30f && currentPhase == 1)
        {
            currentPhase = 2;
            SetHpBarSprite(phase2Sprite);
            SetPhaseGlow(2);
            SetHpTextColor(2);
            BossPhaseChange(2);

            // ensure UI shows new sprite and value immediately
            bossHPSlider.value = bossHp;
            targetHp = bossHp;
            SpawnManager.Instance.isPhaseForSpawn = true;
            
        }
        else if (hpPercent <= 0.30f && currentPhase == 2)
        {
            EnterPhase3();
            currentPhase = 3;
            SetHpBarSprite(phase3Sprite);
            SetPhaseGlow(3);
            SetHpTextColor(3);
            BossPhaseChange(3);
            totalCrystext.gameObject.SetActive(true);
            // ensure UI shows new sprite and value immediately
            bossHPSlider.value = bossHp;
            targetHp = bossHp;
        }



    }


    void SetHpBarSprite(Sprite sprite)
    {
        currentPhaseSprite = sprite;

        if (hpFillImage != null)
            hpFillImage.sprite = currentPhaseSprite;
    }




    public void BossPhaseChange(int newPhase)
    {
        Debug.Log("Boss enters Phase " + newPhase);
        bossController.StartPhase(newPhase);

        // spawn ring immediately on phase change (optional)
        if (spawnRingOnPhaseChange && ringPrefab != null && bossController != null)
        {
            Vector3 spawnPos = bossController.transform.position;
            spawnPos.y = 1f;
            GameObject r = Instantiate(ringPrefab, spawnPos, Quaternion.identity);
            // optionally configure runtime parameters:
            RingAttack ra = r.GetComponent<RingAttack>();
            if (ra != null)
            {
                // example tweaks per phase:
                if (newPhase == 2)
                {
                    ra.expandSpeed = 30f;
                    ra.maxRadius = 100f;
                    ra.damage = 50;
                    ra.thickness = 1f;
                }
                else if (newPhase == 3)
                {
                    ra.expandSpeed = 30f;
                    ra.maxRadius = 100f;
                    ra.damage = 75;
                    ra.thickness = 1f;
                }
            }
        }
    }


    public void BossDefeated()
    {
        //exploze a win
        hp.gameObject.SetActive(false);
        bossHPSlider.gameObject.SetActive(false);
        SpawnManager.Instance.StopAndKillAll();
        SpawnManager.Instance.isPhaseForSpawn = false;
        Destroy(bossPrefab);
        GameSettings.Instance.Win();
    }
    IEnumerator FlashHpBar()
    {
        if (hpFillImage == null) yield break;

        Color originalColor = hpFillImage.color;

        hpFillImage.color = damageFlashColor;
        yield return new WaitForSeconds(flashDuration);

        hpFillImage.color = originalColor;
        hpFillImage.sprite = currentPhaseSprite;

        flashRoutine = null;
    }

    void EnterPhase3()
    {
        PlayerCam.Instance.Shake(2f, .4f);
        isInvulnerable = true;

        if (shieldObject != null)
            shieldObject.SetActive(true);

        Debug.Log("Shield ACTIVATED - Boss is invulnerable.");
        StartCoroutine(MovePanelsUp());
        destroyedCrystals = 0;
        totalCrystals = PanelsForShield.Count;
    }
    public void DisableShield()
    {
        isInvulnerable = false;

        if (shieldObject != null)
            shieldObject.SetActive(false);

        Debug.Log("Shield BROKEN - Boss is vulnerable!");
    }
    IEnumerator MovePanelsUp()
    {
        float targetY = 2f;

        foreach (GameObject panel in PanelsForShield)
        {
            if (panel == null) continue;

            // Spustí individuální animaci pro každý objekt
            StartCoroutine(MoveOnePanel(panel, targetY));
        }

        yield break;
    }

    IEnumerator MoveOnePanel(GameObject panel, float targetY)
    {
        Vector3 startPos = panel.transform.position;
        Vector3 targetPos = new Vector3(startPos.x, targetY, startPos.z);
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * panelMoveSpeed;
            panel.transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        panel.transform.position = targetPos;
    }
    public void CrystalDestroyed(GameObject panel = null)
    {
        destroyedCrystals++;

        Debug.Log("Crystal Destroyed! (" + destroyedCrystals + "/" + totalCrystals + ")");
        totalCrystext.text = destroyedCrystals + "/" + totalCrystals;
        if (panel != null)
        {
            StartCoroutine(HandlePanelAfterCrystalDestroyed(panel));
        }
        // Dokud nejsou všechny pryè, zùstává štít
        if (destroyedCrystals < totalCrystals) return;

        // Jakmile padnou všechny:
        DisableShield();
    }
    IEnumerator HandlePanelAfterCrystalDestroyed(GameObject panel)
    {
       
            Vector3 start = panel.transform.localPosition;
            Vector3 end = start + new Vector3(0, -10f, 0);
            float dur = 0.7f;
            float t = 0f;
            while (t < dur)
            {
                t += Time.deltaTime;
                panel.transform.localPosition = Vector3.Lerp(start, end, t / dur);
                yield return null;
            }
            Destroy(panel);
        
    }

}

