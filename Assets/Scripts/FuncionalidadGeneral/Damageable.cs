using DG.Tweening;
using UnityEngine;

public abstract class Damageable : MonoBehaviour, IHit
{
    [SerializeField] protected float health = 100;
    protected Rigidbody2D rb;
    protected DamageFlash damageFlash;

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        damageFlash = GetComponent<DamageFlash>();
    }

    public virtual void TakeDamage(float damage, WeaponType weapon, Vector2 hitDir)
    {
        damageFlash?.CallDamageFlash();
        ApplyKnockback(hitDir);
        health -= damage;
        if (health <= 0) Death();
    }

    protected virtual void ApplyKnockback(Vector2 hitDir)
    {
        float knockbackDistance = 0.5f;
        float knockbackDuration = 0.15f;

        transform.DOKill();
        transform.DOMove((Vector2)transform.position + hitDir * knockbackDistance, knockbackDuration)
                 .SetEase(Ease.OutQuad);
    }

    protected abstract void Death();
}
