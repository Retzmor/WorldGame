using UnityEngine;

public class ContainerHearts : MonoBehaviour
{
    [SerializeField] HeartUI[] hearts;
    [SerializeField] HealthPlayer healthPlayer;

    private void OnEnable()
    {
        healthPlayer.playerTakeDamage += ActiveHearts; 
        healthPlayer.playerTakeHeal += ActiveHearts;
    }

    private void OnDisable()
    {
        healthPlayer.playerTakeDamage -= ActiveHearts;
        healthPlayer.playerTakeHeal -= ActiveHearts;
    }
    private void Start()
    {
        ActiveHearts(healthPlayer.GetCurrentHealth(), healthPlayer.GetMaxHealth());
    }

    public void ActiveHearts(int currentHealth, int maxHealth)
    {
        int totalHearts = hearts.Length;

        float healthPerHeart = (float)maxHealth / totalHearts;

        for (int i = 0; i < totalHearts; i++)
        {
            float minValue = i * healthPerHeart;
            float maxValue = (i + 1) * healthPerHeart;

            if (currentHealth > minValue)
            {
                hearts[i].ActiveHeart();
            }
            else
            {
                hearts[i].DesactiveHeart();
            }
        }

    }
}
