using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float smoothTime = 0.1f;

    [Header("Visuals (usar hijo con SpriteRenderer)")]
    [SerializeField] private Transform spriteTransform; // 👈 referencia al hijo visual
    [SerializeField] private float squashAmount = 0.2f;
    [SerializeField] private float squashDuration = 0.15f;
    [SerializeField] private float bobAmount = 0.1f;
    [SerializeField] private float bobSpeed = 6f;

    [Header("Ghost Trail")]
    [SerializeField] private GhostTrail ghostPrefab;
    [SerializeField] private float ghostInterval = 0.05f;
    [SerializeField] private Color ghostColor = new Color(1, 1, 1, 0.6f);
    [SerializeField] private bool enableGhost;

    [Header("Effects")]
    [SerializeField] private ParticleSystem dustParticles;

    private PlayerInput playerInput;
    private Rigidbody2D rb;
    [SerializeField] private Animator animator;
    private SpriteRenderer spriteRenderer;

    private Vector2 inputDir;
    private Vector2 currentVelocity;
    private Vector2 velocitySmoothing;
    private Vector3 baseScale;
    private Tween squashTween;
    private float ghostTimer;
    private bool wasMoving;

    private Vector3 lastBobOffset = Vector3.zero;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerInput = GetComponent<PlayerInput>();
        //animator = GetComponentInChildren<Animator>();
        spriteRenderer = spriteTransform.GetComponent<SpriteRenderer>();

        baseScale = spriteTransform.localScale;
    }

    private void Update()
    {
        inputDir = playerInput.actions["Mover"].ReadValue<Vector2>();
        if (inputDir.sqrMagnitude > 1f) inputDir.Normalize();

        if (animator != null)
            animator.SetFloat("Speed", inputDir.magnitude);
    }

    private void FixedUpdate()
    {
        // --- Movimiento físico ---
        Vector2 targetVelocity = inputDir * moveSpeed;
        currentVelocity = Vector2.SmoothDamp(currentVelocity, targetVelocity, ref velocitySmoothing, smoothTime);

        rb.MovePosition(rb.position + currentVelocity * Time.fixedDeltaTime);

        // Detectar inicio o fin de movimiento (para polvo)
        bool isMoving = inputDir.magnitude > 0.1f;
        if (isMoving && !wasMoving) CreateDust();
        if (!isMoving && wasMoving) CreateDust();
        wasMoving = isMoving;
    }

    private void LateUpdate()
    {
        bool isMoving = inputDir.magnitude > 0.1f;

        // --- Squash & Stretch ---
        if (isMoving && spriteRenderer != null)
        {
            if (squashTween == null || !squashTween.IsActive())
            {
                Vector3 squashScale = new Vector3(baseScale.x + squashAmount, baseScale.y - squashAmount, baseScale.z);
                squashTween = spriteTransform.DOScale(squashScale, squashDuration)
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
            spriteTransform.localScale = Vector3.Lerp(spriteTransform.localScale, baseScale, Time.deltaTime * 5f);
        }

        // --- Bobbing ---
        Vector3 basePos = spriteTransform.localPosition - lastBobOffset;
        Vector2 move2D = currentVelocity;
        float moveMag = move2D.magnitude;
        Vector2 dir2D = (moveMag > 0.01f) ? move2D.normalized : inputDir;

        Vector3 newBobOffset = Vector3.zero;
        if (dir2D.sqrMagnitude > 0.0001f)
        {
            Vector2 perp = new Vector2(-dir2D.y, dir2D.x);
            float speedFactor = Mathf.Clamp01(moveMag / moveSpeed);
            float bobScalar = Mathf.Sin(Time.time * bobSpeed) * bobAmount * speedFactor;
            Vector2 bobOffset2D = perp * bobScalar;
            newBobOffset = new Vector3(bobOffset2D.x, bobOffset2D.y, 0f);
        }
        else
        {
            newBobOffset = Vector3.Lerp(lastBobOffset, Vector3.zero, Time.deltaTime * 8f);
        }

        spriteTransform.localPosition = basePos + newBobOffset;
        lastBobOffset = newBobOffset;
    }

    private void SpawnGhost()
    {
        GhostTrail ghost = GhostTrail.GetGhost(ghostPrefab);
        ghost.Init(spriteRenderer.sprite, spriteTransform.position, spriteTransform.localScale, ghostColor);
    }

    private void CreateDust()
    {
        if (dustParticles != null) dustParticles.Play();
    }
}
