using UnityEngine;

public class Pig : MonoBehaviour, IHit
{
    [SerializeField] float health = 100;
    [SerializeField] Transform[] meats;
    [SerializeField] int force;
    public void TakeDamage(float damage)
    {
        health -= damage;
        if(health <= 0)
        {
            DestroyPig();
        }
    }


    public void DestroyPig()
    {
        foreach (Transform meet in meats)
        {
            meet.SetParent(null);
            meet.gameObject.SetActive(true);

            Vector2 direction = meet.transform.position - transform.position;

            Vector2 randomOffset = new Vector2(
            Random.Range(-0.5f, 0.5f),
            Random.Range(-0.5f, 0.5f)
        );

            direction += randomOffset;
            direction.Normalize();

            if (meet.TryGetComponent(out Rigidbody2D rb))
            {
                rb.AddForce(direction * force, ForceMode2D.Impulse);
                rb.linearDamping = 5f;
            }
        }
        Death(health);
    }

    public void Death(float health)
    {
        gameObject.SetActive(false);
    }
}
