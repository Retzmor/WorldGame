using UnityEngine;

public class Arms : MonoBehaviour
{
    [SerializeField] HealthEnemy healthEnemy;
    protected int damage = 5;
    float radius;

    public void RemoveHealth(HealthEnemy healthEnemy)
    {
            healthEnemy.TakeDamage(damage);
    }
}
