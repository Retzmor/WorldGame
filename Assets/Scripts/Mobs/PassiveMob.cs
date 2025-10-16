using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;



public class PassiveMob : Mob
{
    [Header("Flee Settings")]
    public float fleeDistance = 7f;
    public float fleeTime = 3f;
    private float wanderTimer;
    private float currentWanderInterval;
    private bool isFleeing = false;
    private Vector3 startPosition;


    protected override void Start()
    {
        base.Start();

        startPosition = transform.position;

        // Intervalo inicial aleatorio
        currentWanderInterval = Random.Range(minWanderInterval, maxWanderInterval);

        // Offset inicial aleatorio para que no todos arranquen sincronizados
        wanderTimer = Random.Range(0f, currentWanderInterval);
    }

    protected override void AIUpdate()
    {
        if (!isFleeing)
        {
            Wander();
        }

    }

    private void Wander()
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

    protected override void OnHitReaction()
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

    public override void TakeDamage(float damage, WeaponType weaponType, float knockBackValue, Vector2 HitDirection, Vector2 hitPosition)
    {
        base.TakeDamage(damage, weaponType, knockBackValue, HitDirection, hitPosition);
        OnHitReaction();
    }


}
