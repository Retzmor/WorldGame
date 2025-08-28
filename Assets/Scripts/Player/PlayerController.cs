using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class PlayerController : Damageable
{
    private PlayerInput playerInput;
    private Animator animator;

    [SerializeField] private float speedMovement = 5f;
    [SerializeField] private float ppu = 32f; // Pixels per Unit

    private Vector2 movementDirection;
    private Vector3 realPosition; // posición acumulada sin snap
    

    protected override void Start()
    {
        base.Start(); // llama a Damageable.Start()
        // Aquí tu lógica específica de Animal
        animator = GetComponent<Animator>();
        playerInput = GetComponent<PlayerInput>();
        realPosition = transform.position;
    }

    private void Update()
    {
        // Leer input
        movementDirection = playerInput.actions["Mover"].ReadValue<Vector2>();

        // Normalizar si excede magnitud 1
        if (movementDirection.magnitude > 1f)
            movementDirection.Normalize();

        // Actualizar animación
        animator.SetFloat("Speed", movementDirection.magnitude);

        // SOLO mover si hay input
        if (movementDirection != Vector2.zero)
        {
            realPosition = transform.position; // sincroniza antes de mover
            realPosition += (Vector3)(movementDirection * speedMovement * Time.deltaTime);
            transform.position = realPosition;
        }
        else
        {
            // si no hay input, no toques transform.position
            realPosition = transform.position;
        }
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Vector2 hitDirection = (collision.transform.position - transform.position).normalized;
            TakeDamage(1, WeaponType.Sword, -hitDirection);
            if (damageFlash == null) 
            {
                Debug.Log("No hay damage flash");
            }
        }
    }

   
    

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Item"))
        {
            collision.gameObject.SetActive(false);
        }
    }

    protected override void Death()
    {
       
    }
}
