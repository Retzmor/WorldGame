using System;
using UnityEngine;

public class HealthPlayer : Damageable
{
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private PostProcessingManager ppManager;
    private int currentHealth;
    [SerializeField] private float timeToDisableDamageEffect;
    public event Action<int, int> playerTakeDamage;
    public event Action<int, int> playerTakeHeal;

    private void Awake()
    {
        currentHealth = maxHealth;
        health = maxHealth;
    }

    public override void TakeDamage(float damage, WeaponType weaponType, float knockBackValue, Vector2 HitDirection, Vector2 HitPosition)
    {
        base.TakeDamage(damage, weaponType, knockBackValue, HitDirection, HitPosition);

        currentHealth = Mathf.Clamp(currentHealth - (int)damage, 0, maxHealth);

        playerTakeDamage?.Invoke(currentHealth, maxHealth);
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Mob"))
        {
            Vector2 hitDirection = (collision.transform.position - transform.position).normalized;
            Vector2 hitPosition = collision.GetContact(0).point;
            TakeDamage(34, WeaponType.Sword, 1.5f, -hitDirection, hitPosition);
            ppManager.EnableVignette(true,0.4f, Color.darkRed);
            if (damageFlash == null)
            {
                Debug.Log("No hay damage flash");
            }
            if(ScreenShakeManager.Instance != null)
            ScreenShakeManager.Instance.Shake(1.3f);
        }
    }


}
