using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private Image healthFill;

    private void Start()
    {
        PlayerStats stats = FindObjectOfType<PlayerStats>();
        if (stats != null)
        {
            stats.OnHealthChanged += UpdateHealthBar;
            UpdateHealthBar((float)stats.CurrentHealth / stats.MaxHealth); // Initial update
        }
    }

    private void UpdateHealthBar(float fillPercent)
    {
        healthFill.fillAmount = fillPercent;
    }
}
