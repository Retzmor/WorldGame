using UnityEngine;

public class Arrow : MonoBehaviour
{
    float damage = 10f;
    float velocity = 3;

    private void Start()
    {
        Destroy(gameObject, 5f);
    }

    private void Update()
    {
        transform.position += transform.right * velocity * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Enemy") || collision.CompareTag("Animal"))
        {
            gameObject.SetActive(false);
            Vector2 hitDir = (collision.transform.position - transform.position).normalized;

            if (collision.TryGetComponent<IHit>(out var hit))
            {
                hit.TakeDamage(damage, WeaponType.Arrow, hitDir);
            }
        }
    }
}
