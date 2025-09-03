using UnityEngine;

public class Arrow : MonoBehaviour
{
    float damage = 10f;
    float velocity = 5;
    [SerializeField] private WeaponType weaponType;
    [SerializeField] private float knockBack;
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
        if(collision.gameObject.CompareTag("Enemy") || collision.CompareTag("Animal") || collision.CompareTag("Mob"))
        {
            gameObject.SetActive(false);
            Vector2 hitDir = (collision.transform.position - transform.position).normalized;
            Debug.Log("enemigo");
            if (collision.TryGetComponent<Damageable>(out var hit))
            {
                hit.TakeDamage(damage, weaponType, knockBack , hitDir);
                
            }
            else
            {
                
            }
        }
    }
}
