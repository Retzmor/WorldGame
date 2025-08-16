using UnityEngine;

public interface IHit 
{
    public void TakeDamage(float damage, Weapon weapon);
    public void Death();
}
