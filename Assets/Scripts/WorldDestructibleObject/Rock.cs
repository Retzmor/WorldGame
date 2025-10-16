using UnityEngine;

public class Rock : Damageable
{

    [SerializeField] Transform[] rocks;
    [SerializeField] int force;
    

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
    }

   

    //public void TakeDamage(float damage, WeaponType weapon, Vector2 HitDirection)
    //{
    //    float finalDamage = damage;
    //    if (weapon == WeaponType.Pickaxe)
    //    {
    //        finalDamage *= 3;
    //    }
    //    health -= finalDamage;
    //    if (health <= 0)
    //    {
    //        DestroyTree();
    //    }
    //}

    protected override void Death()
    {
        DestroyTree();
        Destroy(gameObject);
    }
}
