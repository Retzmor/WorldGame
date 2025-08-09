using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class PlayerController : MonoBehaviour
{
    Rigidbody2D rb;
    [SerializeField] PlayerInput player;
     Vector2 movementDirection;
    [SerializeField] float speedMovement;
    InputAction accionMovimiento;
    PlayerInput playerInput;
    private Animator animator;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        playerInput = GetComponent<PlayerInput>();
    }

    private void Update()
    {
        movementDirection = playerInput.actions["Mover"].ReadValue<Vector2>();
        animator.SetFloat("Speed", movementDirection.magnitude);
        if (movementDirection == Vector2.zero)
        {
            movementDirection = Vector2.zero;
        }
        rb.linearVelocity = (movementDirection * speedMovement) *Time.deltaTime ;
    }

    //private void FixedUpdate()
    //{
    //    rb.linearVelocity = movementDirection * speedMovement;
    //}
}
