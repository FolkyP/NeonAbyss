using System;
using UnityEngine;
using UnityEngine.UI;

public class PlayerLife : MonoBehaviour
{
    [Header("Base Max Stats")]
    public int maxHealth = 100;     // normal max
    public int maxShield = 100;     // normal max

    [Header("Overheal Limits")]
    public int maxOverHealth = 200; // overheal cap
    public int maxOverShield = 200; // overshield cap

    [Header("Current Values")]
    public int currentHealth;
    public int currentShield;

    [Header("UI")]
    public Slider hpSlider;
    public Text hpText;
    public Slider shieldSlider;
    public Text shieldText;

    public AudioClip damageSound;

    public static PlayerLife Instance;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        currentHealth = maxHealth;
        currentShield = maxShield;

        // IMPORTANT FIX:
        hpSlider.maxValue = maxHealth;
        shieldSlider.maxValue = maxShield;

        UpdateUI();
    }

    private void Update()
    {
        if (transform.position.y < -5f && !IsDead())
        {
            TakeDamage(maxHealth * 10);
        }
    }

    public void TakeDamage(int damage)
    {
        AudioSettings.Instance.PlaySFX(damageSound);

        // Shield first
        int shieldDamage = Mathf.Min(currentShield, damage);
        currentShield -= shieldDamage;
        damage -= shieldDamage;

        // Health next
        if (damage > 0)
            currentHealth -= damage;

        // Clamp
        currentHealth = Mathf.Clamp(currentHealth, 0, maxOverHealth);
        currentShield = Mathf.Clamp(currentShield, 0, maxOverShield);

        UpdateUI();

        if (IsDead())
        {
            Debug.Log("Player Died!");
            GameSettings.Instance.isGameOn = false;
            GameSettings.Instance.isGameStopped = true;
            GameSettings.Instance.Death();
            Time.timeScale = 0f;
        }
    }
   
    public void Heal(int amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxOverHealth);
        UpdateUI();
    }

    public void AddShield(int amount)
    {
        currentShield = Mathf.Clamp(currentShield + amount, 0, maxOverShield);
        UpdateUI();
    }

    private void UpdateUI()
    {
        // Slider always shows MAX as 100, even if overhealed
        if (hpSlider != null)
            hpSlider.value = Mathf.Min(currentHealth, maxHealth);

        if (shieldSlider != null)
            shieldSlider.value = Mathf.Min(currentShield, maxShield);

        // Text shows overheal
        if (hpText != null)
        {
            
                hpText.text = $"{currentHealth}/{maxHealth}";
        }

        if (shieldText != null)
        {
            
                shieldText.text = $"{currentShield}/{maxShield}";
        }
    }

    public bool IsDead()
    {
        return currentHealth <= 0;
    }
}
