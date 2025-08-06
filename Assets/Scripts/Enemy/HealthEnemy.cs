using UnityEngine;

public class HealthEnemy : Enemies
{
    public float health = 100;
    public float currentHealth;

    private void Start()
    {
        currentHealth = health;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if(currentHealth < 0 )
        {
            Death();
        }
    }

    public void Death()
    {
        gameObject.SetActive(false);
    }
}
