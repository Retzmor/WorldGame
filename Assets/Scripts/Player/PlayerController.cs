using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour, IHit
{
    private PlayerInput playerInput;
    private Animator animator;

    [SerializeField] private float speedMovement = 5f;
    [SerializeField] private float ppu = 32f; // Pixels per Unit

    private Vector2 movementDirection;
    private Vector3 realPosition; // posición acumulada sin snap

    private DamageFlash damageFlash;

    public void Death(float health)
    {
        throw new System.NotImplementedException();
    }

    public void TakeDamage(float damage)
    {
        damageFlash.CallDamageFlash();
    }

    private void Start()
    {
        animator = GetComponent<Animator>();
        playerInput = GetComponent<PlayerInput>();
        realPosition = transform.position;
        damageFlash = GetComponent<DamageFlash>();
    }

    private void Update()
    {
        // 1. Leer input
        movementDirection = playerInput.actions["Mover"].ReadValue<Vector2>();

        // 2. Normalizar si excede magnitud 1
        if (movementDirection.magnitude > 1f)
            movementDirection.Normalize();

        // 3. Actualizar animación
        animator.SetFloat("Speed", movementDirection.magnitude);

        // 4. Mover en posición real (sin snap)
        realPosition += (Vector3)(movementDirection * speedMovement * Time.deltaTime);

        // 5. Aplicar pixel snapping solo al render final
        Vector3 snappedPosition = realPosition;
        snappedPosition.x = Mathf.Round(snappedPosition.x * ppu) / ppu;
        snappedPosition.y = Mathf.Round(snappedPosition.y * ppu) / ppu;

        transform.position = snappedPosition;
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            TakeDamage(0);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Item"))
        {
            collision.gameObject.SetActive(false);
        }
    }

    public void TakeDamage(float damage, WeaponType weapon, Vector2 HitDir)
    {
        throw new System.NotImplementedException();
    }

    public void Death()
    {
        throw new System.NotImplementedException();
    }
}
