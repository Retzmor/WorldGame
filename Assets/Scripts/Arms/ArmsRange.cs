using UnityEngine;

public class ArmsRange : Arms
{
    public void Attack(HealthEnemy target)
    {
        if (target != null)
        {
            RemoveHealth(target);
        }
    }
}
