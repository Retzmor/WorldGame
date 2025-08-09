using UnityEngine;

public class Arrow : MonoBehaviour
{
    float damage = 50f;
    float velocity = 3;

    private void Start()
    {
        Destroy(gameObject, 5f);
    }

    private void Update()
    {
        transform.Translate(Vector2.up * velocity * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Entro al trigger");
        if(collision.gameObject.CompareTag("Enemy"))
        {
            Debug.Log("En el if");
            collision.GetComponent<IHit>().TakeDamage(damage);
        }
    }
}
