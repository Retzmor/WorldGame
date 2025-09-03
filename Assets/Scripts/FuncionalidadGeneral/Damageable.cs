using DG.Tweening;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public abstract class Damageable : MonoBehaviour, IHit
{
    [SerializeField] protected float health = 100;
    protected Rigidbody2D rb;
    protected DamageFlash damageFlash;
    [SerializeField] private bool NeedKnockBack;

    public WeaponType weaponNeeded;


    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        damageFlash = GetComponent<DamageFlash>();
    }

   

    protected virtual void ApplyKnockback(Vector2 hitDir, float forceKnockBack )
    {
        float knockbackDistance = forceKnockBack;
        float knockbackDuration = 0.15f;

        transform.DOKill();
        transform.DOMove((Vector2)transform.position + hitDir * knockbackDistance, knockbackDuration)
                 .SetEase(Ease.OutQuad);
    }

    protected abstract void Death();

 

    public virtual void TakeDamage(float damage, WeaponType weaponType, float knockBackValue,  Vector2 HitDirection)
    {
        damageFlash?.CallDamageFlash();
        if (NeedKnockBack) { ApplyKnockback(HitDirection, knockBackValue); }

        if(weaponNeeded == weaponType)
        {
            health -= damage * 2;
        }
        else
        {
            health -= damage;
        }

        
        if (health <= 0) Death();
    }
}
