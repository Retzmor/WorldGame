using UnityEngine;
using UnityEngine.AI;
public class Enemies : MonoBehaviour, IHit
{
    NavMeshAgent _agent;
    [SerializeField] GameObject _gameObject;
    private int _damage;

    [SerializeField] float _health;
    [SerializeField] float _currentHealth;

    public int Damage { get => _damage; set => _damage = value; }

    public void Death(float health)
    {
        Damage = 10;
        Debug.Log(Damage);
        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }

    public void TakeDamage(float damage)
    {
        _health -= damage;
        Death(_health);
    }

    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _agent.updateRotation = false;
        _agent.updateUpAxis = false;
    }

    // Update is called once per frame
    void Update()
    {
        _agent.SetDestination(_gameObject.transform.position);
    }
}
