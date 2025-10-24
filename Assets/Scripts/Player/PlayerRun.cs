using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerRun : MonoBehaviour
{
    PlayerInput playerInput;
    PlayerEnergy playerEnergy;
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;

    public float CurrentSpeed { get; private set; }
    private Vector2 moveInput;
    private bool isRunning;

    private void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        playerEnergy = GetComponent<PlayerEnergy>();
    }

    private void Update()
    {
        moveInput = playerInput.actions["Mover"].ReadValue<Vector2>();
        bool run = playerInput.actions["Correr"].IsPressed();

        if (moveInput.magnitude > 0.1f && run && playerEnergy.HasEnergy)
        {
            isRunning = true;
            CurrentSpeed = runSpeed;
            playerEnergy.ConsumeWhileRunning(isRunning);
        }
        else
        {
            if (isRunning)
            {
                isRunning = false;
                playerEnergy.ConsumeWhileRunning(false);
            }

            CurrentSpeed = walkSpeed;
        }
    }

    public bool IsRunning() => isRunning;
}
