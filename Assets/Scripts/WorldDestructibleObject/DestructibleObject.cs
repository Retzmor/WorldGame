using UnityEngine;

public class DestructibleObject : Damageable
{
    [SerializeField] private Color dominantColor;

    public override void TakeDamage(float damage, WeaponType weaponType, float knockBackValue, Vector2 hitDirection, Vector2 hitPosition)
    {
        base.TakeDamage(damage, weaponType, knockBackValue, hitDirection, hitPosition);
        ParticleManager.Instance.SpawnHitEffect(hitPosition, dominantColor);
    }

 

    protected override void Death()
    {
        base.Death();
        WorldGenerator world = FindAnyObjectByType<WorldGenerator>();
        world.NotifyDecorationDestroyed(this.gameObject);
    }
}
