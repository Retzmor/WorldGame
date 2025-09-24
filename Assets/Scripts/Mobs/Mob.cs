using UnityEngine;
using Pathfinding;


public abstract class Mob : Damageable
{
    [Header("Stats")]
    [SerializeField] protected float maxHealth = 100;
    protected float currentHealth;

    [Header("AI Refs")]
    protected AIPath aiPath;
    protected AIDestinationSetter destinationSetter;
    protected Transform player;

    [Tooltip("Only the hostile mobs")]
    public float loseRange = 12f;

    [Header("Wander Settings")]
    public float wanderRadius = 5f;
    public float minWanderInterval = 2f;
    public float maxWanderInterval = 5f;



     [SerializeField] private bool reverseFlip;

    // se obtiene desde world generator
    [HideInInspector] public WorldGenerator world;
    protected virtual void Awake()
    {
        currentHealth = maxHealth;
        aiPath = GetComponent<AIPath>();
        destinationSetter = GetComponent<AIDestinationSetter>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    protected virtual void Update()
    {
        AIUpdate();

        if (world != null)
            world.ReassignMobChunk(gameObject);

        HandleFlip();
    }

    // 👉 Estos métodos son virtuales para que las clases hijas los personalicen
    protected virtual void AIUpdate() { }
    protected virtual void OnAttack() { }
    protected virtual void OnHitReaction() { }


    void HandleFlip()
    {
        if (reverseFlip)
        {
            if (aiPath.desiredVelocity.x < -0.01f)
            {
                transform.localScale = new Vector3(1, 1, 1);
            }
            // Si se mueve hacia la derecha (x > 0) → normal
            else if (aiPath.desiredVelocity.x > 0.01f)
            {
                transform.localScale = new Vector3(-1, 1, 1);
            }
        }
        else
        {
            if (aiPath.desiredVelocity.x < -0.01f)
            {
                transform.localScale = new Vector3(-1, 1, 1);
            }
            // Si se mueve hacia la derecha (x > 0) → normal
            else if (aiPath.desiredVelocity.x > 0.01f)
            {
                transform.localScale = new Vector3(1, 1, 1);
            }
        }
        // Si se mueve hacia la izquierda (x < 0) → flip
        
    }

}
