using UnityEngine;

public class Arms : MonoBehaviour
{
    [SerializeField] protected HealthEnemy healthEnemy;
    public int damage = 50;

    public virtual void Attack(HealthEnemy target)
    {
        
    }
}
