using System.Collections;
using UnityEngine;
using Pathfinding;

public class MobAI : MonoBehaviour
{
    [Header("Refs")]
    public AIPath aiPath;
    public AIDestinationSetter destinationSetter;

    [Header("Wander Settings")]
    public float wanderRadius = 5f;
    public float minWanderInterval = 2f;
    public float maxWanderInterval = 5f;

    [Header("Flee Settings")]
    public float fleeDistance = 7f;
    public float fleeTime = 3f;

    private Transform player;
    private Vector3 startPosition;
    private float wanderTimer;
    private float currentWanderInterval;
    private bool isFleeing = false;

    [HideInInspector] public WorldGenerator world;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        startPosition = transform.position;

        // Intervalo inicial aleatorio
        currentWanderInterval = Random.Range(minWanderInterval, maxWanderInterval);

        // Offset inicial aleatorio para que no todos arranquen sincronizados
        wanderTimer = Random.Range(0f, currentWanderInterval);
    }

    void Update()
    {
        if (!isFleeing)
        {
            Wander();
        }

        if (world != null)
            world.ReassignMobChunk(gameObject);

        HandleFlip();
    }

    void Wander()
    {
        wanderTimer += Time.deltaTime;

        if (wanderTimer >= currentWanderInterval)
        {
            // Nueva posición aleatoria alrededor del punto de inicio
            Vector3 randomPos = startPosition + new Vector3(
                Random.Range(-wanderRadius, wanderRadius),
                Random.Range(-wanderRadius, wanderRadius),
                0
            );

            destinationSetter.target = null; // no seguir al jugador
            aiPath.destination = randomPos;

            wanderTimer = 0f;

            // Nuevo intervalo aleatorio para la siguiente vez
            currentWanderInterval = Random.Range(minWanderInterval, maxWanderInterval);
        }
    }
    void HandleFlip()
    {
        // Si se mueve hacia la izquierda (x < 0) → flip
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

    public void OnHit()
    {
        if (!isFleeing)
            StartCoroutine(Flee());
    }

    IEnumerator Flee()
    {
        isFleeing = true;

        float timer = 0f;

        // Tiempo de huida aleatorio en torno al base
        float randomFleeTime = Random.Range(fleeTime * 0.8f, fleeTime * 1.2f);

        while (timer < randomFleeTime)
        {
            // Siempre calcula la dirección opuesta al jugador
            Vector3 fleeDir = (transform.position - player.position).normalized;
            Vector3 fleeTarget = transform.position + fleeDir * fleeDistance;

            aiPath.destination = fleeTarget;

            timer += Time.deltaTime;
            yield return null;
        }

        isFleeing = false;
    }
}
