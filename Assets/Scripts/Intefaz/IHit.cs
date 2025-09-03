using UnityEngine;

public interface IHit 
{
    public void TakeDamage(float damage, WeaponType weaponType, float knockBackValue , Vector2 HitDirection);
   // public void Death();
}
