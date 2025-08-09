using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class ArmMelee : Arms
{
    [SerializeField] private Vector2 SizeRange;
    [SerializeField] private float offsetX;
    [SerializeField] private float offsetY;
    [SerializeField] private LayerMask LayerEnemy;

    public override void UseWeapon()

    {  // Offset en espacio local
        Vector2 localOffset = new Vector2(offsetX, offsetY);

        // Convertimos el offset local a posición global aplicando la rotación
        Vector2 positionDetect = (Vector2)transform.position + (Vector2)(transform.rotation * localOffset);

        float angle = transform.eulerAngles.z;
        Collider2D[] zone = Physics2D.OverlapBoxAll(positionDetect, SizeRange, angle, LayerEnemy);

        foreach (var collider in zone)
        {
            if (collider.TryGetComponent(out IHit hit))
            {
                hit.TakeDamage(damage);
                Debug.Log(collider.gameObject.name);
            }
        }

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
