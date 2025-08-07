using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class ControlesJugador : MonoBehaviour
{
    Rigidbody2D rb;
    [SerializeField] PlayerInput player;
     Vector2 movementDirection;
    [SerializeField] float speedMovement;
    InputAction accionMovimiento;
    PlayerInput playerInput;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerInput = GetComponent<PlayerInput>();
    }

    private void Update()
    {
        movementDirection = playerInput.actions["Mover"].ReadValue<Vector2>();
        if (movementDirection == Vector2.zero)
        {
            movementDirection = Vector2.zero;
        }
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = movementDirection * speedMovement;
    }
}
