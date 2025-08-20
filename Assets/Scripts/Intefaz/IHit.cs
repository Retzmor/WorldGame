using UnityEngine;

public interface IHit 
{
    public void TakeDamage(float damage, WeaponType weapon);
    public void Death();
}
