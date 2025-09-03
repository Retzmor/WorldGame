using UnityEngine;

public class InteractionsSystem : MonoBehaviour
{
    [SerializeField] float radius;
    [SerializeField] LayerMask layers;
    [SerializeField] Collider2D[] collisions;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            InteractWithItems();
        }
    }
    public void InteractWithItems()
    {
        collisions = Physics2D.OverlapCircleAll(transform.position, radius, layers);

        if (collisions.Length > 0)
        {
            foreach (var item in collisions)
            {
                if (item.TryGetComponent(out IInteractuable interactuable))
                {
                    interactuable.Interact();
                }
            }
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
