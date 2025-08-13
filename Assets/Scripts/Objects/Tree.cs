using UnityEngine;

public class Tree : MonoBehaviour, IHit
{
    [SerializeField] float health = 10f;            // Salud inicial
    [SerializeField] Transform[] woods;             // Fragmentos que salen volando
    [SerializeField] int force = 5;                 // Fuerza de impulso al romperse

    public void Death(float health)
    {
        DestroyTree();
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Death(health);
        }
    }

    private void DestroyTree()
    {
        foreach (Transform wood in woods)
        {
            // Separa el fragmento y act�valo
            wood.SetParent(null);
            wood.gameObject.SetActive(true);

            // Calcula direcci�n de salida + un peque�o desplazamiento aleatorio
            Vector2 direction = wood.transform.position - transform.position;
            Vector2 randomOffset = new Vector2(
                Random.Range(-0.5f, 0.5f),
                Random.Range(-0.5f, 0.5f)
            );

            direction += randomOffset;
            direction.Normalize();

            // Si tiene Rigidbody2D, aplica fuerza y activa interpolaci�n
            if (wood.TryGetComponent(out Rigidbody2D rb))
            {
                rb.interpolation = RigidbodyInterpolation2D.Interpolate; // <-- Evita jitter
                rb.linearDamping = 5f;                                   // Frena el movimiento
                rb.linearVelocity = Vector2.zero;                              // Reinicia velocidad previa
                rb.angularVelocity = 0f;                                 // Sin giros previos
                rb.AddForce(direction * force, ForceMode2D.Impulse);     // Aplica impulso
            }
        }

        // Destruye el �rbol original
        Destroy(gameObject);
    }
}
