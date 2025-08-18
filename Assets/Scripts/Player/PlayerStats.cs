using UnityEngine;
using System;

public class PlayerStats : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 100;

    [Header("Defense")]
    [SerializeField, Range(0f,1f)] private float blockDamageMultiplier = 0.4f;
    public bool IsBlocking { get; private set; }
    public void SetBlocking(bool value, float multiplier = 0.4f) { IsBlocking = value; blockDamageMultiplier = multiplier; }
    private int currentHealth;

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;

    // Event to notify UI
    public event Action<float> OnHealthChanged;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        OnHealthChanged?.Invoke((float)currentHealth / maxHealth);
    }

    public void TakeDamage(int amount)
    {
        int final = IsBlocking ? Mathf.RoundToInt(amount * blockDamageMultiplier) : amount;
        currentHealth = Mathf.Clamp(currentHealth - final, 0, maxHealth);
        OnHealthChanged?.Invoke((float)currentHealth / maxHealth);
    }
}
