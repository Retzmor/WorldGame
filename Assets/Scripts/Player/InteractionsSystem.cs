using UnityEngine;

public class InteractionsSystem : MonoBehaviour
{
    [SerializeField] float radius;
    [SerializeField] LayerMask layers;
    [SerializeField] Collider2D[] collisions;
    [SerializeField] GameObject TextInteraction;

    private void Update()
    {
        collisions = Physics2D.OverlapCircleAll(transform.position, radius, layers);

        if (collisions.Length > 0)
        {
            TextInteraction.SetActive(true);

            // Si presiona E, interact�a
            if (Input.GetKeyDown(KeyCode.E))
            {
                InteractWithItems();
            }
        }
        else
        {
            TextInteraction.SetActive(false);
        }
    }
    public void InteractWithItems()
    {
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
