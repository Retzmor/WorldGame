using DG.Tweening;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public abstract class Damageable : MonoBehaviour, IHit
{
    [SerializeField] protected float health = 100;
    protected Rigidbody2D rb;
    protected DamageFlash damageFlash;
    [SerializeField] private bool NeedKnockBack;
    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        damageFlash = GetComponent<DamageFlash>();
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

 

    public void TakeDamage(float damage, WeaponType weapon, Vector2 HitDirection)
    {
        damageFlash?.CallDamageFlash();
        if (NeedKnockBack) { ApplyKnockback(HitDirection); }

        health -= damage;
        if (health <= 0) Death();
    }
}
