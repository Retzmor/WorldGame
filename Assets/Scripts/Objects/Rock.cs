using UnityEngine;

public class Rock : MonoBehaviour, IHit
{
    [SerializeField] float health;
    [SerializeField] Transform[] rocks;
    [SerializeField] int force;
    public void TakeDamage(float damage, Weapon weapon)
    {

        float finalDamage = damage;
        if (weapon == Weapon.Pickaxe)
        {
            finalDamage *= 3;
        }
          health -= finalDamage;
        if (health <= 0)
        {
            DestroyTree();
        }
    }

    public void Death()
    {
        Destroy(gameObject);
    }
    public void DestroyTree()
    {
        foreach (Transform rock in rocks)
        {
            rock.SetParent(null);
            rock.gameObject.SetActive(true);

            Vector2 direction = rock.transform.position - transform.position;

            Vector2 randomOffset = new Vector2(
            Random.Range(-0.5f, 0.5f),
            Random.Range(-0.5f, 0.5f)
        );

            direction += randomOffset;
            direction.Normalize();

            if (rock.TryGetComponent(out Rigidbody2D rb))
            {
                rb.AddForce(direction * force, ForceMode2D.Impulse);
                rb.linearDamping = 5f;
            }
        }
        Death();
    }
}
