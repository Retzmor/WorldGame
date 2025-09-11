using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float smoothTime = 0.1f; // suavizado de movimiento

    [Header("Juice")]
    [SerializeField] private float squashAmount = 0.2f;    // cuánto se aplasta
    [SerializeField] private float squashDuration = 0.15f; // tiempo del squash
    [SerializeField] private float bobAmount = 0.1f;       // magnitud del bob (en unidades world)
    [SerializeField] private float bobSpeed = 6f;          // velocidad oscilación del bob

    private PlayerInput playerInput;
    private Vector2 inputDir;
    private Vector3 currentVelocity;     // velocidad suavizada que usamos para mover
    private Vector3 velocitySmoothing;   // buffer para SmoothDamp
    private Vector3 baseScale;
    private Tween squashTween;
    private Animator animator;
    private bool wasMoving = false; // 👈 nuevo flag

    // Estado para aplicar el bob correctamente (vectorial)
    private Vector3 lastBobOffset = Vector3.zero;


    [Header("Ghost Trail")]
    [SerializeField] private GhostTrail ghostPrefab;  // referencia al prefab con GhostTrail
    [SerializeField] private float ghostInterval = 0.05f; // cada cuánto aparece un ghost
    [SerializeField] private Color ghostColor = new Color(1, 1, 1, 0.6f);
    [SerializeField] private bool enableGhost;

    private float ghostTimer;
    private SpriteRenderer spriteRenderer;

    [Header("Effects")]
    [SerializeField] private ParticleSystem dustParticles; // 👈 referencia a partículas

    private void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        animator = GetComponent<Animator>();
        baseScale = transform.localScale;
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void Update()
    {
        inputDir = playerInput.actions["Mover"].ReadValue<Vector2>();
        if (inputDir.sqrMagnitude > 1f) inputDir.Normalize();

        if (animator != null) animator.SetFloat("Speed", inputDir.magnitude);
    }

    private void LateUpdate()
    {
        // --- Movimiento (suavizado de velocidad) ---
        Vector3 targetVelocity = (Vector3)(inputDir * moveSpeed);
        currentVelocity = Vector3.SmoothDamp(currentVelocity, targetVelocity, ref velocitySmoothing, smoothTime);
        transform.position += currentVelocity * Time.deltaTime;

        bool isMoving = inputDir.magnitude > 0.1f;
        // --- Squash & Stretch ---
        if (inputDir.magnitude > 0.1f && spriteRenderer != null)
        {
            if (squashTween == null || !squashTween.IsActive())
            {
                Vector3 squashScale = new Vector3(baseScale.x + squashAmount, baseScale.y - squashAmount, baseScale.z);
                squashTween = transform.DOScale(squashScale, squashDuration)
                    .SetLoops(2, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine);
            }

            ghostTimer -= Time.deltaTime;
            if (ghostTimer <= 0f && enableGhost)
            {
                SpawnGhost();
                ghostTimer = ghostInterval;

            }
        }
        else
        {
            transform.localScale = Vector3.Lerp(transform.localScale, baseScale, Time.deltaTime * 5f);
        }

        if (isMoving && !wasMoving)  // empezó a moverse
        {
            CreateDust();
        }
        else if (!isMoving && wasMoving) // dejó de moverse
        {
            CreateDust();
        }

        wasMoving = isMoving;

        // --- Bobbing perpendicular al movimiento (corregido) ---
        // 1) quitamos el bob anterior (basePos)
        Vector3 basePos = transform.position - lastBobOffset;

        // 2) calculamos el vector de movimiento 2D a usar (preferimos currentVelocity, fallback a inputDir)
        Vector2 move2D = new Vector2(currentVelocity.x, currentVelocity.y);
        float moveMag = move2D.magnitude;
        Vector2 dir2D = (moveMag > 0.01f) ? move2D.normalized : inputDir;

        Vector3 newBobOffset = Vector3.zero;

        if (dir2D.sqrMagnitude > 0.0001f)
        {
            // perpendicular en XY: (-y, x)
            Vector2 perp = new Vector2(-dir2D.y, dir2D.x);

            // opcional: escalar bob según velocidad (para que a baja velocidad sea más sutil)
            float speedFactor = (moveMag > 0.01f) ? Mathf.Clamp01(moveMag / moveSpeed) : 0f;

            float bobScalar = Mathf.Sin(Time.time * bobSpeed) * bobAmount * speedFactor;
            Vector2 bobOffset2D = perp * bobScalar;

            newBobOffset = new Vector3(bobOffset2D.x, bobOffset2D.y, 0f);
        }
        else
        {
            // cuando no hay movimiento, suavizamos la vuelta a 0
            newBobOffset = Vector3.Lerp(lastBobOffset, Vector3.zero, Time.deltaTime * 8f);
        }

        // 3) aplicamos el nuevo bob y guardamos
        transform.position = basePos + newBobOffset;
        lastBobOffset = newBobOffset;
    }
    private void SpawnGhost()
    {
        GhostTrail ghost = GhostTrail.GetGhost(ghostPrefab);
        ghost.Init(spriteRenderer.sprite, transform.position, transform.localScale, ghostColor);
    }

    private void CreateDust()
    {
        dustParticles.Play();
    }
}
