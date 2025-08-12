using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
public class Enemies : MonoBehaviour, IHit
{
    NavMeshAgent _agent;
    [SerializeField] GameObject _gameObject;
    private int _damage;

    [SerializeField] float _health = 100;
    [SerializeField] float _currentHealth;

    public int Damage { get => _damage; set => _damage = value; }

    void Start()
    {
        _currentHealth = _health;
        _agent = GetComponent<NavMeshAgent>();
        _agent.updateRotation = false;
        _agent.updateUpAxis = false;
    }

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

    void Update()
    {
        _agent.SetDestination(_gameObject.transform.position);

        Vector3 direction = _gameObject.transform.position - transform.position;

        if (direction.x < 0)
            transform.localScale = new Vector3(1, 1, 1); 
        else if (direction.x > 0)
            transform.localScale = new Vector3(-1, 1, 1);
    }
}
