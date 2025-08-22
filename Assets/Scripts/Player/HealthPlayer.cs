using System;
using UnityEngine;

public class HealthPlayer : Damageable
{
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;

    public event Action<int, int> playerTakeDamage;
    public event Action<int, int> playerTakeHeal;

    private void Awake()
    {
        currentHealth = maxHealth;
        health = maxHealth;
    }

    public override void TakeDamage(float damage, WeaponType weapon, Vector2 hitDir)
    {
        base.TakeDamage(damage, weapon, hitDir); // esto mantiene knockback + flash

        currentHealth = Mathf.Clamp(currentHealth - (int)damage, 0, maxHealth);

        playerTakeDamage?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Death();
        }
    }

    protected override void Death()
    {
        Debug.Log("Jugador muerto");
    }

    public void HealHealth(int heal)
    {
        currentHealth = Mathf.Clamp(currentHealth + heal, 0, maxHealth);
        health = currentHealth;
        playerTakeHeal?.Invoke(currentHealth, maxHealth);
    }

    public int GetMaxHealth() => maxHealth;
    public int GetCurrentHealth() => currentHealth;
}
