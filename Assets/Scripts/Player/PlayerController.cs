using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb;
    private PlayerInput playerInput;
    private Animator animator;

    [SerializeField] private float speedMovement = 5f;

    private Vector2 movementDirection;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        playerInput = GetComponent<PlayerInput>();
    }

    private void Update()
    {
        // Lee el input
        movementDirection = playerInput.actions["Mover"].ReadValue<Vector2>();

        // Actualiza animaci�n
        animator.SetFloat("Speed", movementDirection.magnitude);
    }

    private void FixedUpdate()
    {
        // Aplica movimiento f�sico
        rb.linearVelocity = movementDirection * speedMovement;
    }
}
