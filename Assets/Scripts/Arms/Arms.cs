using UnityEngine;

public class Arms : MonoBehaviour
{
    [SerializeField] HealthEnemy healthEnemy;
    protected int damage = 50;

    public void RemoveHealth(HealthEnemy healthEnemy)
    {
       healthEnemy.TakeDamage(damage);
    }
}
