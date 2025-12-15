using UnityEngine;
using UnityEngine.UI;

public class BossManager : MonoBehaviour
{
    public static BossManager Instance;

    public GameObject bossPrefab;

    [Header("Settings")]
    public BossController bossController;
    public int bossHp;
    public int maxBossHp;
    public Slider bossHPSlider;

    [Header("State")]
    public bool isInvulnerable = false;
    public int currentPhase = 1;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        Instance = this;
    }

    public void StartBossFight()
    {
        bossHPSlider.gameObject.SetActive(true);
        maxBossHp = bossHp;
        bossHPSlider.value = bossHp;
        currentPhase = 1;
        bossController.StartPhase(1);
    }

    public void ReciveDamage(int amount)
    {
        if (isInvulnerable)
        {
            Debug.Log("Boss je nesmrtelný! Zniè pilíøe!");
            return;
        }

        bossHp -= amount;
        bossHPSlider.value = bossHp;
        CheckPhaseChange();

        if (bossHp <= 0)
        {
            bossController.StopAllAttacks();
            BossDefeated();
        }
    }

    void CheckPhaseChange()
    {
        float hpPercent = (float)bossHp / maxBossHp;

        if (hpPercent <= 0.70f && hpPercent > 0.30f && currentPhase == 1)
        {
            currentPhase = 2;
            BossPhaseChange(2);
        }
        else if (hpPercent <= 0.30f && currentPhase == 2)
        {
            currentPhase = 3;
            BossPhaseChange(3);
        }
    }

    public void BossPhaseChange(int newPhase)
    {
        Debug.Log("Boss enters Phase " + newPhase);
        bossController.StartPhase(newPhase);
    }

    public void BossDefeated()
    {
        Debug.Log("BOSS DEFEATED! Game Over (Victory)");
    }
}
