using UnityEngine;

public class Tree : MonoBehaviour, IHit
{
    [SerializeField] float health;
    [SerializeField] Transform[] woods;
    [SerializeField] int force;
    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            DestroyTree();
        } 
    }

    public void DestroyTree()
    {
        foreach(Transform wood in woods)
        {
            wood.SetParent(null);
            wood.gameObject.SetActive(true);

            Vector2 direction = wood.transform.position - transform.position;

            Vector2 randomOffset = new Vector2(
            Random.Range(-0.5f, 0.5f),
            Random.Range(-0.5f, 0.5f)
        );

            direction += randomOffset;
            direction.Normalize();

            if (wood.TryGetComponent(out Rigidbody2D rb))
            {
                rb.AddForce(direction * force, ForceMode2D.Impulse);
                rb.linearDamping = 5f;
            }
        }
        Destroy(gameObject);
    }
}
