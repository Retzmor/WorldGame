using DG.Tweening;
using UnityEngine;

public class Animal : Damageable
{ 
    [SerializeField] Transform[] meats;
    [SerializeField] int force;
    private DamageFlash DamageFlash;

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
    gameObject.SetActive(false);
       
    }


    protected override void Death()
    {
      DestroyPig();
    }
}
