using UnityEngine;

public class LootDropper : MonoBehaviour
{
    [SerializeField] Transform[] lootItems;
    [SerializeField] int force;

    public void DropLoot()
    {
        foreach (Transform item in lootItems)
        {
            item.SetParent(null);
            item.gameObject.SetActive(true);

            if (item.TryGetComponent(out Rigidbody2D rb))
            {
                Vector2 dir = (item.position - transform.position).normalized;
                rb.AddForce(dir * force, ForceMode2D.Impulse);
                rb.linearDamping = 5f;
            }
        }
    }
}
