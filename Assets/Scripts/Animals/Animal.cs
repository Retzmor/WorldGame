using DG.Tweening;
using UnityEngine;

public class Animal : MonoBehaviour, IHit
{
    [SerializeField] float health = 100;
    [SerializeField] Transform[] meats;
    [SerializeField] int force;
    private DamageFlash DamageFlash;
    private Rigidbody2D rb;
    private void Start()
    {
        DamageFlash = GetComponent<DamageFlash>();
        rb = GetComponent<Rigidbody2D>();
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

    public void TakeDamage(float damage, WeaponType weapon, Vector2 HitDir)
    {
        DamageFlash.CallDamageFlash();

        float knockbackDistance = 0.5f;  // cuánto se mueve
        float knockbackDuration = 0.15f; // qué tan rápido vuelve
        transform.DOKill(); // cancelar tweens previos en este objeto
        transform.DOMove((Vector2)transform.position + HitDir * knockbackDistance, knockbackDuration)
                 .SetEase(Ease.OutQuad);

        Debug.Log("ola");

        health -= damage;
        if (health <= 0)
        {
            DestroyPig();
        }
    }
}
