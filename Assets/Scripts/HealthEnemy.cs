using UnityEngine;

public class HealthEnemy : Enemies
{
    float health = 100;
    float currentHealth;

    private void Start()
    {
        currentHealth = health;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
    }
}
