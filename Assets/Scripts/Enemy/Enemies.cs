using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
public class Enemies : Damageable
{
    NavMeshAgent _agent;
    [SerializeField] GameObject _gameObject;
    [SerializeField] private int _damage;
    public int Damage { get => _damage; set => _damage = value; }


    protected override void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _agent.updateRotation = false;
        _agent.updateUpAxis = false;
        base.Start(); // llama a Damageable.Start()
        // Aquí tu lógica específica de Animal
    }


    void Update()
    {
        _agent.SetDestination(_gameObject.transform.position);

        Vector3 direction = _gameObject.transform.position - transform.position;

        if (direction.x < 0)
            transform.localScale = new Vector3(1, 1, 1); 
        else if (direction.x > 0)
            transform.localScale = new Vector3(-1, 1, 1);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Damageable player = collision.gameObject.GetComponent<Damageable>();
            if (player != null)
            {
                Vector2 hitDir = (collision.transform.position - transform.position).normalized;

                player.TakeDamage(_damage, WeaponType.Sword, hitDir);
            }
        }
    }

    protected override void Death()
    {
        gameObject.SetActive(false);
    }
}
