using UnityEngine;

public class ArmsRange : Arms
{
    public override void Attack(HealthEnemy target)
    {
        if (target != null)
        {
            target.TakeDamage(damage);
        }
    }
}
