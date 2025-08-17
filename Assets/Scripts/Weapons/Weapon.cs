using UnityEngine;
public abstract class Weapon : MonoBehaviour
{
    public float damage;
    public float fireRate; // Cadencia (disparos por segundo)
    public float lastShotTime;
    [SerializeField] protected Animator animator;
    public abstract void Attack();
    public abstract void AnimationAttack();


    private void Start()
    {
        animator = GetComponent<Animator>();
    }

}