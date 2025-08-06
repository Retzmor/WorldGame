using UnityEngine;

public class ArmMelee : Arms
{
    public void Attack(HealthEnemy target)
    {
        if (target != null)
        {
            RemoveHealth(target);
        }
    }
}
