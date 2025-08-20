using UnityEngine;


public class ArmMelee : Weapon
{
    [SerializeField] private Vector2 SizeRange;
    [SerializeField] private float offsetX;
    [SerializeField] private float offsetY;
    [SerializeField] private LayerMask LayerEnemy;
    [SerializeField] WeaponType WeaponType;

    public override void Attack()
    {
        if (Time.time - lastShotTime < 1f / fireRate)
            return;

        lastShotTime = Time.time;


        // Offset en espacio local
        Vector2 localOffset = new Vector2(offsetX, offsetY);

        // Convertimos el offset local a posición global aplicando la rotación
        Vector2 positionDetect = (Vector2)transform.position + (Vector2)(transform.rotation * localOffset);

        float angle = transform.eulerAngles.z;
        Collider2D[] zone = Physics2D.OverlapBoxAll(positionDetect, SizeRange, angle, LayerEnemy);

        AnimationAttack();

        foreach (var collider in zone)
        {
            if (collider.TryGetComponent(out IHit hit))
            {
                Vector2 hitDirection = (collider.transform.position - transform.position).normalized;
                hit.TakeDamage(damage, WeaponType, hitDirection);
                Debug.Log("ATACA");
            }
        }

    }

    public override void AnimationAttack()
    {
        if (animator != null)
            animator.SetTrigger("Attack");

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX("Hit");
        else
            Debug.LogWarning("AudioManager.Instance es null");
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Matrix4x4 oldMatrix = Gizmos.matrix;

        Vector2 localOffset = new Vector2(offsetX, offsetY);
        Vector2 worldOffset = (Vector2)(transform.rotation * localOffset);

        Gizmos.matrix = Matrix4x4.TRS(transform.position + (Vector3)worldOffset, transform.rotation, Vector3.one);

        Gizmos.DrawWireCube(Vector3.zero, SizeRange);

        Gizmos.matrix = oldMatrix;
    }
}
