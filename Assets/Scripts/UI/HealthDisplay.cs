using UnityEngine;
using UnityEngine.UI;

public class HealthDisplay : MonoBehaviour
{
    [SerializeField] private Image healthFill;
    private PlayerStats playerStats;

    private void Start()
    {
        playerStats = FindObjectOfType<PlayerStats>();
    }

    private void Update()
    {
        float healthPercent = playerStats.CurrentHealth / playerStats.MaxHealth;
        healthFill.fillAmount = healthPercent;
    }
}
