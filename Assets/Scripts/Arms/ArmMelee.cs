using UnityEngine;

public class ArmMelee : Arms
{
    public override void Attack(HealthEnemy target)
    {
        Debug.Log("Llamando funsion");
        if (target != null)
        {
            Debug.Log("ArmaMelee");
            target.TakeDamage(damage);
        }
    }
}
