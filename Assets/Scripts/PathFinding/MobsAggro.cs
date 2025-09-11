using Pathfinding;
using UnityEngine;

public class MobsAggro : MonoBehaviour
{
    [Header("Detección del jugador")]
    public Transform player;
    public float detectionRange = 8f;
    public float loseRange = 12f;

    private AIDestinationSetter setter;
    private AIPath aiPath;

    public bool IsAggro { get; private set; }
    [HideInInspector] public WorldGenerator world;

    void Awake()
    {
        setter = GetComponent<AIDestinationSetter>();
        aiPath = GetComponent<AIPath>();
    }

    void Update()
    {
        if (player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);

        // 👀 Detectar al jugador
        if (!IsAggro && dist <= detectionRange)
        {
            IsAggro = true;
            setter.target = player;
            if (aiPath != null) aiPath.isStopped = false;
        }
        else if (IsAggro && dist > loseRange)
        {
            IsAggro = false;
            setter.target = null;
            if (aiPath != null) aiPath.isStopped = true;
        }

        if (IsAggro && setter.target != null)
            setter.target = player;

        // 👇 Reasignación automática al chunk actual
        if (world != null)
            world.ReassignMobChunk(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, loseRange);
    }
}
