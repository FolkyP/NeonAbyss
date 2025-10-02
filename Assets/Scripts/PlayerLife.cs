using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerLife : MonoBehaviour
{
    [Header("Stats")]
    public int maxHealth = 100;
    public int maxShield = 100;

    [Header("Current Values")]
    public int currentHealth;
    public int currentShield;

    [Header("UI")]
    public Slider hpSlider;
    public Text hpText; // or TMP_Text if using TextMeshPro
    public Slider shieldSlider;
    public Text shieldText; // or TMP_Text if using TextMeshPro

    private void Start()
    {
        currentHealth = maxHealth;
        currentShield = maxShield;

        // Setup sliders
        hpSlider.maxValue = maxHealth;
        shieldSlider.maxValue = maxShield;

        UpdateUI();
    }

    public void TakeDamage(int damage)
    {
        int shieldDamage = Mathf.Min(currentShield, damage);
        currentShield -= shieldDamage;
        damage -= shieldDamage;

        if (damage > 0)
            currentHealth -= damage;

        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        currentShield = Mathf.Clamp(currentShield, 0, maxShield);

        UpdateUI();

        if (IsDead())
        {
            Debug.Log("Player Died!");
            // Handle death (respawn/game over)
        }
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        UpdateUI();
    }

    public void AddShield(int amount)
    {
        currentShield = Mathf.Clamp(currentShield + amount, 0, maxShield);
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (hpSlider != null) hpSlider.value = currentHealth;
        if (shieldSlider != null) shieldSlider.value = currentShield;

        if (hpText != null) hpText.text = currentHealth + "/" + maxHealth;
        if (shieldText != null) shieldText.text = currentShield + "/" + maxShield;
    }

    public bool IsDead()
    {
        return currentHealth <= 0;
    }
}
