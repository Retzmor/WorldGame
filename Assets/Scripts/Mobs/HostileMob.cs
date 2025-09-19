using UnityEngine;

public class HostileMob : Mob
{
    [Header("Combat")]
    [SerializeField] private float detectionRange = 6f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float loseRangeTarget = 12f;

    public bool IsAggro { get; private set; }


    protected override void AIUpdate()
    {
        if (player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);

        // 👀 Detectar al jugador
        if (!IsAggro && dist <= detectionRange)
        {
            IsAggro = true;
            destinationSetter.target = player;
            if (aiPath != null) aiPath.isStopped = false;
        }
        else if (IsAggro && dist > loseRange)
        {
            IsAggro = false;
            destinationSetter.target = null;
            if (aiPath != null) aiPath.isStopped = true;
        }

        if (IsAggro && destinationSetter.target != null)
            destinationSetter.target = player;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, loseRange);
    }
}
