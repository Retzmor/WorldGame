using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private PlayerInput playerInput;
    private Animator animator;

    [SerializeField] private float speedMovement = 5f;
    [SerializeField] private float ppu = 32f; // Pixels per Unit

    private Vector2 movementDirection;
    private Vector3 realPosition; // posición acumulada sin snap

    private void Start()
    {
        animator = GetComponent<Animator>();
        playerInput = GetComponent<PlayerInput>();
        realPosition = transform.position;
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
}
