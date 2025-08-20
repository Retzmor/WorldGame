using UnityEngine;

public interface IHit 
{
    public void TakeDamage(float damage, WeaponType weapon, Vector2 HitDirection);
    public void Death();
}
