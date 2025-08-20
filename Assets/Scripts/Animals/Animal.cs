using UnityEngine;

public class Animal : MonoBehaviour, IHit
{
    [SerializeField] float health = 100;
    [SerializeField] Transform[] meats;
    [SerializeField] int force;
    private DamageFlash DamageFlash;

    private void Start()
    {
        DamageFlash = GetComponent<DamageFlash>();
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
        Death();
    }

    public void Death()
    {
        gameObject.SetActive(false);
    }

    public void TakeDamage(float damage, WeaponType weapon)
    {
        DamageFlash.CallDamageFlash();
        health -= damage;
        if (health <= 0)
        {
            DestroyPig();
        }
    }
}
