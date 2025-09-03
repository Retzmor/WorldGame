using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
public class Enemies : Damageable
{
    NavMeshAgent _agent;
    [SerializeField] GameObject _gameObject;
    [SerializeField] private int _damage;
    [SerializeField] Transform[] bones;
    [SerializeField] int force;
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
        if(_gameObject != null)
        {
            _agent.SetDestination(_gameObject.transform.position);

            Vector3 direction = _gameObject.transform.position - transform.position;

            if (direction.x < 0)
                transform.localScale = new Vector3(1, 1, 1);
            else if (direction.x > 0)
                transform.localScale = new Vector3(-1, 1, 1);
        }
        
    }

    //private void OnCollisionEnter2D(Collision2D collision)
    //{
    //    if (collision.gameObject.CompareTag("Player"))
    //    {
    //        Damageable player = collision.gameObject.GetComponent<Damageable>();
    //        if (player != null)
    //        {
    //            Vector2 hitDir = (collision.transform.position - transform.position).normalized;

    //            player.TakeDamage(_damage, WeaponType.Sword, hitDir);
    //        }
    //    }
    //}

    protected override void Death()
    {
        foreach (Transform bone in bones)
        {
            bone.SetParent(null);
            bone.gameObject.SetActive(true);

            Vector2 direction = bone.transform.position - transform.position;

            Vector2 randomOffset = new Vector2(
            Random.Range(-0.5f, 0.5f),
            Random.Range(-0.5f, 0.5f)
        );
            direction += randomOffset;
            direction.Normalize();

            if (bone.TryGetComponent(out Rigidbody2D rb))
            {
                rb.AddForce(direction * force, ForceMode2D.Impulse);
                rb.linearDamping = 5f;
            }
        }

        Destroy(gameObject);
    }
}
